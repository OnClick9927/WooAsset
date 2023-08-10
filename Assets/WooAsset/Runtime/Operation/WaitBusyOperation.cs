using System.Threading.Tasks;

namespace WooAsset
{
    class WaitBusyOperation : YieldOperation
    {
        protected async override void EditorWait()
        {
            while (AssetsLoop.isBusy)
                await Task.Delay(10);
            InvokeComplete();

        }
        public override void NormalLoop()
        {
            if (!AssetsLoop.isBusy)
                InvokeComplete();
        }
    }

}
