using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace WooAsset
{
    public interface ICanGetAsset
    {
        public T GetAsset<T>() where T : Object;
        System.Type GetAssetType();
    }

    public abstract class AssetHandle : AssetOperation
    {
        public virtual string bundleName => string.Empty;

        public virtual bool IsBundleAsset => false;
        private bool _async;
        public string path { get; private set; }
        public Type type { get; private set; }

        public override bool async => _async;
        protected AssetHandle(string path, bool async, Type type)
        {
            _async = async;
            this.type = type;
            this.path = path;
        }



    }
    public abstract class BundleAssetHandle<T> : BundleAssetHandle
    {
        internal BundleAssetHandle(AssetLoadArgs loadArgs, Bundle bundle) : base(loadArgs, bundle)
        {
        }

        public T value { get; private set; }
        protected virtual void SetResult(T value)
        {
            this.value = value;
            InvokeComplete();
        }
    }
    public abstract class BundleAssetHandle : AssetHandle
    {
        public override sealed string bundleName => data.bundleName;
        //protected System.Type type => loadArgs.type;
        public sealed override bool IsBundleAsset => true;
        public override bool async => loadArgs.async;
        protected Bundle bundle { get; private set; }
        public AssetData data => loadArgs.data;

        public AssetType assetType => data.type;
        //public string path => data.path;
        private AssetLoadArgs loadArgs;

        internal BundleAssetHandle(AssetLoadArgs loadArgs, Bundle bundle) : base(loadArgs.data.path, loadArgs.async, loadArgs.type)
        {
            this.loadArgs = loadArgs;
            this.bundle = bundle;
        }
        protected sealed override void OnUnLoad() { }
        protected sealed override async void OnLoad()
        {
            if (AssetsLoop.instance.isBusy)
                await Operation.busy;
            await bundle;
            InternalLoad();
        }

        protected abstract void InternalLoad();

    }

    public abstract class CustomAsset : AssetHandle
    {
        public sealed override string bundleName => string.Empty;
        public sealed override bool IsBundleAsset => false;
        protected CustomAsset(string path, bool async, Type type) : base(path, async, type)
        {
        }
    }
    public abstract class CustomAsset<T> : CustomAsset
    {
        protected CustomAsset(string path, bool async, Type type) : base(path, async, type)
        {
        }

        public T value { get; private set; }
        protected virtual void SetResult(T value)
        {
            this.value = value;
            InvokeComplete();
        }
    }


    public class ResourceAsset : CustomAsset<UnityEngine.Object>, ICanGetAsset
    {
        public const string flag = "Resources:";
        public static bool IsFit(string path)
        {
            return path.StartsWith(flag);
        }

        private ResourceRequest loadOp;

        public ResourceAsset(string path, bool async, Type type) : base(path, async, type)
        {
        }

        public System.Type GetAssetType() => isDone && !isErr ? value.GetType() : null;

        public override float progress => isDone ? 1 : (async ? loadOp.progress : 0);
        public T GetAsset<T>() where T : Object => isDone ? value as T : null;
        protected sealed override async void OnLoad()
        {
            if (AssetsLoop.instance.isBusy)
                await Operation.busy;
            InternalLoad();
        }

        protected async void InternalLoad()
        {
            var path = this.path.Remove(0, flag.Length);
            if (async)
            {

                loadOp = Resources.LoadAsync(path, type);
                await loadOp;
                SetResult(loadOp.asset);
            }
            else
            {
                var result = Resources.Load(path, type);
                SetResult(result);
            }
        }

        protected override void OnUnLoad()
        {
            //if (value != null)
            //    Resources.UnloadAsset(value);
        }

        public static string MakeResPath(string path)
        {
            return $"{flag}{path}";
        }
    }


    public class Asset : BundleAssetHandle<UnityEngine.Object>, ICanGetAsset
    {
        private AssetRequest loadOp;
        internal Asset(AssetLoadArgs loadArgs, Bundle bundle) : base(loadArgs, bundle)
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



        internal virtual AssetRequest LoadAsync(string path, System.Type type) => bundle.LoadAssetAsync(path, type);
        internal virtual void OnLoadAsyncEnd(AssetRequest request) { }
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
        internal SubAsset(AssetLoadArgs loadArgs, Bundle bundle) : base(loadArgs, bundle)
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
            .FirstOrDefault(x=>x is T) as T;


        internal override AssetRequest LoadAsync(string path, Type type)
        {
            return bundle.LoadAssetWithSubAssetsAsync(path, type);
        }
        internal override void OnLoadAsyncEnd(AssetRequest request)
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
    public class RawAsset : BundleAssetHandle<RawObject>
    {
        internal RawAsset(AssetLoadArgs loadArgs, Bundle bundle) : base(loadArgs, bundle)
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

    public class SceneAsset : BundleAssetHandle
    {
        public override float progress => isDone ? 1 : bundle.progress;
        internal SceneAsset(AssetLoadArgs loadArgs, Bundle bundle) : base(loadArgs, bundle)
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
