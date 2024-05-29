using UnityEngine;
using UnityEngine.SceneManagement;
namespace WooAsset
{
    public class SceneAsset : AssetHandle
    {
        public override float progress => isDone ? 1 : bundle.progress;
        public SceneAsset(AssetLoadArgs loadArgs, Bundle bundle) : base(loadArgs, bundle)
        {
        }

        protected override void InternalLoad()
        {
            if (bundle.isErr)
                SetErr(bundle.error);
            InvokeComplete();
        }

        public Scene LoadScene(LoadSceneMode mode) => LoadScene(new LoadSceneParameters(mode));
        public virtual Scene LoadScene(LoadSceneParameters parameters) => !isDone || isErr ? default : bundle.LoadScene(path, parameters);


        public AsyncOperation LoadSceneAsync(LoadSceneMode mode) => !isDone || isErr ? null : LoadSceneAsync(new LoadSceneParameters(mode));
        public virtual AsyncOperation LoadSceneAsync(LoadSceneParameters parameters) => !isDone || isErr ? null : bundle.LoadSceneAsync(path, parameters);

        public virtual AsyncOperation UnloadSceneAsync(UnloadSceneOptions op)
        {
            return !isDone || isErr ? default : bundle.UnloadSceneAsync(path, op);
        }

    }
}
