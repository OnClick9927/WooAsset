using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace WooAsset
{
    public class AssetsScriptableObject : ScriptableObject
    {

        protected virtual void OnLoad() { }

        private static Dictionary<System.Type, AssetsScriptableObject> _map = new Dictionary<System.Type, AssetsScriptableObject>();
        public static T Get<T>() where T : AssetsScriptableObject
        {
            var type = typeof(T);
            var tmp = AssetsEditorTool.GetOrDefaultFromDictionary(_map, type);
            if (tmp) return tmp as T;
            string stoPath = AssetsEditorTool.CombinePath(AssetsEditorTool.EditorPath, $"{typeof(T).Name}.asset");
            T t = null;
            if (AssetsEditorTool.ExistsFile(stoPath))
                t = Load<T>(stoPath);
            else
                t = CreateScriptableObject<T>(stoPath);
            t.OnLoad();
            _map[type] = t;
            return t;
        }
        private static T CreateScriptableObject<T>(string savePath) where T : ScriptableObject
        {
            ScriptableObject sto = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(sto, savePath);
            EditorUtility.SetDirty(sto);
            AssetDatabase.ImportAsset(savePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return Load<T>(savePath);
        }
        private static T Load<T>(string path) where T : Object => AssetDatabase.LoadAssetAtPath<T>(path);

        private static void Update<T>(T t) where T : Object
        {
            EditorApplication.delayCall += delegate ()
            {
                EditorUtility.SetDirty(t);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

            };
        }

        public void Save()
        {
            Update(this);
        }
    }
}
