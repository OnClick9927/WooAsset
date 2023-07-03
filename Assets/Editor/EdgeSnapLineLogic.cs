#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEditor.Experimental.SceneManagement;

using System.IO;

namespace ThunderFireUITool
{
    /// <summary>
    /// 处理边缘吸附
    /// </summary>
    [InitializeOnLoad]
    class EdgeSnapLineLogic
    {
        [MenuItem("Tools/Upm")]
        static void GG()
        {
            EditorUtility.OpenWithDefaultApp(Path.Combine(Application.dataPath, "../upm.bat"));
        }
        /// <summary>
        /// Snap相关的逻辑和全局变量
        /// </summary>
        private class SnapLogic
        {
            //屏幕空间吸附距离，后续可以添加到设置中之类
            public static float SnapSceneDistance = 8f;

            /// <summary>
            /// 当前帧物体最终吸附的位置
            /// </summary>
            public static Vector3 ObjFinalPos;
            /// <summary>
            /// 表示本次EditorApplication.update吸附到的辅助线距离
            /// Vert代表竖直，Horiz代表水平
            /// </summary>
            public static float SnapLineDisVert, SnapLineDisHoriz;
            /// <summary>
            /// 表示本次EditorApplication.update吸附到的边缘距离
            /// Vert代表竖直，Horiz代表水平
            /// </summary>
            public static float SnapEdgeDisVert, SnapEdgeDisHoriz;
            /// <summary>
            /// 表示本次EditorApplication.update吸附到的Interval距离
            /// Vert代表竖直，Horiz代表水平
            /// </summary>
            public static float SnapIntervalDisVert, SnapIntervalDisHoriz;
            //TODO 可以改成只在滚轮滚动之后更新几次
            public static float SnapWorldDistance
            {
                get
                {
                    SceneView sceneView = SceneView.lastActiveSceneView;
                    Vector3 v1 = sceneView.camera.ScreenToWorldPoint(new Vector3(SnapSceneDistance, 0, 0));
                    Vector3 v2 = sceneView.camera.ScreenToWorldPoint(new Vector3(0, 0, 0));
                    return Mathf.Abs(v1.x - v2.x);
                }
            }

        }
        private struct Rect
        {
            /// <summary>
            /// 长度为6，上中下左中右
            /// </summary>
            public float[] pos;

            public Rect(RectTransform trans)
            {
                pos = new float[6];
                pos[0] = (float)Math.Round((double)trans.TransformPoint(new Vector3(0, trans.rect.yMax, 0)).y, 1);
                pos[2] = (float)Math.Round((double)trans.TransformPoint(new Vector3(0, trans.rect.yMin, 0)).y, 1);
                pos[3] = (float)Math.Round((double)trans.TransformPoint(new Vector3(trans.rect.xMin, 0, 0)).x, 1);
                pos[5] = (float)Math.Round((double)trans.TransformPoint(new Vector3(trans.rect.xMax, 0, 0)).x, 1);
                pos[4] = (float)Math.Round((double)trans.position.x, 1);
                pos[1] = (float)Math.Round((double)trans.position.y, 1);
            }
        }
        static EdgeSnapLineLogic()
        {
            m_Rects = new List<Rect>();

            ResetAll();

            EditorApplication.hierarchyChanged += ResetAll;
            Selection.selectionChanged += ResetAll;
            EditorApplication.update += ListenMoving;
            EditorApplication.update += SnapToFinalPos;
        }

        private static GameObject m_SelectedObject;
        private static List<Rect> m_Rects;



