namespace WooAsset
{
    public class RawObject 
    {
        public string rawPath;
        public byte[] bytes;

        public static RawObject Create(string path, byte[] bytes)
        {
            RawObject obj = new RawObject();
            obj.rawPath = path;
            obj.bytes = bytes;
            return obj;
        }
    }

}
