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
            public long PreviewSize;
            //public Texture2D thumbnail;
            public int InstanceID;
            public string type;
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
                    PreviewSize = GetMemorySizeLong(path, type),
                    InstanceID = GetMainAssetInstanceID(path),
                    type = type.FullName,
                    //thumbnail = AssetPreview.GetMiniThumbnail(obj),
                  
                };
                cachedAssets.Add(find);
                Save();
            }
            dic[path] = find;
            return find;

        }
        private static MethodInfo _GetTextureMemorySizeLong;
        static MethodInfo _GetMainAssetInstanceID;
        private static long GetMemorySizeLong(string path, Type obj)
        {
            if (!(typeof(Texture).IsAssignableFrom(obj) )) return AssetsEditorTool.GetFileLength(path);

            if (_GetTextureMemorySizeLong == null)
            {
                var type = typeof(AssetDatabase).Assembly.GetType("UnityEditor.TextureUtil");
                _GetTextureMemorySizeLong = type.GetMethod("GetStorageMemorySizeLong", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
            }
            var newType = (long)_GetTextureMemorySizeLong.Invoke(null, new object[] { obj });
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


        class CacheModificationProcessor : AssetModificationProcessor
        {
            static void RemoveCache(string path)
            {
                var count = AssetsEditorTool.cache.cachedAssets.RemoveAll(x => x.path == path);
                if (count > 0)
                {
                    AssetsEditorTool.cache.dic.Remove(path);
                    AssetsEditorTool.cache.Save();
                }
            }
            // 资源即将被创建时调用
            public static void OnWillCreateAsset(string assetPath)
            {
                RemoveCache(assetPath);
            }

            // 资源即将被保存时调用
            public static string[] OnWillSaveAssets(string[] paths)
            {
                foreach (string path in paths)
                {
                    RemoveCache(path);
                }
                return paths; // 必须返回 paths
            }

            // 资源即将被移动时调用
            public static AssetMoveResult OnWillMoveAsset(string sourcePath, string destinationPath)
            {
                RemoveCache(sourcePath);
                RemoveCache(destinationPath);

                return AssetMoveResult.DidNotMove; // 返回 DidMove 允许移动
            }

            // 资源即将被删除时调用
            public static AssetDeleteResult OnWillDeleteAsset(string assetPath, RemoveAssetOptions options)
            {
                RemoveCache(assetPath);
                return AssetDeleteResult.DidNotDelete; // 返回 DidDelete 允许删除
            }

        }

    }
}
