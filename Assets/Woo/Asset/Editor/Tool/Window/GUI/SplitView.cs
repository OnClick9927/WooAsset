using System;
using UnityEditor;
using UnityEngine;

namespace WooAsset
{
    [Serializable]
    public class SplitView:GUIBase
    {
        public SplitType splitType = SplitType.Vertical;
        public float split = 200;
        public float minSize = 100;
        public event Action<Rect> fistPan, secondPan;
        public event Action onBeginResize;
        public event Action onEndResize;
        public bool dragging
        {
            get { return _resizing; }
            private set
            {
                if (_resizing != value)
                {
                    _resizing = value;
                    if (value)
                    {
                        if (onBeginResize != null)
                        {
                            onBeginResize();
                        }
                    }
                    else
                    {
                        if (onEndResize != null)
                        {
                            onEndResize();
                        }
                    }
                }
            }
        }
        private bool _resizing;
        public Rect [] GetSplitRect()
        {
            var rs = position.Split(splitType, split, 4);
            return rs;
        }
        public override void OnGUI(Rect position)
        {
            base.OnGUI(position);
            var rs = position.Split(splitType, split, 4);
            var mid = position.SplitRect(splitType, split, 4);
            if (fistPan != null)
            {
                fistPan(rs[0]);
            }
            if (secondPan != null)
            {
                secondPan(rs[1]);
            }
            EditorGUI.DrawRect(mid.Zoom(AnchorType.MiddleCenter,-2), Color.gray);
            Event e = Event.current;
            if (mid.Contains(e.mousePosition))
            {
                if (splitType == SplitType.Vertical)
                    EditorGUIUtility.AddCursorRect(mid, MouseCursor.ResizeHorizontal);
                else
                    EditorGUIUtility.AddCursorRect(mid, MouseCursor.ResizeVertical);
            }
            switch (Event.current.type)
            {
                case EventType.MouseDown:
                    if (mid.Contains(Event.current.mousePosition))
                    {
                        dragging = true;
                    }
                    break;
                case EventType.MouseDrag:
                    if (dragging)
                    {
                        switch (splitType)
                        {
                            case SplitType.Vertical:
                                split += Event.current.delta.x;
                                break;
                            case SplitType.Horizontal:
                                split += Event.current.delta.y;
                                break;
                        }
                        split = Mathf.Clamp(split, minSize, splitType== SplitType.Vertical? position.width - minSize: position.height - minSize);

                        e.Use();
                        //if (EditorWindow.focusedWindow != null)
                        //{
                        //    EditorWindow.focusedWindow.Repaint();
                        //}
                    }
                    break;
                case EventType.MouseUp:
                    if (dragging)
                    {
                        dragging = false;
                    }
                    break;
            }
        }
        protected override void OnDispose()
        {
            fistPan = null;
            secondPan = null;
            onBeginResize = null;
            onEndResize = null;
        }
    }
}
