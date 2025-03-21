﻿using System.Collections.Generic;
using System.Linq;

namespace WooAsset
{

    [System.Serializable]
    public class FileData
    {
        public enum FileCompareType
        {
            Hash,
            Length
        }
        public string path;
        public string name;
        public long length;
        public string hash;



        public static FileData CreateByFile(string path)
        {
            var data = new FileData()
            {
                name = AssetsHelper.GetFileName(path),
                path = path,
                length = AssetsHelper.GetFileLength(path),
            };
            data.hash = AssetsHelper.GetFileHash(path);
            return data;
        }

        public static void Compare(List<FileData> old, List<BundleData> src, FileCompareType checkType, out List<BundleData> change, out List<FileData> delete, out List<BundleData> add)
        {
            delete = old.FindAll(x => src.Find(y => y.bundleName == x.name) == null);
            add = src.FindAll(x => old.Find(y => y.name == x.bundleName) == null);
            if (checkType == FileCompareType.Hash)
            {
                change = src.FindAll(x => old.Find(y => y.name == x.hash && x.hash != y.hash) != null);
            }
            else
            {
                change = src.FindAll(x => old.Find(y => y.name == x.bundleName && x.length != y.length) != null);

            }
        }
        //public BundleFileData ToBundleFileData()
        //{
        //    return new BundleFileData()
        //    {
        //        hash = hash,
        //        length = length,
        //        name = name,
        //    };
        //}


        //public static void Compare(List<FileData> old, List<FileData> src, FileCompareType checkType, out List<FileData> change, out List<FileData> delete, out List<FileData> add)
        //{
        //    delete = old.FindAll(x => src.Find(y => y.name == x.name) == null);
        //    add = src.FindAll(x => old.Find(y => y.name == x.name) == null);
        //    if (checkType == FileCompareType.Hash)
        //    {
        //        change = src.FindAll(x => old.Find(y => y.name == x.name && x.hash != y.hash) != null);
        //    }
        //    else
        //    {
        //        change = src.FindAll(x => old.Find(y => y.name == x.name && x.length != y.length) != null);
        //    }

        //}

    }
}
