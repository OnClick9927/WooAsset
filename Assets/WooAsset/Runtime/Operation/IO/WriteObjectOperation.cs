using UnityEngine;

namespace WooAsset
{
    public class WriteObjectOperation<T> : Operation
    {
        private Operation op;
        public override float progress => isDone ? 1 : (op == null ? 0 : op.progress);
        protected T t;
        protected string path;
        protected bool async;
        public WriteObjectOperation(T t, string path, bool async)
        {
            this.t = t;
            this.path = path;

            this.async = async;
            Done();
        }
        protected virtual byte[] GetBytes(byte[] bytes)
        {
            return bytes;
        }
        protected virtual async void Done()
        {
            var bytes = AssetsHelper.encoding.GetBytes(JsonUtility.ToJson(t, true));
            op = AssetsHelper.WriteFile(GetBytes(bytes), path, async);
            await op;
            InvokeComplete();
        }
    }
}
