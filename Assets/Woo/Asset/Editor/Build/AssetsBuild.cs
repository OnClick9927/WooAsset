using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System;
using System.Linq;
using Application = UnityEngine.Application;
using System.Text;

namespace WooAsset
{
    public partial class AssetsBuild
    {
        private static string buildTarget
        {
            get
            {
                switch (EditorUserBuildSettings.activeBuildTarget)
                {
                    case BuildTarget.Android:
                        return "Android";
                    case BuildTarget.StandaloneWindows:
                    case BuildTarget.StandaloneWindows64:
                        return "Windows";
                    case BuildTarget.iOS:
                        return "iOS";
                    case BuildTarget.WebGL:
                        return "WebGL";
                    default:
                        return "";
                }
            }
        }
        public static string outputPath
        {
            get
            {
                string path = "Assets/../DLC/";
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                path = AssetsInternal.CombinePath(path, buildTarget);
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                return path;
            }
        }

        static AssetsToolSetting tool { get { return AssetsToolSetting.Load<AssetsToolSetting>(); } }
        static AssetsBuildSetting setting { get { return AssetsBuildSetting.Load<AssetsBuildSetting>(); } }
        static AssetsEditorCache cache { get { return AssetsEditorCache.Load<AssetsEditorCache>(); } }

        public static void AddAssetTag(string tag, List<string> assets)
        {
            cache.AddAssetTag(tag, assets);
            cache.Save();
        }
        public static void RemoveTagAssets(List<string> assets)
        {
            cache.RemoveTagAssets(assets);
            cache.Save();
        }
        public static void RemoveUseLessTagAssets()
        {
            cache.RemoveUseLessTagAssets(setting.tags);
            cache.Save();
        }
        public static void CollectInBuildAssets()
        {
            cache.Collect(setting.buildPaths);
            RemoveUseLessTagAssets();
            cache.Save();
        }
        public static void FreshPreViewBundles(bool md5)
        {
            CollectInBuildAssets();
            var result = AssetsBuild.CollectBundleGroup();
            if (md5)
            {
                result = AssetsBuild.CollectMain(result);
            }
            cache.SetPreviewBundles(result);
            cache.Save();
        }
        public static void ClearCache()
        {
            cache.Clear();
            cache.Save();
        }
        static List<string> GetAllFilesIncludeList(string directory, List<string> exName, List<string> result)
        {
            DirectoryInfo root = new DirectoryInfo(directory);
            FileInfo[] files = root.GetFiles();
            for (int i = 0; i < exName.Count; i++) exName[i] = exName[i].ToLower();
            string ex;
            for (int i = 0; i < files.Length; i++)
            {
                ex = Path.GetExtension(files[i].FullName).ToLower();
                if (exName.IndexOf(ex) < 0) continue;
                result.Add(AssetsInternal.ToRegularPath(files[i].FullName));
            }
            DirectoryInfo[] dirs = root.GetDirectories();
            if (dirs.Length > 0)
            {
                for (int i = 0; i < dirs.Length; i++)
                {
                    GetAllFilesIncludeList(dirs[i].FullName, exName, result);
                }
            }

            return result;

        }
        static void RemoveMetaFiles(string outputPath)
        {
            List<string> file_paths = GetAllFilesIncludeList(outputPath, new List<string>() { ".meta", ".manifest" }, new List<string>());
            for (int i = 0; i < file_paths.Count; i++)
            {
                string file = file_paths[i];
                File.Delete(file);
            }
        }
        static void Encrypt(IAssetStreamEncrypt en, string outputPath, string[] bundles)
        {
            foreach (var abPath in bundles)
            {
                string filepath = AssetsInternal.CombinePath(outputPath, abPath);
                var data = File.ReadAllBytes(filepath);
                File.WriteAllBytes(filepath, en.EnCode(abPath, data));
            }

        }
        static void RemoveUselessFiles(string outputPath, string[] bundles)
        {
            //RemoveMetaFiles(outputPath);
            var all = Directory.GetFiles(outputPath);
            List<string> need = bundles.ToList().ConvertAll(b => AssetsInternal.CombinePath(outputPath, b));
            need = all.ToList().FindAll(x => !need.Contains(x));
            foreach (var item in need)
            {
                File.Delete(item);
            }
        }
        static void BuildVersion(IAssetStreamEncrypt en, string outputPath, string version_txt, string[] bundles)
        {
            AssetsVersion version = new AssetsVersion();
            foreach (var bundle in bundles)
            {
                string path = AssetsInternal.CombinePath(outputPath, bundle);
                FileInfo fileInfo = new FileInfo(path);
                AssetsVersion.VersionData data = new AssetsVersion.VersionData();
                data.length = fileInfo.Length;
                data.bundleName = bundle;
                data.md5 = AssetsInternal.GetFileHash(path);
                version.data_list.Add(data);
            }
            version.version = version_txt;
            var v = JsonUtility.ToJson(version);
            string version_file_name = AssetsInternal.GetNameHash(AssetsVersion.versionName);
            var bytes = en.EnCode(version_file_name, AssetsVersion.encoding.GetBytes(v));
            File.WriteAllBytes(AssetsInternal.CombinePath(outputPath, version_file_name), bytes);
        }


