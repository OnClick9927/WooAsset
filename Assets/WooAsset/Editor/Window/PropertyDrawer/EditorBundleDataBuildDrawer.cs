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
        private float GetPropertyHeightDown(SerializedProperty property)
        {
            float down;
            var packType = (PackType)property.FindPropertyRelative(nameof(EditorBundleDataBuild.packType)).enumValueIndex;
            if (packType == PackType.N2MBySize || packType == PackType.N2MBySizeAndDir
                || packType == PackType.N2MBySizeAndDirAndAssetType || packType == PackType.N2MByAssetTypeAndSize)
                down = 40;
            else
                down = 20;
            return down;
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

            float down = 20;

            var attr = Getattribute(index_property.intValue);
            AssetSelectorParamType paramType = attr.type;
            if (paramType.HasFlag(AssetSelectorParamType.AssetType))
                down += 20;
            if (paramType.HasFlag(AssetSelectorParamType.Path))
                down += 20;
            if (paramType.HasFlag(AssetSelectorParamType.UserData))
                down += 20;
            if (paramType.HasFlag(AssetSelectorParamType.Tag))
                down += 20;
            return (down);
        }
        private float GetPropertyHeightUp(SerializedProperty property)
        {
            var typeIndex = property.FindPropertyRelative(nameof(EditorBundleDataBuild.selectors));
            var size = typeIndex.arraySize;
            float down = 20;
            for (int i = 0; i < size; i++)
            {
                var param_property = typeIndex.GetArrayElementAtIndex(i);
                down += GetElementHeight(param_property);
            }
            return down;
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float down = GetPropertyHeightDown(property);
            float up = GetPropertyHeightUp(property);
            return up + down + 10;
        }
        private void DrawDwon(Rect position, SerializedProperty property)
        {
            var packType_pro = property.FindPropertyRelative(nameof(EditorBundleDataBuild.packType));
            var packType = (PackType)packType_pro.enumValueIndex;
            if (packType == PackType.N2MBySize || packType == PackType.N2MBySizeAndDir
                   || packType == PackType.N2MBySizeAndDirAndAssetType || packType == PackType.N2MByAssetTypeAndSize)
            {
                var rs_1 = RectEx.HorizontalSplit(position, 20);
                EditorGUI.PropertyField(rs_1[0], packType_pro);
                EditorGUI.PropertyField(rs_1[1], property.FindPropertyRelative(nameof(EditorBundleDataBuild.size)));
            }
            else
            {
                EditorGUI.PropertyField(position, packType_pro);
            }

        }

        private void DrawElement(Rect position, SerializedProperty param_property, SerializedProperty selector_property, int array_index)
        {
            var rs = RectEx.HorizontalSplit(position, 20);

            var index_property = param_property.FindPropertyRelative(nameof(AssetSelectorParam.typeIndex));

            var rss = RectEx.VerticalSplit(rs[0], rs[0].width - 20, 2);
            var rsss = RectEx.VerticalSplit(rss[0], rss[0].width - 100);
            var index = EditorGUI.Popup(rsss[0], "", index_property.intValue, EditorBundleDataBuild.shortTypes);
            if (index != index_property.intValue)
                index_property.intValue = index;
            var type_property = param_property.FindPropertyRelative(nameof(AssetSelectorParam.type));
            var _index = EditorGUI.Popup(rsss[1], type_property.intValue, type_property.enumDisplayNames);
            if (_index != type_property.intValue)
                type_property.intValue = _index;


            if (GUI.Button(rss[1], EditorGUIUtility.TrIconContent("d_Toolbar Minus"), EditorStyles.iconButton))
            {
                selector_property.DeleteArrayElementAtIndex(array_index);
            }



            var attr = Getattribute(index_property.intValue);
            position = RectEx.Zoom(rs[1], TextAnchor.MiddleRight, new Vector2(-50, 0));
            AssetSelectorParamType paramType = attr.type;
            //var param = property.FindPropertyRelative(nameof(EditorBundleDataBuild.param));
            if (paramType.HasFlag(AssetSelectorParamType.AssetType))
            {
                rs = RectEx.HorizontalSplit(position, 20);
                EditorGUI.PropertyField(rs[0], param_property.FindPropertyRelative(nameof(AssetSelectorParam.assetType)));
                position = rs[1];
            }
            if (paramType.HasFlag(AssetSelectorParamType.Path))
            {
                rs = RectEx.HorizontalSplit(position, 20);
                EditorGUI.PropertyField(rs[0], param_property.FindPropertyRelative(nameof(AssetSelectorParam.path)));
                position = rs[1];
            }
            if (paramType.HasFlag(AssetSelectorParamType.Tag))
            {
                rs = RectEx.HorizontalSplit(position, 20);
                EditorGUI.PropertyField(rs[0], param_property.FindPropertyRelative(nameof(AssetSelectorParam.tag)));
                position = rs[1];
            }
            if (paramType.HasFlag(AssetSelectorParamType.UserData))
            {
                rs = RectEx.HorizontalSplit(position, 20);
                EditorGUI.PropertyField(rs[0], param_property.FindPropertyRelative(nameof(AssetSelectorParam.userData)));
                position = rs[1];
            }

        }
        private void DrawUp(Rect position, SerializedProperty property)
        {
            var _rs = RectEx.HorizontalSplit(position, 20);


            var selector_property = property.FindPropertyRelative(nameof(EditorBundleDataBuild.selectors));
            var rs_first = RectEx.VerticalSplit(_rs[0], _rs[0].width - 20);

            GUI.Label(rs_first[0], nameof(EditorBundleDataBuild.selectors));

            if (GUI.Button(rs_first[1], EditorGUIUtility.TrIconContent("d_Toolbar Plus"), EditorStyles.iconButton))
            {
                selector_property.InsertArrayElementAtIndex(0);
            }



            var _pos = RectEx.Zoom(_rs[1], TextAnchor.MiddleRight,new Vector2(-50,0));





            var size = selector_property.arraySize;
            for (int i = size - 1; i >= 0; i--)
            {
                var index = i;
                var param_property = selector_property.GetArrayElementAtIndex(index);
                var height = GetElementHeight(param_property);
                var rs = RectEx.HorizontalSplit(_pos, height);
                DrawElement(rs[0], param_property, selector_property, i);
                _pos = rs[1];
            }




        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            position = RectEx.Zoom(position, TextAnchor.MiddleCenter, -8);
            var rs = RectEx.HorizontalSplit(position, position.height - GetPropertyHeightDown(property));


            GUI.Box(rs[0], "", EditorStyles.helpBox);
            GUI.Box(rs[1], "");

            DrawDwon(rs[1], property);
            DrawUp(rs[0], property);

        }
    }
}
