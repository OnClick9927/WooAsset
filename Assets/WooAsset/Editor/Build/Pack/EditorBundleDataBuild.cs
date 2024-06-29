using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using static WooAsset.AssetsEditorTool;

namespace WooAsset
{
    [System.Serializable]
    public class EditorBundleDataBuild
    {
        private IAssetSelector selector;
        private static string[] types;
        private static string[] _shortTypes;
        public static string[] shortTypes
        {
            get
            {
                if (_shortTypes == null)
                    SearchTypes();
                return _shortTypes;
            }
        }
        public AssetSelectorParam param = new AssetSelectorParam();
        public int typeIndex;
        public static System.Type baseType = typeof(IAssetSelector);
        public enum PackType
        {
            One2One,
            N2One,
            N2MBySize,
            N2MBySizeAndDir,
        }
        public PackType packType;
        public long size = 2097152;
        public static System.Type GetSelectType(int typeIndex)
        {
            if (_shortTypes == null)
                SearchTypes();
            var type_str = types[typeIndex];
            System.Type type = TypeSelect.GetSubTypesInAssemblies(baseType)
               .Where(type => !type.IsAbstract)
               .ToList()
               .Find(x => x.FullName == type_str);

            return type;
        }


        private static void SearchTypes()
        {
            var list = TypeSelect.GetSubTypesInAssemblies(baseType)
           .Where(type => !type.IsAbstract).ToList();
            list.Sort((x, y) =>
            {
                var name_x = x.FullName;
                var name_y = y.FullName;
                var len = Mathf.Min(name_x.Length, name_y.Length);
                for (var i = 0; i < len; i++)
                {
                    if (name_x[i] < name_y[i])
                    {
                        return 1;
                    }
                }
                return 1;
            });
            types = list.Select(type => type.FullName).ToArray();
            _shortTypes = list.Select(type => type.Name).ToArray();
        }

        public void Build(List<EditorAssetData> assets, List<EditorBundleData> result)
        {
            selector = Activator.CreateInstance(GetSelectType(this.typeIndex)) as IAssetSelector;
            var s = selector.Select(assets, param);
            assets.RemoveAll(x => s.Contains(x));
            switch (packType)
            {
                case PackType.One2One:
                    EditorBundleTool.One2One(s, result); break;
                case PackType.N2One:
                    EditorBundleTool.N2One(s, result); break;
                case PackType.N2MBySize:
                    EditorBundleTool.N2MBySize(s, result, size); break;
                case PackType.N2MBySizeAndDir:
                    EditorBundleTool.N2MBySize(s, result, size); break;
            }
        }
    }
}
