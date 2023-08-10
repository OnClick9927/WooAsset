using UnityEngine;

namespace WooAsset
{
    public class GameObjectBridge : ObjectBridge<GameObject>
    {
        public GameObjectBridge(GameObject context, AssetHandle handle) : base(context, handle)
        {
        }
    }
}
