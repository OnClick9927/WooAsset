using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using static WooAsset.AssetsEditorTool;

namespace WooAsset
{
    [CustomEditor(typeof(BundleRule))]
    class BundleRuleEditor : Editor
    {

        [CustomPropertyDrawer(typeof(AssetSelectorParam))]
        class AssetSelectorParamDrawer : PropertyDrawer
        {
            private static Dictionary<int, AssetSelectorAttribute> map = new Dictionary<int, AssetSelectorAttribute>();
            private AssetSelectorAttribute Getattribute(int index)
            {
                if (!map.ContainsKey(index))
                {
                    var type = BundleRule.GetSelectType(index);
                    var attr = type.GetCustomAttribute<AssetSelectorAttribute>(false);
                    map.Add(index, attr);
                }
                return map[index];
            }
            public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
            {
                var index_property = property.FindPropertyRelative(nameof(AssetSelectorParam.typeIndex));

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

            public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
            {
                DrawElement(position, property);
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
                var index = EditorGUI.Popup(rss[0], "", index_property.intValue, BundleRule.shortTypes);
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

        }

        private BundleRule rule => target as BundleRule;

        public override void OnInspectorGUI()
        {
            serializedObject.UpdateIfRequiredOrScript();



            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(BundleRule.selectors)));



            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(BundleRule.packType)));

            if (rule.packType.ToString().Contains("Size"))
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(BundleRule.size)));

            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}
