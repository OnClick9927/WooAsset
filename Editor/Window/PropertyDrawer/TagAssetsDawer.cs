using UnityEditor;
using UnityEngine;
using static WooAsset.AssetsEditorTool;

namespace WooAsset
{
    [CustomPropertyDrawer(typeof(TagAssets), true)]
    class TagAssetsDawer : UnityEditor.PropertyDrawer
    {
        static GUIContent empty = new GUIContent();
        static GUIContent assest = new GUIContent("Assets");

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var assets = property.FindPropertyRelative(nameof(TagAssets.assets));
            var _base = base.GetPropertyHeight(assets, label);
            if (!assets.isExpanded)
                return _base;
            return assets.arraySize * _base + 60;
        }
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var rs = RectEx.VerticalSplit(position, 150);
            var tag = property.FindPropertyRelative(nameof(TagAssets.tag));
            var assets = property.FindPropertyRelative(nameof(TagAssets.assets));
            if (!assets.isExpanded)
                EditorGUI.PropertyField(rs[0], tag, empty);
            else
            {
                var rss = RectEx.HorizontalSplit(rs[0], 20);
                EditorGUI.PropertyField(rss[0], tag, empty);
            }
            //GUI.enabled = false;
            EditorGUI.PropertyField(rs[1], assets, TagAssetsDawer.assest);
            //GUI.enabled = true;

        }
    }
}
