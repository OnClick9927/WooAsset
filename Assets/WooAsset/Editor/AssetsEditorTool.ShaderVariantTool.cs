using UnityEditor;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Threading.Tasks;
using System.Reflection;
using UnityEditor.SceneManagement;

namespace WooAsset
{
    public partial class AssetsEditorTool
    {
        private class ShaderVariantTool
        {
            static class ShaderVariantCollectionHelper
            {
                public static object InvokeNonPublicStaticMethod(System.Type type, string method, params object[] parameters)
                {
                    var methodInfo = type.GetMethod(method, BindingFlags.NonPublic | BindingFlags.Static);
                    if (methodInfo == null)
                    {
                        UnityEngine.Debug.LogError($"{type.FullName} not found method : {method}");
                        return null;
                    }
                    return methodInfo.Invoke(null, parameters);
                }
                public static void ClearCurrentShaderVariantCollection()
                {
                    InvokeNonPublicStaticMethod(typeof(ShaderUtil), "ClearCurrentShaderVariantCollection");
                }
                public static void SaveCurrentShaderVariantCollection(string savePath)
                {
                    InvokeNonPublicStaticMethod(typeof(ShaderUtil), "SaveCurrentShaderVariantCollection", savePath);
                }
                public static int GetCurrentShaderVariantCollectionShaderCount()
                {
                    return (int)InvokeNonPublicStaticMethod(typeof(ShaderUtil), "GetCurrentShaderVariantCollectionShaderCount");
                }
                public static int GetCurrentShaderVariantCollectionVariantCount()
                {
                    return (int)InvokeNonPublicStaticMethod(typeof(ShaderUtil), "GetCurrentShaderVariantCollectionVariantCount");
                }

                /// <summary>
                /// 获取着色器的变种总数量
                /// </summary>
                public static string GetShaderVariantCount(string assetPath)
                {
                    Shader shader = AssetDatabase.LoadAssetAtPath<Shader>(assetPath);
                    var variantCount = InvokeNonPublicStaticMethod(typeof(ShaderUtil), "GetVariantCount", shader, true);
                    return variantCount.ToString();
                }
                public static void FocusUnityGameWindow()
                {
                    System.Type T = Assembly.Load("UnityEditor").GetType("UnityEditor.GameView");
                    EditorWindow.GetWindow(T, false, "GameView", true);
                }

            }
            private const float WaitMilliseconds = 2 * 1000f;
            private const float SleepMilliseconds = 2 * 100f;
            private static void CollectVariants(GameObject root, List<string> materials)
            {
                Camera camera = Camera.main;
                // 设置主相机
                float aspect = camera.aspect;
                int totalMaterials = materials.Count;
                float height = Mathf.Sqrt(totalMaterials / aspect) + 1;
                float width = Mathf.Sqrt(totalMaterials / aspect) * aspect + 1;
                float halfHeight = Mathf.CeilToInt(height / 2f);
                float halfWidth = Mathf.CeilToInt(width / 2f);
                camera.orthographic = true;
                camera.orthographicSize = halfHeight;
                camera.transform.position = new Vector3(0f, 0f, -10f);

                // 创建测试球体
                int xMax = (int)(width - 1);
                int x = 0, y = 0;
                for (int i = 0; i < materials.Count; i++)
                {
                    var material = materials[i];
                    var position = new Vector3(x - halfWidth + 1f, y - halfHeight + 1f, 0f);
                    CreateSphere(root.transform, material, position, i);

                    if (x == xMax)
                    {
                        x = 0;
                        y++;
                    }
                    else
                    {
                        x++;
                    }
                }
            }
            private static void CreateSphere(Transform root, string assetPath, Vector3 position, int index)
            {
                var material = AssetDatabase.LoadAssetAtPath<Material>(assetPath);
                var shader = material.shader;
                if (shader == null) return;
                var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                go.GetComponent<Renderer>().sharedMaterial = material;
                go.transform.position = position;
                go.name = $"Sphere_{index} | {material.name}";
                go.transform.parent = root;
            }

            public async static Task Execute()
            {
                string _savePath = AssetsEditorTool.option.shaderVariantOutputDirectory;
                if (!AssetsEditorTool.ExistsDirectory(_savePath))
                {
                    AssetsHelper.LogError("ShaderVariantDirectory Not Exist,CheckSetting");
                    return;
                }
                _savePath = AssetsHelper.ToRegularPath(AssetsHelper.CombinePath(_savePath, "shadervariants.shadervariants"));
                AssetDatabase.DeleteAsset(_savePath);
                // 聚焦到游戏窗口
                ShaderVariantCollectionHelper.FocusUnityGameWindow();
                string activePath = EditorSceneManager.GetActiveScene().path;
                // 创建临时测试场景
                EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
                await Task.Delay((int)SleepMilliseconds);
                ShaderVariantCollectionHelper.ClearCurrentShaderVariantCollection();
                await Task.Delay((int)SleepMilliseconds);
                List<string> _allMaterials = AssetDatabase.FindAssets("t:Material", option.pkgs.SelectMany(x => x.paths).Concat(option.shaderVariantInputDirectory).ToArray())
                    .ToList()
                    .ConvertAll(x => AssetDatabase.GUIDToAssetPath(x));
                await Task.Delay((int)SleepMilliseconds);
                GameObject root = new GameObject("Go");
                CollectVariants(root, _allMaterials);
                await Task.Delay((int)WaitMilliseconds);
                ShaderVariantCollectionHelper.SaveCurrentShaderVariantCollection(_savePath);
                //GameObject.DestroyImmediate(root);

                // 尝试释放编辑器加载的资源
                EditorUtility.UnloadUnusedAssetsImmediate(true);
                AssetDatabase.Refresh();
                if (!string.IsNullOrEmpty(activePath))
                    EditorSceneManager.OpenScene(activePath, OpenSceneMode.Single);
                AssetsHelper.Log("shader variant succeed");
            }
        }

    }
}
