using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace WooAsset
{
    public class AssetsEditorCache : AssetsScriptableObject
    {

        public EditorAssetCollection tree_asset_all = new EditorAssetCollection();



        [System.Serializable]
        public class PkgBundles
        {
            public string pkgName;
            public List<EditorBundleData> previewBundles = new List<EditorBundleData>();
            public EditorAssetCollection tree = new EditorAssetCollection();

        }

        private int _index = 0;
        public int index
        {
            get
            {
                _index = Mathf.Clamp(_index, 0, pkgBundles.Count);
                return _index;

            }
            set
            {
                _index = Mathf.Clamp(value, 0, pkgBundles.Count);
            }
        }


        public bool viewAllAssets;


        public List<PkgBundles> pkgBundles = new List<PkgBundles>();

        private List<EditorBundleData> previewBundles_noerr = new List<EditorBundleData>();
        private EditorAssetCollection tree_bundle_noerr = new EditorAssetCollection();

        public List<EditorBundleData> previewBundles
        {
            get
            {
                if (pkgBundles.Count == 0)
                    return previewBundles_noerr;
                return pkgBundles[index].previewBundles;

            }
        }
        public EditorAssetCollection tree_bundle
        {
            get
            {
                if (pkgBundles.Count == 0)
                    return tree_bundle_noerr;
                return pkgBundles[index].tree;

            }

        }
        public EditorAssetCollection tree_asset
        {
            get
            {
                if (viewAllAssets)
                    return tree_asset_all;
                return tree_bundle;
            }
        }


        public ManifestData manifest;
        internal TaskPipelineType Pipeline;

        public EditorBundleData GetBundleGroupByAssetPath(string assetPath)
        {
            return previewBundles.Find(x => x.ContainsAsset(assetPath));
        }
        public EditorBundleData GetBundleGroupByBundleName(string bundleName)
        {
            return previewBundles.Find(x => x.hash == bundleName);
        }

        [System.Serializable]
        public class AssetDataBaseCache
        {
            public string path;
            private long _PreviewSize;
            private int _InstanceID;
            private string _type;
            private string _hash;
            private Texture2D _thumb;
            public Texture2D thumb
            {
                get
                {
                    if (_thumb == null)
                    {
                        _thumb = AssetPreview.GetMiniThumbnail(AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path));
                    }
                    return _thumb;
                }
            }
            public long PreviewSize
            {
                get
                {
                    if (_PreviewSize == 0)
                        _PreviewSize = GetMemorySizeLong(path, AssetsEditorTool.GetTypeByName(type));
                    return _PreviewSize;
                }
            }

            public int InstanceID
            {
                get
                {
                    if (_InstanceID == 0)
                        _InstanceID = GetMainAssetInstanceID(path);

                    return _InstanceID;
                }
            }
            public string type
            {
                get
                {
                    if (string.IsNullOrEmpty(_type))
                        _type = AssetDatabase.GetMainAssetTypeAtPath(path).FullName;
                    return _type;
                }
            }
            public string hash
            {
                get
                {
                    if (string.IsNullOrEmpty(_hash))
                        _hash = AssetsEditorTool.GetFileHash(path);

                    return _hash;
                }
            }
        }

        public List<AssetDataBaseCache> cachedAssets = new List<AssetDataBaseCache>();
        private Dictionary<string, AssetDataBaseCache> dic = new Dictionary<string, AssetDataBaseCache>();
        public AssetDataBaseCache GetCache(string path)
        {
            if (dic.TryGetValue(path, out var cache))
            {
                return cache;
            }
            var find = cachedAssets.Find(x => x.path == path);
            if (find == null)
            {
                //var obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
                var type = AssetDatabase.GetMainAssetTypeAtPath(path);
                find = new AssetDataBaseCache()
                {
                    path = path,
                    //PreviewSize = GetMemorySizeLong(path, type),
                    //InstanceID = GetMainAssetInstanceID(path),
                    //type = type.FullName,
                    //hash = AssetsEditorTool.GetFileHash(path)
                    //thumbnail = AssetPreview.GetMiniThumbnail(obj),

                };
                cachedAssets.Add(find);
                //Save();
            }
            dic[path] = find;
            return find;

        }
        private static MethodInfo _GetTextureMemorySizeLong;
        static MethodInfo _GetMainAssetInstanceID;
        private static long GetMemorySizeLong(string path, Type obj)
        {
            if (!(typeof(Texture).IsAssignableFrom(obj))) return AssetsEditorTool.GetFileLength(path);

            if (_GetTextureMemorySizeLong == null)
            {
                var type = typeof(AssetDatabase).Assembly.GetType("UnityEditor.TextureUtil");
                _GetTextureMemorySizeLong = type.GetMethod("GetStorageMemorySizeLong", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
            }
            var newType = (long)_GetTextureMemorySizeLong.Invoke(null, new object[] { AssetDatabase.LoadAssetAtPath<Texture>(path) });
            return newType;
        }

        private static int GetMainAssetInstanceID(string path)
        {
            if (_GetMainAssetInstanceID == null)
            {
                _GetMainAssetInstanceID = typeof(AssetDatabase).GetMethod("GetMainAssetInstanceID", BindingFlags.Static | BindingFlags.NonPublic);
            }

            return (int)_GetMainAssetInstanceID.Invoke(null, new object[] { path });

        }

        public void RemoveUseLessCache()
        {
            cachedAssets.RemoveAll(x => !AssetsEditorTool.ExistsFile(x.path) || !AssetsEditorTool.ExistsDirectory(x.path));
            dic.Clear();
        }

    }
}
