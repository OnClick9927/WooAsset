using System;
using System.Collections.Generic;
using System.Linq;
using Object = UnityEngine.Object;

namespace WooAsset
{
    public class AssetsBuildOption : AssetsScriptableObject
    {
        [System.Serializable]
        public class ModeOption
        {
            public TypeSelect mode = new TypeSelect();

            public string[] Folders = new string[] { };
            public bool CheckAssetType = false;
            public enum SpeedType
            {
                B, KB, MB, GB
            }
            public SpeedType speedType = SpeedType.MB;
            public int loadSpeed = 1024;
            public long GetEditorWaitTime(long length)
            {
                length /= loadSpeed;
                switch (speedType)
                {
                    case SpeedType.B: return length;
                    case SpeedType.KB: return length / 1024;
                    case SpeedType.MB: return length / 1024 / 1024;
                    case SpeedType.GB: return length / 1024 / 1024 / 1024;
                    default:
                        return length;
                }
            }
            internal void OnEnable()
            {
                if (mode.baseType == null)
                {
                    mode.baseType = typeof(IAssetsMode);
                    mode.Enable();
                }
            }
        }

        //[System.Serializable]
        //public class SpriteAtlasOption
        //{
        //    public List<string> atlasPaths = new List<string>();
        //    public PackingSetting packSetting = new PackingSetting();
        //    public TextureSetting textureSetting = new TextureSetting();
        //    public TextureImporterPlatformSettings PlatformSetting = new TextureImporterPlatformSettings()
        //    {
        //        maxTextureSize = 2048,
        //        format = TextureImporterFormat.Automatic,
        //        crunchedCompression = true,
        //        textureCompression = TextureImporterCompression.Compressed,
        //        compressionQuality = 50,
        //    };

        //}

        [System.Serializable]
        public class SimulatorServerOption
        {
            public bool enable;
            public int port = 8080;

            public enum SpeedType
            {
                B, KB, MB
            }
            public SpeedType speedType;
            public int speed = 1024;

            public int GetSpeed()
            {
                switch (speedType)
                {
                    case SpeedType.B: return speed;
                    case SpeedType.KB: return speed * 1024;
                    case SpeedType.MB: return speed * 1024 * 1024;
                    default:
                        return speed;
                }
            }
        }
        [System.Serializable]
        public class BuildInOption
        {
            public bool copyToStream = false;
            public List<Object> assets = new List<Object>();
            public TypeSelect selector = new TypeSelect();

            internal void OnEnable()
            {
                if (selector.baseType == null)
                {
                    selector.baseType = typeof(IBuildInBundleSelector);
                    selector.Enable();
                }
            }

        }
        [System.Serializable]
        public class BundleOptimizeOption
        {
            public TypeSelect optimizer = new TypeSelect();
            public int count;
            internal void OnEnable()
            {
                if (optimizer.baseType == null)
                {
                    optimizer.baseType = typeof(IBundleOptimizer);
                    optimizer.Enable();
                }
            }
        }

        //public ShaderOption shader = new ShaderOption();
        public ModeOption mode = new ModeOption();
        //public SpriteAtlasOption spriteAtlas = new SpriteAtlasOption();
        public SimulatorServerOption server = new SimulatorServerOption();
        public BuildInOption buildIn = new BuildInOption();
        public BundleOptimizeOption bundleOptimize = new BundleOptimizeOption();





        public bool ClearAssetCache = true;

        public string version = "0.0.1";
        public BuildMode buildMode = BuildMode.Increase;
        public TypeTreeOption typeTreeOption = TypeTreeOption.IgnoreTypeTreeChanges;
        public BundleNameType bundleNameType = BundleNameType.Hash;
        public BundleNameCalculateType bundleNameCalculate = BundleNameCalculateType.Assets_And_Dependences;
        public CompressType compress = CompressType.LZ4;
        public int MaxCacheVersionCount = 8;
        public List<EditorPackageData> pkgs = new List<EditorPackageData>();
        public TypeSelect build = new TypeSelect();
        public TypeSelect encrypt = new TypeSelect();
        public TypeSelect buildPipeline = new TypeSelect();

