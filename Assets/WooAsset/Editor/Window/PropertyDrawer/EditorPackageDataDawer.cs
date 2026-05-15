using System;
using UnityEditor;
using UnityEngine;
using static WooAsset.AssetsEditorTool;
using static WooAsset.AssetsWindow;

namespace WooAsset
{
    [CustomPropertyDrawer(typeof(EditorPackageData), true)]
    class EditorPackageDataDawer : UnityEditor.PropertyDrawer
    {
        static GUIContent empty = new GUIContent();
        static GUIContent tags = new GUIContent(nameof(EditorPackageData.tags));
        static GUIContent paths = new GUIContent(nameof(EditorPackageData.paths));
        static GUIContent builds = new GUIContent(nameof(EditorPackageData.rules));

        private float GetArrayPropertyHeight(SerializedProperty property, string name)
        {
            var paths = property.FindPropertyRelative(name);
            if (!paths.isExpanded)
                return 20;
            else
                return 50 + Mathf.Max(1, paths.arraySize) * 20;
        }


        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {

            var height = 40 + GetArrayPropertyHeight(property, nameof(EditorPackageData.tags))
                + GetArrayPropertyHeight(property, nameof(EditorPackageData.paths))
            + GetArrayPropertyHeight(property, nameof(EditorPackageData.rules));
            return height;
        }


        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var build = property.FindPropertyRelative(nameof(EditorPackageData.build));
            var name = property.FindPropertyRelative(nameof(EditorPackageData.name));

            var paths = property.FindPropertyRelative(nameof(EditorPackageData.paths));
            var builds = property.FindPropertyRelative(nameof(EditorPackageData.rules));
            var tags = property.FindPropertyRelative(nameof(EditorPackageData.tags));

            var rs = RectEx.HorizontalSplit(position, 25);
            var rss = RectEx.VerticalSplit(rs[0], 150);
            var rsss = RectEx.VerticalSplit(rss[1], rss[1].width - 60, 10);
            GUI.Label(rss[0], nameof(EditorPackageData.name));
            rsss[0].height = 18;
            EditorGUI.PropertyField(rsss[0], name, empty);
            build.boolValue = GUI.Toggle(rsss[1], build.boolValue, nameof(EditorPackageData.build), EditorStyles.toggleGroup);


            rs = RectEx.HorizontalSplit(rs[1], GetArrayPropertyHeight(property, nameof(EditorPackageData.paths)), 8);


            EditorGUI.PropertyField(rs[0], paths, EditorPackageDataDawer.paths);

            rs = RectEx.HorizontalSplit(rs[1], GetArrayPropertyHeight(property, nameof(EditorPackageData.tags)), 8);


            EditorGUI.PropertyField(rs[0], tags, EditorPackageDataDawer.tags);

            EditorGUI.PropertyField(rs[1], builds, EditorPackageDataDawer.builds);
        }
    }
}
