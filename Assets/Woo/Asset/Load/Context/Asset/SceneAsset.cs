using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace WooAsset
{
    public class SceneAsset : Asset
    {
        private SceneAssetLoadArgs loadArgs;

        public SceneAsset(Bundle bundle, List<Asset> dps, SceneAssetLoadArgs loadArgs) : base(bundle, dps, default)
        {
            this.loadArgs = loadArgs;
        }
        public override string path { get { return loadArgs.path; } }
        public override float progress
        {
            get
            {
                float sum = bundle.progress;
                if (dps == null) return sum;
                float dpSum = 0;
                for (int i = 0; i < dps.Count; i++)
                    dpSum += dps[i].progress / dps.Count;
                return (dpSum + sum) * 0.5f;
            }
        }
        protected async override void OnLoad()
        {
            await bundle;
            SetResult(null);
        }

        protected override void OnUnLoad()
        {
            Resources.UnloadUnusedAssets();
        }

        protected string sceneName { get { return Path.GetFileNameWithoutExtension(path); } }
        public virtual void LoadScene(LoadSceneMode mode)
        {
            SceneManager.LoadScene(sceneName, mode);
        }
        public virtual Scene LoadScene(LoadSceneParameters parameters)
        {
            return SceneManager.LoadScene(sceneName, parameters);
        }
        public virtual AsyncOperation LoadSceneAsync(LoadSceneParameters parameters)
        {
            return SceneManager.LoadSceneAsync(sceneName, parameters);
        }
        public virtual AsyncOperation LoadSceneAsync(LoadSceneMode mode)
        {
            return SceneManager.LoadSceneAsync(sceneName, mode);
        }
    }
}
