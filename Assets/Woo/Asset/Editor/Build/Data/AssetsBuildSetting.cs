using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using System;

namespace WooAsset
{
    class AssetsBuildSetting : AssetsScriptableObject
    {
        [System.Serializable]
        public class TypeSelect
        {
            public string[] types;
            public string[] shortTypes;
            public int typeIndex;
            public Type baseType;
            public void Enable()
            {
                var list = baseType.GetSubTypesInAssemblies()
               .Where(type => !type.IsAbstract);
                types = list.Select(type => type.FullName).ToArray();
                shortTypes = list.Select(type => type.Name).ToArray();
            }
            public Type GetSelectType()
            {
                var type_str = types[typeIndex];
                Type type = baseType.GetSubTypesInAssemblies()
                   .Where(type => !type.IsAbstract)
                   .ToList()
                   .Find(x => x.FullName == type_str);

                return type;
            }

            public bool SetType(Type type)
            {
                string name = type.FullName;
                if (type.IsAbstract || !baseType.IsAssignableFrom(type)) return false;
                for (int i = 0; i < types.Length; i++)
                {
                    if (types[i] == name)
                    {
                        typeIndex = i;
                        return true;
                    }
                }
                return false;
            }
        }
        [HideInInspector] public TypeSelect buildGroup = new TypeSelect();
        [HideInInspector] public TypeSelect encrypt = new TypeSelect();
        [HideInInspector] public List<string> tags = new List<string>();

        public string version = "0.0.1";
        public long bundleSize = 8 * 1024 * 1024;
        public bool forceRebuild = false;
        public bool IgnoreTypeTreeChanges = true;

        public List<string> ignoreFileExtend = new List<string>() {
            ".cs",
            ".meta"
        };
        [SerializeField] public List<string> buildPaths = new List<string>();
        private void OnEnable()
        {
            encrypt.baseType = typeof(IAssetStreamEncrypt);
            buildGroup.baseType = typeof(ICollectBundle);
            buildGroup.Enable();
            encrypt.Enable();
        }
        public Type GetBuildGroupType()
        {
            return buildGroup.GetSelectType();
        }
        public Type GetStreamEncryptType()
        {
            return encrypt.GetSelectType();
        }
        public bool SetBuildGroupType(Type type)
        {
            if (buildGroup.SetType(type))
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




        public BuildAssetBundleOptions GetOption()
        {
            BuildAssetBundleOptions opt = BuildAssetBundleOptions.None;
            opt |= BuildAssetBundleOptions.StrictMode;
            if (forceRebuild)
                opt |= BuildAssetBundleOptions.ForceRebuildAssetBundle;
            if (IgnoreTypeTreeChanges)
                opt |= BuildAssetBundleOptions.IgnoreTypeTreeChanges;
            opt |= BuildAssetBundleOptions.DisableLoadAssetByFileName;
            opt |= BuildAssetBundleOptions.DisableLoadAssetByFileNameWithExtension;
            return opt;
        }

    }
}
