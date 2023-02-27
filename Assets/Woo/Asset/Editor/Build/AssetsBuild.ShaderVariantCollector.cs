using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System;
using System.Linq;
using System.Diagnostics;
using UnityEditor.SceneManagement;
using System.Reflection;
using System.Threading.Tasks;

namespace WooAsset
{

    public partial class AssetsBuild
    {
        public static class ShaderVariantCollector
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
            private enum ESteps
            {
                None,
                Run
            }

            private const int WaitMilliseconds = 1000;
            private const int SleepMilliseconds = 100;
            private static string _savePath;
            private static List<string> _buildPaths;

            private static int _processMaxNum;

            private static ESteps _steps = ESteps.None;
            private static List<string> _allMaterials;
            private static GameObject root;
            static UnityEngine.SceneManagement.Scene active;


            /// <summary>
            /// 开始收集
            /// </summary>
            public static async Task Run()
            {
                if (_steps != ESteps.None) return;
                _savePath = tool.shaderVariantDirectory;
                if (!Directory.Exists(_savePath)) return;
                _savePath = AssetsInternal.ToRegularPath(AssetsInternal.CombinePath(_savePath, "shadervariants.shadervariants"));
                _buildPaths = setting.buildPaths;
                _processMaxNum = int.MaxValue;
                AssetDatabase.DeleteAsset(_savePath);
                // 聚焦到游戏窗口
                ShaderVariantCollectionHelper.FocusUnityGameWindow();
                // 创建临时测试场景
                active = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Additive);
                _steps = ESteps.Run;
                await Prepare();
            }

            private static async Task Prepare()
            {
                ShaderVariantCollectionHelper.ClearCurrentShaderVariantCollection();
                await Task.Delay(SleepMilliseconds);
                _allMaterials = GetAllMaterials();
                await Task.Delay(SleepMilliseconds);
                await CollectVariants();
            }

            private static async Task CollectVariants()
            {
                int count = Mathf.Min(_processMaxNum, _allMaterials.Count);
                List<string> range = _allMaterials.GetRange(0, count);
                _allMaterials.RemoveRange(0, count);
                CollectVariants(range);
                if (_allMaterials.Count > 0)
                {
                    await Task.Delay(SleepMilliseconds);
                    await CollectVariants();
                }
                else
                {
                    await Task.Delay(WaitMilliseconds);

                    // 保存结果并创建清单
                    ShaderVariantCollectionHelper.SaveCurrentShaderVariantCollection(_savePath);
                    GameObject.DestroyImmediate(root);

                    // 尝试释放编辑器加载的资源
                    EditorUtility.UnloadUnusedAssetsImmediate(true);
                    AssetDatabase.Refresh();
                    EditorSceneManager.CloseScene(active, true);
                    _steps = ESteps.None;
                }
            }
            private static List<string> GetAllMaterials()
            {
                return AssetDatabase.FindAssets("t:material", _buildPaths.ToArray()).ToList().ConvertAll(x =>
                {
                    return AssetDatabase.GUIDToAssetPath(x);
                });
            }
            private static void CollectVariants(List<string> materials)
            {
                root = new GameObject("Go");
                Camera camera = Camera.main;
                if (camera == null)
                {
                    camera = new GameObject("main ca").AddComponent<Camera>();
                    camera.tag = "MainCamera";
                    camera.transform.parent = root.transform;
                }

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
            private static GameObject CreateSphere(Transform root, string assetPath, Vector3 position, int index)
            {
                var material = AssetDatabase.LoadAssetAtPath<Material>(assetPath);

                var shader = material.shader;
                if (shader == null)
                    return null;

                var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                go.GetComponent<Renderer>().sharedMaterial = material;
                go.transform.position = position;
                go.name = $"Sphere_{index} | {material.name}";
                go.transform.parent = root;
                return go;
            }


        }
    }
}
