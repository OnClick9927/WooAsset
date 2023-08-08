using UnityEngine;

namespace WooAsset
{
    public class AssetsScriptableObject : ScriptableObject
    {

        public static T Load<T>() where T : AssetsScriptableObject
        {
            string stoPath = AssetsHelper.CombinePath(AssetsEditorTool.editorPath, $"{typeof(T).Name}.asset");
            if (AssetsHelper.ExistsFile(stoPath))
                return AssetsEditorTool.Load<T>(stoPath);
            return AssetsEditorTool.CreateScriptableObject<T>(stoPath);
        }
        public void Save()
        {
            AssetsEditorTool.Update(this);
        }
    }
}
