using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace WooAsset
{
    [UnityEditor.CustomPropertyDrawer(typeof(AssetReference), true)]
    public class AssetReferenceDrawer : UnityEditor.PropertyDrawer
    {

        System.Object GetSerializedPropertyValue(SerializedObject serializedObject, string propertyPath)
        {
            System.Object tempObject = serializedObject.targetObject;
            var splitPaths = propertyPath.Split('.');
            Array array = null;
            for (int i = 0; i < splitPaths.Length; i++)
            {
                var splitPath = splitPaths[i];
                if (splitPath == "Array")
                {
                    var arrayType = tempObject.GetType();
                    if (arrayType.IsArray)
                    {
                        array = (Array)tempObject;
                    }
                    else
                    {
                        if (arrayType.Name.StartsWith("List"))
                        {
                            var _itemsField = arrayType.GetField("_items", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                            tempObject = _itemsField.GetValue(tempObject);
                            array = (Array)tempObject;
                        }
                    }
                }
                else if (i > 0 && splitPaths[i - 1] == "Array" && splitPath.StartsWith("data["))
                {
                    var arrayIndex = int.Parse(splitPath.Replace("data[", "").Replace("]", ""));
                    tempObject = array.GetValue(arrayIndex);
                }
                else
                {
                    var propField = tempObject.GetType().GetField(splitPath, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    tempObject = propField.GetValue(tempObject);
                }
            }
            return tempObject;
        }
        public override void OnGUI(Rect position, UnityEditor.SerializedProperty property, GUIContent label)
        {
            var guid_property = property.FindPropertyRelative(nameof(AssetReference.guid));
            var propertyPath = property.propertyPath;
            var p_obj = GetSerializedPropertyValue(property.serializedObject, propertyPath);

            var guid = guid_property.stringValue;
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var type = p_obj.GetType().GetProperty(nameof(AssetReference.type)).GetValue(p_obj) as Type;
            var obj = AssetDatabase.LoadAssetAtPath(path, type);

            var tmp = EditorGUI.ObjectField(position, property.name, obj, type, false);
            if (tmp != obj)
            {
                var dest = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(tmp), AssetPathToGUIDOptions.OnlyExistingAssets);
                guid_property.stringValue = dest;
            }

        }
    }
}
