using UnityEngine;

namespace WooAsset
{

    public class InstantiateObjectOperation : Operation
    {
        public override float progress { get { return isDone ? 1 : 0; } }
        public GameObject gameObject { get; private set; }

        private GameObjectBridge bridge;
        public InstantiateObjectOperation(string path, Transform parent)
        {
            Done(path, parent);
        }
        private async void Done(string path, Transform parent)
        {
            var asset = await Assets.LoadAssetAsync(path, typeof(GameObject));
            Create(asset, parent);
        }
        private void Create(Asset asset, Transform parent)
        {
            if (!asset.isErr)
            {
                GameObject prefab = asset.GetAsset<GameObject>();
                if (prefab == null)
                {
                    SetErr("The asset has broken");
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
                SetErr("The asset has broken");
            }


            this.InvokeComplete();
        }
        public void Destroy()
        {
            Assets.Destroy(this.gameObject);
        }
    }

}
