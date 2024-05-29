using System;
using System.Collections.Generic;
using System.Linq;
using Object = UnityEngine.Object;
namespace WooAsset
{
    public class Asset : AssetHandle<UnityEngine.Object>
    {

        private Object[] assets;

        private AssetRequest loadOp;
        public Asset(AssetLoadArgs loadArgs, Bundle bundle) : base(loadArgs, bundle)
        {

        }


        public override float progress
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
        public virtual Type GetAssetType() => isDone && !isErr ? value.GetType() : null;
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




        protected virtual async void LoadUnityObject()
        {
            if (bundle.isErr)
            {
                this.SetErr(bundle.error);
                InvokeComplete();
                return;
            }
            if (async)
            {
                loadOp = bundle.LoadAssetAsync(path, GetAssetType(type));
                await loadOp;
                assets = loadOp.allAssets;
                SetResult(loadOp.asset);
            }
            else
            {
                var result = bundle.LoadAsset(path, GetAssetType(type));
                assets = result;
                SetResult(result[0]);
            }
        }
        protected Type GetAssetType(Type type)
        {
            if (type == typeof(UnityEngine.Object))
            {

                switch (assetType)
                {
                    case AssetType.Sprite: return typeof(UnityEngine.Sprite);
                    case AssetType.Shader: return typeof(UnityEngine.Shader);
                    case AssetType.ShaderVariant: return typeof(UnityEngine.ShaderVariantCollection);
                    case AssetType.None:
                    case AssetType.Ignore:
                    case AssetType.Directory:
                    case AssetType.Mesh:
                    case AssetType.Texture:
                    case AssetType.TextAsset:
                    case AssetType.VideoClip:
                    case AssetType.AudioClip:
                    case AssetType.Scene:
                    case AssetType.Material:
                    case AssetType.Prefab:
                    case AssetType.Font:
                    case AssetType.Animation:
                    case AssetType.AnimationClip:
                    case AssetType.AnimatorController:
                    case AssetType.ScriptObject:
                    case AssetType.Model:
                    default:
                        return type;
                }
            }
            return type;
        }
        protected sealed override void InternalLoad()
        {
            LoadUnityObject();

        }

    }

}
