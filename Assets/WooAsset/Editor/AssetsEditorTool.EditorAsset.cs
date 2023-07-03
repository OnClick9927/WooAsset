using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;
namespace WooAsset
{
    partial class AssetsEditorTool
    {
        public class EditorAsset : Asset
        {
            public EditorAsset(AssetLoadArgs loadArgs) : base(loadArgs)
            {
            }
            public override T GetAsset<T>()
            {
                if (value is Texture2D)
                {
                    if (typeof(T) == typeof(Sprite))
                    {
                        var sp = AssetDatabase.LoadAssetAtPath(path, typeof(Sprite));
                        SetResult(sp);
                    }
                }
                return value as T;
            }
            public override float progress { get { return 1; } }

            public Object[] _allAssets;
            public override Object[] allAssets => _allAssets;

            protected override void OnLoad()
            {
                _allAssets = AssetDatabase.LoadAllAssetsAtPath(path);
                var result = AssetDatabase.LoadAssetAtPath(path, typeof(Object));

                SetResult(result);
            }
        }
    }

}
