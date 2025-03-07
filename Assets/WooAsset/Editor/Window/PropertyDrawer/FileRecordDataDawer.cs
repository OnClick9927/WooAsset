using UnityEditor;
using UnityEngine;

namespace WooAsset
{
    [CustomPropertyDrawer(typeof(FileRecordData), true)]
    class FileRecordDataDawer : UnityEditor.PropertyDrawer
    {
        static GUIContent empty = new GUIContent();
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            GUI.enabled = false;

            var rs = AssetsWindow.RectEx.VerticalSplit(position, 100);
            var type = property.FindPropertyRelative(nameof(FileRecordData.type));
            var path = property.FindPropertyRelative(nameof(FileRecordData.path));

            EditorGUI.PropertyField(rs[0], type, empty);
            EditorGUI.PropertyField(rs[1], path, empty);
            GUI.enabled = true;

        }
    }
}
