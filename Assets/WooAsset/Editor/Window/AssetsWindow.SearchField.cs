using UnityEngine;
using System;
using System.Reflection;
using UnityEditor;

namespace WooAsset
{
    partial class AssetsWindow
    {
        class SearchField
        {
            //private Guid uuid = Guid.NewGuid();
            public string value = "";
            public event Action<string> onEndEdit;
            public event Action<string> onValueChange;
            public event Action<int> onModeChange;
            public string[] modes;
            public int mode;
            private MethodInfo info;
            private int controlID;
            public SearchField(string value, string[] modes, int mode)
            {
                this.mode = mode;
                this.value = value;
                this.modes = modes;
                info = typeof(EditorGUI).GetMethod("ToolbarSearchField",
                    BindingFlags.NonPublic | BindingFlags.Static,
                    null,
                    new System.Type[]
                    { typeof(int),typeof(Rect), typeof(string[]) ,typeof(int).MakeByRefType(),typeof(string)},
                    null);
            }

            public void SetVelue(string tmp)
            {
                if (tmp != value)
                {
                    value = tmp;
                    if (onValueChange != null)
                        onValueChange(value);
                }
            }
            public void SetMode(int tmp)
            {
                if (tmp != mode)
                {
                    mode = tmp;
                    if (onModeChange != null)
                        onModeChange(mode);
                }
            }
            public void OnGUI(Rect position)
            {
                if (info != null)
                {
                    controlID = GUIUtility.GetControlID(("EditorSearchField" /*+ uuid.ToString()*/).GetHashCode(), FocusType.Keyboard, position);

                    object[] args = new object[] { controlID, position, modes, mode, value };
                    string tmp = (string)info.Invoke(null, args);
                    SetMode((int)args[3]);
                    SetVelue(tmp);
                    Event e = Event.current;
                    if ((e.keyCode == KeyCode.Return || e.keyCode == KeyCode.Escape || e.character == '\n'))
                    {
                        if (GUIUtility.keyboardControl == controlID)
                        {
                            GUIUtility.keyboardControl = -1;
                            if (e.type != EventType.Repaint && e.type != EventType.Layout)
                                Event.current.Use();
                            if (onEndEdit != null) onEndEdit(value);
                        }

                    }
                }

            }

        }

    }

}
