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
        private static string[] types;
        private static Type[] realTypes;
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
        public List<AssetSelectorParam> selectors;
        public static System.Type baseType = typeof(IAssetSelector);
        public enum PackType
        {
            One2One,
            N2One,
            N2MBySize,
            N2MByDir,
            N2MByAssetType,



            N2MBySizeAndDir,
            N2MByDirAndAssetType,
            N2MByAssetTypeAndSize,

            N2MBySizeAndDirAndAssetType,
        }


        public PackType packType;
        public long size = 2097152;
        public static System.Type GetSelectType(int typeIndex)
        {
            if (_shortTypes == null)
                SearchTypes();
            var type_str = types[typeIndex];
            System.Type type = realTypes
               .Where(type => !type.IsAbstract)
               .ToList()
               .Find(x => x.FullName == type_str);

            return type;
        }


        private static void SearchTypes()
        {
            var list = AssetsEditorTool.GetSubTypesInAssemblies(baseType)
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
            realTypes = list.ToArray();
            types = list.Select(type => type.FullName).ToArray();
            _shortTypes = list.Select(type => type.Name).ToArray();
        }

        public void Build(List<EditorAssetData> assets, List<EditorBundleData> result)
        {
            IEnumerable<EditorAssetData> selected = new List<EditorAssetData>();

            var _result = this.selectors.Select(param =>
                  {
                      return new { param = param, selector = Activator.CreateInstance(GetSelectType(param.typeIndex)) as IAssetSelector };
                  }).Select(x =>
                  {
                      var _assets = x.selector.Select(assets, x.param);
                      return new { x.param.type, _assets };
                  });


            var union = _result.Where(x => x.type == AssetSelectorParam.SelectType.Union).SelectMany(x => x._assets);
            if (union.Count() == 0)
                union = new List<EditorAssetData>(assets);
            selected = selected.Union(union);

            var intersect = _result.Where(x => x.type == AssetSelectorParam.SelectType.Intersect).Select(x => x._assets);
            foreach (var item in intersect)
                selected = selected.Intersect(item);

            var list = selected.Distinct().ToList();
            assets.RemoveAll(x => list.Contains(x));
            switch (packType)
            {
                case PackType.One2One:
                    EditorBundleTool.One2One(list, result); break;
                case PackType.N2One:
                    EditorBundleTool.N2One(list, result); break;
                case PackType.N2MBySize:
                    EditorBundleTool.N2MBySize(list, result, size); break;
                case PackType.N2MBySizeAndDir:
                    EditorBundleTool.N2MBySizeAndDir(list, result, size); break;
                case PackType.N2MByAssetType:
                    EditorBundleTool.N2MByAssetType(list, result); break;
                case PackType.N2MByAssetTypeAndSize:
                    EditorBundleTool.N2MByAssetTypeAndSize(list, result, size); break;
                case PackType.N2MBySizeAndDirAndAssetType:
                    EditorBundleTool.N2MBySizeAndDirAndAssetType(list, result, size); break;

                case PackType.N2MByDir:
                    EditorBundleTool.N2MByDir(list, result); break;
                case PackType.N2MByDirAndAssetType:
                    EditorBundleTool.N2MByDirAndAssetType(list, result); break;
            }
        }
    }
}
