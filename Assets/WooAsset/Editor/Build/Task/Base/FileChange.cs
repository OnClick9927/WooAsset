

using System;
using System.Collections.Generic;

namespace WooAsset
{
    [Serializable]
    public class FileChange
    {
        public List<FileData> add;
        public List<FileData> delete;
        public List<FileData> change;
    }
}
