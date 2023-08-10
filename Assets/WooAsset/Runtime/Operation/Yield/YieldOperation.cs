using System.Threading.Tasks;

namespace WooAsset
{
    class YieldOperation : Operation
    {
        public override float progress => isDone ? 1 : 0;
        public YieldOperation()
        {
#if UNITY_EDITOR
            Wait();
#else
            AssetsLoop.AddOperation(this);
#endif
        }
        private async void Wait()
        {
#if UNITY_EDITOR
            await Task.Delay(10);
            InvokeComplete();
#endif
        }
        public void Update()
        {
            InvokeComplete();
        }
    }

}
