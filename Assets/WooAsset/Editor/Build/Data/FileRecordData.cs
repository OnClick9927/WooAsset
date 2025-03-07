namespace WooAsset
{
    [System.Serializable]
    public class FileRecordData
    {
        public FileType type;
        public string path;

        public bool Fit(string path)
        {
            if (type == FileType.File)
                return path == this.path;
            return path.StartsWith(this.path);
        }
    }
}