        public static List<BundleGroup> CollectBundleGroup()
        {
            Type collectType = setting.GetBuildGroupType();
            Dictionary<AssetInfo, List<AssetInfo>> dic = cache.GetDpDic();
            List<AssetInfo> assets = new List<AssetInfo>(cache.GetAssets());
            assets.AddRange(cache.GetSingleFiles());
            assets.RemoveAll(x => x.type == AssetInfo.AssetType.Directory);
            assets.RemoveAll(x => x.path == AssetManifest.Path);
            var collector = Activator.CreateInstance(collectType) as ICollectBundle;
            var builds = new List<BundleGroup>();
            collector.Create(assets, dic, builds);
            builds.RemoveAll(x => x.assets.Count == 0);
            builds.Sort((a, b) =>
            {
                return a.length > b.length ? -1 : 1;
            });
            return builds;
        }
        public static List<BundleGroup> CollectMain(List<BundleGroup> builds)
        {
            for (int i = 0; i < builds.Count; i++)
            {
                BundleGroup build = builds[i];
                build.name = AssetsInternal.GetNameHash(build.name);
            }
            Dictionary<string, string> allAssets = new Dictionary<string, string>();
            Dictionary<string, List<string>> assetDependence = new Dictionary<string, List<string>>();

            foreach (var build in builds)
            {
                foreach (var assetPath in build.assets)
                {
                    allAssets.Add(assetPath, build.name);
                }
            }
            foreach (var item in allAssets)
            {
                var path = item.Key;
                var dps = cache.GetDps(path);
                if (dps != null)
                    assetDependence.Add(path, dps);
            }
            if (!File.Exists(AssetManifest.Path))
                AssetsEditorTool.CreateScriptableObject<AssetManifest>(AssetManifest.Path);
            AssetManifest main = AssetsEditorTool.Load<AssetManifest>(AssetManifest.Path);
            main.Read(allAssets, assetDependence, cache.GetTagDic());
            AssetsEditorTool.Update(main);
            BundleGroup mainGroup = new BundleGroup(AssetsInternal.GetNameHash(AssetManifest.Path));
            mainGroup.AddAsset(AssetManifest.Path);
            builds.Add(mainGroup);
            return builds;
        }
        public bool SetBuildGroupType(Type type)
        {
            return setting.SetBuildGroupType(type);
        }
        public bool SetStreamEncryptType(Type type)
        {
            return setting.SetStreamEncryptType(type);
        }
        public static void Build()
        {
            AssetsBuild.ShaderVariantCollector.Run(async () =>
            {
                await AssetsBuild.AtlasBuild.Run();
                string outputPath = AssetsBuild.outputPath;
                BuildAssetBundleOptions option = setting.GetOption();
                string version_txt = setting.version;
                CollectInBuildAssets();
                FreshPreViewBundles(true);
                AssetBundleManifest main = BuildPipeline.BuildAssetBundles(outputPath, cache.GetPreviewBundles().ConvertAll(x =>
                {
                    return new AssetBundleBuild()
                    {
                        assetBundleName = x.name,
                        assetNames = x.assets.ToArray()
                    };
                }).ToArray(), option, EditorUserBuildSettings.activeBuildTarget);
                var bundles = main.GetAllAssetBundles();
                RemoveUselessFiles(outputPath, bundles);
                IAssetStreamEncrypt en = Activator.CreateInstance(setting.GetStreamEncryptType()) as IAssetStreamEncrypt;
                Encrypt(en, outputPath, bundles);
                BuildVersion(en, outputPath, version_txt, bundles);
            });

        }





        public static void OpenOutputFolder()
        {
            EditorUtility.OpenWithDefaultApp(outputPath);
        }
        public static void ClearOutputFolder()
        {
            Directory.Delete(outputPath, true);
        }


        public async static void CopyToStreamPath()
        {
            AssetsInternal.CopyBundleOperation op = AssetsInternal.CopyDirectory(outputPath,
                 AssetsInternal.CombinePath(Application.streamingAssetsPath, buildTarget));
            await op;
            AssetDatabase.Refresh();
        }

        public static string[] GetLegalDirectories(string path)
        {
            return Directory.GetDirectories(path, "*", SearchOption.TopDirectoryOnly)
                .Where(path => { return !IsIgnorePath(path); }).ToArray();
        }
        public static bool IsIgnorePath(string path)
        {
            var list = AssetsInternal.ToRegularPath(path).Split('/').ToList();
            if (!list.Contains("Assets") || list.Contains("Editor") || list.Contains("Resources")) return true;
            for (int i = 0; i < setting.ignoreFileExtend.Count; i++)
            {
                if (path.EndsWith(setting.ignoreFileExtend[i]))
                {
                    return true;
                }
            }
            return false;
        }
        public static string[] GetLegalFiles(string path)
        {
            var paths = Directory.GetFiles(path, "*", SearchOption.TopDirectoryOnly);
            var list = paths.ToList();
            list.RemoveAll(s =>
            {
                return IsIgnorePath(s);
            });
            return list.ToArray();
        }


    }
}
