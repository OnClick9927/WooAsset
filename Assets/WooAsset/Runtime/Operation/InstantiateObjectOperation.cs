using UnityEngine;

namespace WooAsset
{

    public class InstantiateObjectOperation : Operation
    {
        public enum ErrorCode
        {
            SourceNull,
            SourceInvalid,
            GameObjectNull
        }
        public override float progress { get { return isDone ? 1 : 0; } }
        public GameObject gameObject { get; private set; }

        private GameObjectBridge bridge;

        public InstantiateObjectOperation(AssetHandle asset, Transform parent)
        {

            if (asset == null)
            {
                SetErr(OperationException.Create(ExceptionType.Instantiate, ErrorCode.SourceNull));
                InvokeComplete();
            }
            else
            {

                if (asset is ICanGetAsset)
                {
                    Done(asset, parent);
                }
                else
                {
                    SetErr(OperationException.Create(ExceptionType.Instantiate, ErrorCode.SourceInvalid, $"not valid asset: {asset.path}"));
                    InvokeComplete();
                }
            }
        }
        private async void Done(AssetHandle asset, Transform parent)
        {
            await asset;
            Create(asset, parent);
        }
        private void Create(AssetHandle asset, Transform parent)
        {
            if (!asset.isErr)
            {
                GameObject prefab = (asset as ICanGetAsset).GetAsset<GameObject>();
                if (prefab == null)
                {
                    SetErr(OperationException.Create(ExceptionType.Instantiate, ErrorCode.GameObjectNull, $"could not load gameObject from : {asset.path}"));
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
