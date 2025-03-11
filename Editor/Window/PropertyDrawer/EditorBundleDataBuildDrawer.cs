using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using static WooAsset.AssetsWindow;
using static WooAsset.EditorBundleDataBuild;

namespace WooAsset
{
    [CustomPropertyDrawer(typeof(EditorBundleDataBuild))]
    class EditorBundleDataBuildDrawer : PropertyDrawer
    {
        static GUIContent selectors = new GUIContent("Selectors");

        private float GetPropertyHeightRight(SerializedProperty property)
        {
            float height;
            var packType = (PackType)property.FindPropertyRelative(nameof(EditorBundleDataBuild.packType)).enumValueIndex;
            if (packType == PackType.N2MBySize || packType == PackType.N2MBySizeAndDir
                || packType == PackType.N2MBySizeAndDirAndAssetType || packType == PackType.N2MByAssetTypeAndSize)
                height = 40;
            else
                height = 20;
            return height + 20;
        }
        private static Dictionary<int, AssetSelectorAttribute> map = new Dictionary<int, AssetSelectorAttribute>();
        private AssetSelectorAttribute Getattribute(int index)
        {
            if (!map.ContainsKey(index))
            {
                var type = GetSelectType(index);
                var attr = type.GetCustomAttribute<AssetSelectorAttribute>(false);
                map.Add(index, attr);
            }
            return map[index];
        }

        private float GetElementHeight(SerializedProperty param_property)
        {
            var index_property = param_property.FindPropertyRelative(nameof(AssetSelectorParam.typeIndex));

            float height = 20;

            var attr = Getattribute(index_property.intValue);
            AssetSelectorParamType paramType = attr.type;
            if (paramType.HasFlag(AssetSelectorParamType.AssetType))
                height += 20;
            if (paramType.HasFlag(AssetSelectorParamType.Path))
                height += 20;
            if (paramType.HasFlag(AssetSelectorParamType.UserData))
                height += 20;
            if (paramType.HasFlag(AssetSelectorParamType.Tag))
                height += 20;
            return height;
        }
        private float GetPropertyHeightLeft(SerializedProperty property)
        {
            var selectors = property.FindPropertyRelative(nameof(EditorBundleDataBuild.selectors));

            var size = selectors.arraySize;
            float down = 20;
            if (selectors.isExpanded)
                for (int i = 0; i < size; i++)
                {
                    var param_property = selectors.GetArrayElementAtIndex(i);
                    down += GetElementHeight(param_property);
                }
            return down;
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float down = GetPropertyHeightRight(property);
            float up = GetPropertyHeightLeft(property);
            return Mathf.Max(down, up) + 5;
        }

        private void DrawRightProperty(Rect position, SerializedProperty property)
        {
            var rs = RectEx.VerticalSplit(position, 80);
            GUI.Label(rs[0], property.displayName);
            EditorGUI.PropertyField(rs[1], property, empty);

        }
        private void DrawRight(Rect position, SerializedProperty property)
        {
            position = RectEx.HorizontalSplit(position, 60)[0];
            var packType_pro = property.FindPropertyRelative(nameof(EditorBundleDataBuild.packType));
            var packType = (PackType)packType_pro.enumValueIndex;
            var rs_1 = RectEx.HorizontalSplit(position, 20);
            GUI.Label(rs_1[0], "Pack", EditorStyles.toolbarButton);
            position = rs_1[1];
            rs_1 = RectEx.HorizontalSplit(position, 20);
            DrawRightProperty(rs_1[0], packType_pro);
            if (packType == PackType.N2MBySize || packType == PackType.N2MBySizeAndDir
                   || packType == PackType.N2MBySizeAndDirAndAssetType || packType == PackType.N2MByAssetTypeAndSize)
            {
                DrawRightProperty(rs_1[1], property.FindPropertyRelative(nameof(EditorBundleDataBuild.size)));
            }
        }
        static GUIContent empty = new GUIContent();
        private void DrawFlag(ref Rect position, SerializedProperty param_property, string propertyPath, AssetSelectorParamType type)
        {
            var rs = RectEx.HorizontalSplit(position, 20);
            position = rs[1];
            var _property = param_property.FindPropertyRelative(propertyPath);
            var _rs = RectEx.VerticalSplit(rs[0], 80);
            GUI.Label(_rs[0], type.ToString());
            EditorGUI.PropertyField(_rs[1], _property, empty);
        }
        private void DrawElement(Rect position, SerializedProperty param_property)
        {
            var rs = RectEx.HorizontalSplit(position, 20);
            position = RectEx.Zoom(rs[1], TextAnchor.MiddleCenter, new Vector2(-40, 0));
            var index_property = param_property.FindPropertyRelative(nameof(AssetSelectorParam.typeIndex));
            var rss = RectEx.VerticalSplit(rs[0], rs[0].width - 80, 2);
            //var rsss = RectEx.VerticalSplit(rss[0], rss[0].width - 80);
            var index = EditorGUI.Popup(rss[0], "", index_property.intValue, EditorBundleDataBuild.shortTypes);
            if (index != index_property.intValue)
                index_property.intValue = index;
            var type_property = param_property.FindPropertyRelative(nameof(AssetSelectorParam.type));
            var _index = EditorGUI.Popup(rss[1], type_property.intValue, type_property.enumDisplayNames);
            if (_index != type_property.intValue)
                type_property.intValue = _index;
            var attr = Getattribute(index_property.intValue);
            AssetSelectorParamType paramType = attr.type;
            if (paramType.HasFlag(AssetSelectorParamType.AssetType))
            {
                DrawFlag(ref position, param_property, nameof(AssetSelectorParam.assetType), AssetSelectorParamType.AssetType);
            }
            if (paramType.HasFlag(AssetSelectorParamType.Path))
            {
                DrawFlag(ref position, param_property, nameof(AssetSelectorParam.path), AssetSelectorParamType.Path);
            }
            if (paramType.HasFlag(AssetSelectorParamType.Tag))
            {
                DrawFlag(ref position, param_property, nameof(AssetSelectorParam.tag), AssetSelectorParamType.Tag);
            }
            if (paramType.HasFlag(AssetSelectorParamType.UserData))
            {
                DrawFlag(ref position, param_property, nameof(AssetSelectorParam.userData), AssetSelectorParamType.UserData);
            }

        }
        private void DrawLeft(Rect position, SerializedProperty property)
        {
            var selector_property = property.FindPropertyRelative(nameof(EditorBundleDataBuild.selectors));
            EditorPackageDataDawer.DrawArray(position, selectors, selector_property, (p) =>
            {
                return GetElementHeight(p);
            }, DrawElement);


        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            //position = RectEx.Zoom(position, TextAnchor.MiddleCenter, -8);
            var rs = RectEx.VerticalSplit(position, position.width - 300, 10);


            //GUI.Box(rs[0], "", EditorStyles.helpBox);
            GUI.Box(position, "", EditorStyles.helpBox);

            DrawLeft(rs[0], property);
            DrawRight(rs[1], property);

        }
    }
}
