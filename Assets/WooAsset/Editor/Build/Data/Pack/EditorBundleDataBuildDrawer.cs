using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using static WooAsset.AssetsWindow;
using static WooAsset.EditorBundleDataBuild;

namespace WooAsset
{
    [CustomPropertyDrawer(typeof(EditorBundleDataBuild))]
    public class EditorBundleDataBuildDrawer : PropertyDrawer
    {
        private float GetPropertyHeightDown(SerializedProperty property)
        {
            float down;
            var packType = (PackType)property.FindPropertyRelative(nameof(EditorBundleDataBuild.packType)).enumValueIndex;
            if (packType == PackType.N2MBySize || packType == PackType.N2MBySizeAndDir)
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
        private float GetPropertyHeightUp(SerializedProperty property)
        {
            float down = 20;
            var typeIndex = property.FindPropertyRelative(nameof(EditorBundleDataBuild.typeIndex));
            var attr = Getattribute(typeIndex.intValue);
            AssetSelectorParamType paramType = attr.type;
            if (paramType.HasFlag(AssetSelectorParamType.AssetType))
                down += 20;
            if (paramType.HasFlag(AssetSelectorParamType.Path))
                down += 20;
            if (paramType.HasFlag(AssetSelectorParamType.UserData))
                down += 20;
            if (paramType.HasFlag(AssetSelectorParamType.Tag))
                down += 20;
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
            if (packType == PackType.N2MBySize || packType == PackType.N2MBySizeAndDir)
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
        private void DrawUp(Rect position, SerializedProperty property)
        {
            var typeIndex = property.FindPropertyRelative(nameof(EditorBundleDataBuild.typeIndex));
            var rs = RectEx.HorizontalSplit(position, 20);
            var index = EditorGUI.Popup(rs[0], "Selector Type", typeIndex.intValue, EditorBundleDataBuild.shortTypes);
            if (index != typeIndex.intValue)
            {
                typeIndex.intValue = index;
                //typeIndex.serializedObject.ApplyModifiedProperties();
            }
            var attr = Getattribute(typeIndex.intValue);
            position = rs[1];
            AssetSelectorParamType paramType = attr.type;
            var param = property.FindPropertyRelative(nameof(EditorBundleDataBuild.param));
            if (paramType.HasFlag(AssetSelectorParamType.AssetType))
            {
                rs = RectEx.HorizontalSplit(position, 20);
                EditorGUI.PropertyField(rs[0], param.FindPropertyRelative(nameof(AssetSelectorParam.type)));
                position = rs[1];
            }
            if (paramType.HasFlag(AssetSelectorParamType.Path))
            {
                rs = RectEx.HorizontalSplit(position, 20);
                EditorGUI.PropertyField(rs[0], param.FindPropertyRelative(nameof(AssetSelectorParam.path)));
                position = rs[1];
            }
            if (paramType.HasFlag(AssetSelectorParamType.Tag))
            {
                rs = RectEx.HorizontalSplit(position, 20);
                EditorGUI.PropertyField(rs[0], param.FindPropertyRelative(nameof(AssetSelectorParam.tag)));
                position = rs[1];
            }
            if (paramType.HasFlag(AssetSelectorParamType.UserData))
            {
                rs = RectEx.HorizontalSplit(position, 20);
                EditorGUI.PropertyField(rs[0], param.FindPropertyRelative(nameof(AssetSelectorParam.userData)));
                position = rs[1];
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
