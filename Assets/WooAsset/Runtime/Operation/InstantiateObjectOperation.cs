using UnityEngine;

namespace WooAsset
{

    public class InstantiateObjectOperation : Operation
    {
        public override float progress { get { return isDone ? 1 : 0; } }
        public GameObject gameObject { get; private set; }

        private GameObjectBridge bridge;
    
        public InstantiateObjectOperation(Asset asset, Transform parent)
        {
            Done(asset, parent);
        }
        private async void Done(Asset asset, Transform parent)
        {
            await asset;
            Create(asset, parent);
        }
        private void Create(Asset asset, Transform parent)
        {
            if (!asset.isErr)
            {
                GameObject prefab = asset.GetAsset<GameObject>();
                if (prefab == null)
                {
                    SetErr($"could not load gameObject from : {asset.path}");
                }
                else
                {
                    this.gameObject = GameObject.Instantiate(prefab, parent);
                    bridge = new GameObjectBridge(gameObject, asset);
                    Assets.AddBridge(bridge);
                }
            }
            else
            {
                SetErr(asset.error);
            }
            this.InvokeComplete();
        }
        public void Destroy()
        {
            Assets.Destroy(this.gameObject);
        }
    }

}
