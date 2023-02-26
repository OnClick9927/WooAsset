

using System.Collections.Generic;
using UnityEngine;

namespace WooAsset
{
    public partial class Assets
    {
        public class InstantiateObjectOperation : AssetOperation
        {
            private static Dictionary<GameObject, Asset> map = new Dictionary<GameObject, Asset>();

            public override float progress { get { return isDone ? 1 : 0; } }
            public GameObject gameObject;

            public InstantiateObjectOperation(string path,Transform parent)
            {
                Done(path, parent);
            }
            private async void Done(string path, Transform parent)
            {
                var asset = LoadAssetAsync(path);
                await asset;
                Create(asset, parent);
            }
            private void Create(Asset asset, Transform parent)
            {
                GameObject prefab = asset.GetAsset<GameObject>();
                gameObject = GameObject.Instantiate(prefab, parent);
                map[gameObject] = asset;
                this.InvokeComplete();
            }
            public void Destroy()
            {
                Destroy(this.gameObject);
            }
            public static void Destroy(GameObject gameObject)
            {
                if (map.ContainsKey(gameObject))
                {
                    var asset = map[gameObject];
                    Assets.Release(asset);
                    map.Remove(gameObject);
                    GameObject.Destroy(gameObject);
                }
            }
        }
    }
}
