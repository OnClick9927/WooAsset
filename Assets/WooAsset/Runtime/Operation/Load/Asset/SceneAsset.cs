using UnityEngine;
using UnityEngine.SceneManagement;
namespace WooAsset
{
    public class SceneAsset : AssetHandle
    {
        public override float progress => isDone ? 1 : bundle.progress;
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
        public virtual Scene LoadScene(LoadSceneParameters parameters) => !isDone || isErr ? default :bundle.LoadScene(path, parameters) ;

        public virtual AsyncOperation LoadSceneAsync(LoadSceneParameters parameters) => !isDone || isErr ? null : bundle.LoadSceneAsync(path, parameters);
        public virtual AsyncOperation UnLoadScene(string path, UnloadSceneOptions op) => !isDone || isErr ? default : bundle.UnLoadScene(path, op);

    }
}
