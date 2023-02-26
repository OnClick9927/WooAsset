using UnityEngine;
using Object = UnityEngine.Object;
namespace WooAsset
{
    public class EditorAsset : Asset
    {
        public EditorAsset(AssetLoadArgs loadArgs) : base(null, null, loadArgs)
        {
        }
        public override float progress { get { return 1; } }
        protected override void OnLoad()
        {
            var result = UnityEditor.AssetDatabase.LoadAssetAtPath(path, typeof(Object));
            SetResult(result);
        }
        protected override void OnUnLoad()
        {
            Resources.UnloadUnusedAssets();
        }
    }
}
