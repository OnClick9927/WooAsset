namespace WooAsset
{
    public abstract class AssetRequest : Operation
    {
        public abstract UnityEngine.Object asset { get; }
        public abstract UnityEngine.Object[] allAssets { get; }
    }

}
