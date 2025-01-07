using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace WooAsset
{



    public abstract class AssetHandle<T> : AssetHandle
    {
        protected AssetHandle(AssetLoadArgs loadArgs, Bundle bundle) : base(loadArgs, bundle)
        {
        }

        public T value { get; private set; }
        protected virtual void SetResult(T value)
        {
            this.value = value;
            InvokeComplete();
        }
    }
    public abstract class AssetHandle : AssetOperation
    {
        protected System.Type type => loadArgs.type;

        public override bool async => loadArgs.async;
        protected Bundle bundle { get; private set; }
        public AssetData data => loadArgs.data;

        public AssetType assetType => data.type;
        public virtual string path => data.path;
        public string bundleName => data.bundleName;
        private AssetLoadArgs loadArgs;

        public AssetHandle(AssetLoadArgs loadArgs, Bundle bundle)
        {
            this.loadArgs = loadArgs;
            this.bundle = bundle;
        }
        protected sealed override void OnUnLoad() { }
        protected sealed override async void OnLoad()
        {
            if (AssetsLoop.isBusy)
                await new WaitBusyOperation();
            await bundle;
            InternalLoad();
        }

        protected abstract void InternalLoad();

    }

    public class Asset : AssetHandle<UnityEngine.Object>
    {
        private AssetRequest loadOp;
        public Asset(AssetLoadArgs loadArgs, Bundle bundle) : base(loadArgs, bundle)
        {

        }


        public sealed override float progress
        {
            get
            {
                if (isDone) return 1;
                if (async)
                {
                    if (loadOp == null)
                        return bundle.progress * 0.5f;
                    return 0.5f + 0.5f * loadOp.progress;
                }
                return bundle.progress;
            }
        }

        public T GetAsset<T>() where T : Object => isDone ? value as T : null;
        public System.Type GetAssetType() => isDone && !isErr ? value.GetType() : null;



        protected virtual AssetRequest LoadAsync(string path, System.Type type) => bundle.LoadAssetAsync(path, type);
        protected virtual void OnLoadAsyncEnd(AssetRequest request) { }
        protected virtual Object LoadSync(string path, System.Type type) => bundle.LoadAsset(path, type);
        protected sealed async override void InternalLoad()
        {
            if (bundle.isErr)
            {
                this.SetErr(bundle.error);
                InvokeComplete();
                return;
            }
            var _type = AssetsHelper.GetAssetType(assetType, type);
            if (async)
            {
                loadOp = LoadAsync(path, _type);
                await loadOp;
                OnLoadAsyncEnd(loadOp);
                SetResult(loadOp.asset);
            }
            else
            {
                var result = LoadSync(path, _type);
                SetResult(result);
            }

        }

    }
    public class SubAsset : Asset
    {
        private Object[] assets;
        public SubAsset(AssetLoadArgs loadArgs, Bundle bundle) : base(loadArgs, bundle)
        {

        }

        public virtual Object[] allAssets => isDone && !isErr ? assets : null;
        public IReadOnlyList<T> GetSubAssets<T>() where T : Object => !isDone || isErr
                ? null
                : allAssets
                .Where(x => x is T)
                .Select(x => x as T)
                .ToArray();
        public T GetSubAsset<T>(string name) where T : Object => !isDone || isErr
            ? null :
            allAssets
            .Where(x => x.name == name)
            .FirstOrDefault() as T;


        protected override AssetRequest LoadAsync(string path, Type type)
        {
            return bundle.LoadAssetWithSubAssetsAsync(path, type);
        }
        protected override void OnLoadAsyncEnd(AssetRequest request)
        {
            assets = request.allAssets;
        }
        protected override Object LoadSync(string path, Type type)
        {
            var result = bundle.LoadAssetWithSubAssets(path, type);
            assets = result;
            return result[0];
        }
    }
    public class RawAsset : AssetHandle<RawObject>
    {
        public RawAsset(AssetLoadArgs loadArgs, Bundle bundle) : base(loadArgs, bundle)
        {
        }
        public RawObject GetAsset() => isDone ? value : null;

        public override float progress
        {
            get
            {
                if (isDone) return 1;
                return bundle.progress;
            }
        }
        protected override void InternalLoad()
        {
            if (bundle.isErr)
            {
                this.SetErr(bundle.error);
                InvokeComplete();
                return;
            }

            var raw = bundle.LoadRawObject(path);
            SetResult(raw);
        }
    }

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
