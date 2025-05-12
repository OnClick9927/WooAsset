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
        static GUIContent builds = new GUIContent(nameof(EditorPackageData.builds));
        static EditorBundleDataBuildDrawer draw = new EditorBundleDataBuildDrawer();


        public static void DrawArray(Rect pos, GUIContent content, SerializedProperty tags, Func<SerializedProperty, float> getheight, Action<Rect, SerializedProperty> drawEle)
        {
            var _rs = RectEx.HorizontalSplit(pos, 20);
            GUI.Label(pos, "", EditorStyles.toolbar);
            var rs_first = RectEx.VerticalSplit(_rs[0], _rs[0].width - 60, 10);
            var rs_second = RectEx.VerticalSplit(rs_first[0], rs_first[0].width - 20);
            tags.isExpanded = EditorGUI.Foldout(rs_second[0], tags.isExpanded, content, true);

            //if (EditorGUI.DropdownButton(rs_second[0], content, FocusType.Passive, EditorStyles.toolbarPopup))
            //{
            //    tags.isExpanded = !tags.isExpanded;
            //}
            var _size = tags.arraySize;
            var tmp = EditorGUI.IntField(rs_first[1], _size);

            if (_size != tmp)
            {
                if (_size < tmp)
                    for (int i = 0; i < tmp - _size; i++)
                    {
                        tags.InsertArrayElementAtIndex(0);
                    }
                else
                    for (int i = 0; i < _size - tmp; i++)
                    {
                        tags.DeleteArrayElementAtIndex(0);

                    }
            }

            if (GUI.Button(rs_second[1], EditorGUIUtility.TrIconContent("d_Toolbar Plus"), EditorStyles.toolbarButton))
            {
                tags.InsertArrayElementAtIndex(0);
            }
            if (!tags.isExpanded) return;


            var _pos = RectEx.Zoom(_rs[1], TextAnchor.MiddleRight, new Vector2(-10, 0));
            var size = tags.arraySize;
            for (int i = size - 1; i >= 0; i--)
            {
                var index = i;
                var _property = tags.GetArrayElementAtIndex(index);

                _rs = RectEx.HorizontalSplit(_pos, getheight.Invoke(_property));
                var rss = RectEx.VerticalSplit(_rs[0], _rs[0].width - 20, 2);
                drawEle?.Invoke(rss[0], _property);

                if (GUI.Button(rss[1], EditorGUIUtility.TrIconContent("d_Toolbar Minus"), EditorStyles.iconButton))
                {
                    tags.DeleteArrayElementAtIndex(index);
                }
                _pos = _rs[1];
            }
        }
        private float GetPathsHeight(SerializedProperty property)
        {
            var paths = property.FindPropertyRelative(nameof(EditorPackageData.paths));
            var height = 0;
            if (!paths.isExpanded)
            {
                height += 20;
            }
            else
            {
                height += 20 + paths.arraySize * 20;
            }
            return height;
        }
        private float GetTagsHeight(SerializedProperty property)
        {
            var paths = property.FindPropertyRelative(nameof(EditorPackageData.tags));
            var height = 0;
            if (!paths.isExpanded)
            {
                height += 20;
            }
            else
            {
                height += 20 + paths.arraySize * 20;
            }
            return height;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var tags = property.FindPropertyRelative(nameof(EditorPackageData.tags));
            var description = property.FindPropertyRelative(nameof(EditorPackageData.description));
            var builds = property.FindPropertyRelative(nameof(EditorPackageData.builds));

            var height = 40 + GetPathsHeight(property) + GetTagsHeight(property);
            if (builds.isExpanded)
            {
                for (int i = 0; i < builds.arraySize; i++)
                {
                    height += draw.GetPropertyHeight(builds.GetArrayElementAtIndex(i), null);
                }
            }
            return height + 20 + 20;
        }
        private void DrawLeft(Rect pos, SerializedProperty property)
        {
            var build = property.FindPropertyRelative(nameof(EditorPackageData.build));
            var name = property.FindPropertyRelative(nameof(EditorPackageData.name));
            var description = property.FindPropertyRelative(nameof(EditorPackageData.description));

            var paths = property.FindPropertyRelative(nameof(EditorPackageData.paths));
            var builds = property.FindPropertyRelative(nameof(EditorPackageData.builds));
            var tags = property.FindPropertyRelative(nameof(EditorPackageData.tags));

            var rs = RectEx.HorizontalSplit(pos, 20);
            var rss = RectEx.VerticalSplit(rs[0], 150);
            var rsss = RectEx.VerticalSplit(rss[1], rss[1].width - 60, 10);
            GUI.Label(rss[0], nameof(EditorPackageData.name));
            EditorGUI.PropertyField(rsss[0], name, empty);
            build.boolValue = GUI.Toggle(rsss[1], build.boolValue, nameof(EditorPackageData.build), EditorStyles.toggleGroup);
            rs = RectEx.HorizontalSplit(rs[1], 20, 5);
            rss = RectEx.VerticalSplit(rs[0], 150);
            GUI.Label(rss[0], nameof(EditorPackageData.description));
            EditorGUI.PropertyField(rss[1], description, empty);

            rs = RectEx.HorizontalSplit(rs[1], GetPathsHeight(property), 8);

            DrawArray(rs[0], EditorPackageDataDawer.paths, paths, (p) => { return 20; }, (pos, _property) =>
            {
                EditorGUI.PropertyField(pos, _property, empty);
            });
            rs = RectEx.HorizontalSplit(rs[1], GetTagsHeight(property), 8);
            DrawArray(rs[0], EditorPackageDataDawer.tags, tags, (p) => { return 20; }, (pos, _property) =>
            {
                EditorGUI.PropertyField(pos, _property, empty);
            });
            DrawArray(rs[1], EditorPackageDataDawer.builds, builds, (p) =>
            {
                return draw.GetPropertyHeight(p, null);
            }, (pos, _property) =>
            {
                EditorGUI.PropertyField(pos, _property, empty);
            });
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            //GUI.enabled = false;
            //var rs = AssetsWindow.RectEx.HorizontalSplit(position, position.height / 2, 10);

            DrawLeft(position, property);
            //DrawRight(rs[1], property);
        }
    }
}
