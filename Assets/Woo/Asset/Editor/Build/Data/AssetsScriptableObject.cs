using System.IO;
using UnityEngine;

namespace WooAsset
{
    class AssetsScriptableObject : ScriptableObject
    {
        public static T Load<T>() where T : AssetsScriptableObject
        {
            string stoPath = AssetsInternal.CombinePath(AssetsEditorTool.editorPath,$"{typeof(T).Name}.asset");
            if (File.Exists(stoPath))
                return AssetsEditorTool.Load<T>(stoPath);
            return AssetsEditorTool.CreateScriptableObject<T>(stoPath);
        }
        public void Save()
        {
            AssetsEditorTool.Update(this);
        }
    }
}
