namespace WooAsset
{
    public class EmptyOperation : Operation
    {
        public override float progress => 1;
        public EmptyOperation() {
            InvokeComplete();
        }
    }
}
