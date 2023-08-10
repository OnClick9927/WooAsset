using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace WooAsset
{
    partial class AssetsEditorTool
    {
        public class EditorSceneAsset : SceneAsset
        {
            public EditorSceneAsset(AssetLoadArgs loadArgs) : base(loadArgs)
            {
            }
            public override float progress { get { return 1; } }
            private long _assetLength;

            public override long assetLength => _assetLength;
            protected override void InternalLoad()
            {
                _assetLength = FileData.CreateByFile(path).length;
                SetResult(null);
            }
 
            public override AsyncOperation LoadSceneAsync(LoadSceneParameters parameters)
            {
                return EditorSceneManager.LoadSceneAsyncInPlayMode(path, parameters);
            }
            public override Scene LoadScene(LoadSceneParameters parameters)
            {
                return EditorSceneManager.LoadSceneInPlayMode(path, parameters);
            }

        }

    }
}
