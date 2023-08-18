namespace WooAsset
{
    [System.Serializable]
    public class StreamBundleList
    {
        public string[] fileNames;
        private static string _fileName;
        public static string fileName
        {
            get
            {
                if (string.IsNullOrEmpty(_fileName))
                    _fileName = AssetsHelper.GetStringHash("bundles#######") + ".bytes";
                return _fileName;
            }
        }
    }
}