        private static bool ObjectFit(GameObject obj)
        {
            if (obj == null) return false;
            Graphic[] components = obj.GetComponents<Graphic>();
            if (components == null || components.Length == 0) return false;
            return obj.activeInHierarchy && obj.GetComponent<RectTransform>() != null;
        }
        private static void ResetAll()
        {
            if (Selection.gameObjects.Length == 1 && ObjectFit(Selection.activeGameObject))
            {
                m_SelectedObject = Selection.activeGameObject;
                m_SelectedObject.transform.hasChanged = false;
            }
            else
            {
                m_SelectedObject = null;
                return;
            }

            m_Rects.Clear();
            RectTransform[] allObjects;
            var prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
            if (prefabStage != null)
            {
                allObjects = prefabStage.prefabContentsRoot.GetComponentsInChildren<RectTransform>();
            }
            else
            {
                allObjects = UnityEngine.Object.FindObjectsOfType<RectTransform>();
            }
            foreach (RectTransform item in allObjects)
            {
                if (ObjectFit(item.gameObject) && item.gameObject != m_SelectedObject)
                {
                    m_Rects.Add(new Rect(item));
                }
            }
        }

        /// <summary>
        /// 核心逻辑
        /// </summary>
        /// <param name="eps">eps=0表示吸附最终位置（需要画提示线）</param>
        private static void FindEdges(float eps)
        {

            Rect objRect = new Rect(m_SelectedObject.GetComponent<RectTransform>());

            foreach (Rect rect in m_Rects)
            {
                if (eps != 0)
                {
                    float dis = Mathf.Infinity;
                    for (int i = 0; i < 3; i++)
                    {
                        for (int j = 0; j < 3; j++)
                        {
                            if (Mathf.Abs(dis) > Mathf.Abs(rect.pos[i] - objRect.pos[j]))
                            {
                                dis = rect.pos[i] - objRect.pos[j];
                            }
                        }
                    }
                    if (Mathf.Abs(dis) < eps && Mathf.Abs(dis) < Mathf.Abs(SnapLogic.SnapEdgeDisVert))
                    {
                        SnapLogic.SnapEdgeDisVert = dis;
                    }
                    dis = Mathf.Infinity;
                    for (int i = 3; i < 6; i++)
                    {
                        for (int j = 3; j < 6; j++)
                        {
                            if (Mathf.Abs(dis) > Mathf.Abs(rect.pos[i] - objRect.pos[j]))
                            {
                                dis = rect.pos[i] - objRect.pos[j];
                            }
                        }
                    }
                    if (Mathf.Abs(dis) < eps && Mathf.Abs(dis) < Mathf.Abs(SnapLogic.SnapEdgeDisHoriz))
                    {
                        SnapLogic.SnapEdgeDisHoriz = dis;
                    }
                }
            }
        }

        private static void ListenMoving()
        {
            if (m_SelectedObject != null && m_SelectedObject.GetComponent<RectTransform>().position != SnapLogic.ObjFinalPos)
            {
                SnapLogic.SnapEdgeDisHoriz = SnapLogic.SnapEdgeDisVert = Mathf.Infinity;
                FindEdges(SnapLogic.SnapWorldDistance);
            }
        }

        private static void SnapToFinalPos()
        {
            if (m_SelectedObject == null) return;
            RectTransform rectTransform = m_SelectedObject.GetComponent<RectTransform>();
            if (rectTransform == null) return;
            if (rectTransform.position != SnapLogic.ObjFinalPos)
            {
                Vector3 vec = rectTransform.position;
                if (Math.Abs(SnapLogic.SnapEdgeDisHoriz) <= Math.Abs(SnapLogic.SnapIntervalDisHoriz) &&
                Math.Abs(SnapLogic.SnapEdgeDisHoriz) < Math.Abs(SnapLogic.SnapLineDisHoriz))
                {
                    vec.x += SnapLogic.SnapEdgeDisHoriz;
                }
                if (Math.Abs(SnapLogic.SnapEdgeDisVert) <= Math.Abs(SnapLogic.SnapIntervalDisVert) &&
                Math.Abs(SnapLogic.SnapEdgeDisVert) < Math.Abs(SnapLogic.SnapLineDisVert))
                {
                    vec.y += SnapLogic.SnapEdgeDisVert;
                }
                rectTransform.position = vec;
            }
        }
    }
}
#endif