namespace WooAsset
{
    public class RawObject 
    {
        public string rawPath;
        public byte[] bytes;


        public static RawObject Create(string path)
        {
            RawObject obj = new RawObject();
            obj.rawPath = path;
            return obj;
        }
    }

}
