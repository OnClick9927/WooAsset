using System;
using UnityEngine;

namespace WooAsset
{
    [System.Serializable]
    public class AssetReference<T> : AssetReference where T : UnityEngine.Object
    {
        public override Type type => typeof(T);
    }
    [System.Serializable]
    public class AssetReference
    {
        public string guid;
        private string _path = string.Empty;
        private bool set;

        public string path
        {
            get
            {
                if (!set)
                {
                    if (Assets.Initialized())
                    {
                        _path = AssetsInternal.GUIDToAssetPath(guid);
                        set = true;
                    }
                }
                return _path;
            }
        }
        public virtual System.Type type => typeof(UnityEngine.Object);
        public Asset LoadAsset() => Assets.LoadAsset(path);
        public Asset LoadAssetAsync() => Assets.LoadAssetAsync(path);
        public RawAsset LoadRawAssetAsync() => Assets.LoadRawAssetAsync(path);
        public RawAsset LoadRawAsset() => Assets.LoadRawAsset(path);
        public SceneAsset LoadSceneAsset() => Assets.LoadSceneAsset(path);
        public SceneAsset LoadSceneAssetAsync() => Assets.LoadSceneAssetAsync(path);
        public SubAsset LoadSubAsset() => Assets.LoadSubAsset(path);
        public SubAsset LoadSubAssetAsync() => Assets.LoadSubAssetAsync(path);
        public void Release() => Assets.Release(path);
        public WooAsset.InstantiateObjectOperation Instantiate(Transform parent) => Assets.InstantiateAsync(path, parent);


    }
}
