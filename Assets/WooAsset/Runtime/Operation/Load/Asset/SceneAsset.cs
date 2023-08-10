using UnityEngine;
using UnityEngine.SceneManagement;
namespace WooAsset
{
    public class SceneAsset : AssetHandle
    {
        protected string sceneName { get { return AssetsHelper.GetFileNameWithoutExtension(path); } }

        public override float progress => isDone ? 1 : bundle.progress * 0.5f + dpProgress * 0.5f;
        public override long assetLength => this.bundle.assetLength;
        public SceneAsset(AssetLoadArgs loadArgs) : base(loadArgs)
        {
        }
        protected async override void InternalLoad()
        {
            await LoadBundle();
            if (bundle.isErr)
                SetErr(bundle.error);
            SetResult(null);
        }

        public Scene LoadScene(LoadSceneMode mode) => LoadScene(new LoadSceneParameters(mode));
        public AsyncOperation LoadSceneAsync(LoadSceneMode mode) => !isDone || isErr ? null : LoadSceneAsync(new LoadSceneParameters(mode));
        public virtual Scene LoadScene(LoadSceneParameters parameters) => !isDone || isErr ? default : SceneManager.LoadScene(sceneName, parameters);
        public virtual AsyncOperation LoadSceneAsync(LoadSceneParameters parameters) => !isDone || isErr ? null : SceneManager.LoadSceneAsync(sceneName, parameters);
    }
}