        [System.Serializable]
        public class RecordOption
        {
            public enum RecordType
            {
                Ignore,
                Record,
            }
            public RecordType type = RecordType.Ignore;
            public List<FileRecordData> records = new List<FileRecordData>();

            public void OnEnable()
            {
                records.RemoveAll(x =>

(x.type == FileType.File && !AssetsEditorTool.ExistsFile(x.path)) ||
(x.type == FileType.Directory && !AssetsEditorTool.ExistsDirectory(x.path)));

            }
        }
        public RecordOption record = new RecordOption();
        public List<TagAssets> tags = new List<TagAssets>();

        protected override void OnLoad()
        {
            OnEnable();
        }

        private void OnEnable()
        {
            buildIn.OnEnable();
            bundleOptimize.OnEnable();
            mode.OnEnable();
            if (encrypt.baseType == null)
            {
                encrypt.baseType = typeof(IAssetEncrypt);
                encrypt.Enable();
            }
            if (build.baseType == null)
            {
                build.baseType = typeof(IAssetsBuild);
                build.Enable();
            }


            if (buildPipeline.baseType == null)
            {
                buildPipeline.baseType = typeof(IBuildPipeLine);
                buildPipeline.Enable();
            }
            record.OnEnable();
            tags.ForEach(z => z.assets.RemoveAll(x => (x.type == FileType.File && !AssetsEditorTool.ExistsFile(x.path)) ||
      (x.type == FileType.Directory && !AssetsEditorTool.ExistsDirectory(x.path))));
        }




        public Type GetAssetBuildType() => build.GetSelectType();
        public Type GetStreamEncryptType() => encrypt.GetSelectType();
        public Type GetAssetModeType() => mode.mode.GetSelectType();
        public Type GetBuildInBundleSelectorType() => buildIn.selector.GetSelectType();
        public Type GetBuildPipelineType() => buildPipeline.GetSelectType();
        public bool SetBundleOptimizerType(Type type)
        {
            if (bundleOptimize.optimizer.SetType(type))
            {
                Save();
                return true;
            }
            return false;
        }
        public Type GetBundleOptimizerType() => bundleOptimize.optimizer.GetSelectType();


        public bool SetAssetBuildType(Type type)
        {
            if (build.SetType(type))
            {
                Save();
                return true;
            }
            return false;
        }
        public bool SetStreamEncryptType(Type type)
        {
            if (encrypt.SetType(type))
            {
                Save();
                return true;
            }
            return false;
        }
        public bool SetBuildInBundleSelectorType(Type type)
        {
            if (buildIn.selector.SetType(type))
            {
                Save();
                return true;
            }
            return false;
        }
        public bool SetBuildPipelineType(Type type)
        {
            if (buildPipeline.SetType(type))
            {
                Save();
                return true;
            }
            return false;
        }



        public void AddToRecord(string path, FileType type)
        {
            if (record.records.Any(x => x.path == path && x.type == type)) return;
            record.records.Add(new FileRecordData() { type = type, path = path });
        }
        public void RemoveFromRecord(string path, FileType type) => record.records.RemoveAll(x => x.path == path && x.type == type);




        public List<string> GetAllTags() => tags.ConvertAll(x => x.tag);
        public void AddAssetTag(string path, FileType type, string tag)
        {
            if (tags == null) tags = new List<TagAssets>();
            TagAssets assets = tags.Find(x => x.tag == tag);
            if (assets == null)
            {
                assets = new TagAssets();
                tags.Add(assets);
            }
            assets.Add(type, path);

        }
        public void RemoveAssetTag(string path, FileType type, string tag)
        {
            if (tags == null) return;
            TagAssets assets = tags.Find(x => x.tag == tag);
            if (assets == null) return;
            assets.Remove(type, path);

        }
    }
}
