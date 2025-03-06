using UnityEditor;
using UnityEngine;

namespace WooAsset
{
    [CustomPropertyDrawer(typeof(TagAssets.TagData), true)]
    class TagAssets_TagDataDawer : UnityEditor.PropertyDrawer
    {
        static GUIContent empty = new GUIContent();
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            GUI.enabled = false;
            var rs = AssetsWindow.RectEx.VerticalSplit(position, 100);
            var type = property.FindPropertyRelative(nameof(TagAssets.TagData.type));
            var path = property.FindPropertyRelative(nameof(TagAssets.TagData.path));

            EditorGUI.PropertyField(rs[0], type, empty);
            EditorGUI.PropertyField(rs[1], path, empty);
            GUI.enabled = true;

        }
    }
}
