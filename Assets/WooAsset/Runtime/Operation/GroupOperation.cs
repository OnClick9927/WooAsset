namespace WooAsset
{
    public class GroupOperation : Operation
    {
        private readonly Operation[] ops;
        private int total;
        private int step;
        public override float progress => isDone ? 1 : step / (float)total;

        public GroupOperation(params Operation[] ops)
        {
            this.ops = ops;
            if (this.ops == null || this.ops.Length == 0)
                InvokeComplete();
            else
                Done();
        }

        private async void Done()
        {
            total = this.ops.Length;
            for (int i = 0; i < total; i++)
            {
                await this.ops[i];
                step= i;
            }
            InvokeComplete();
        }
    }
}
