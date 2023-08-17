using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
namespace WooAsset
{
    class AssetsLoop : MonoBehaviour
    {
        private List<YieldOperation> yieldOperations = new List<YieldOperation>();

        private static AssetsLoop _instance;
        private static AssetsLoop instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject("Assets");
                    _instance = go.AddComponent<AssetsLoop>();
                    _instance._watch = Stopwatch.StartNew();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }
        private Stopwatch _watch;
        public static bool isBusy
        {
            get
            {
                if (!Application.isPlaying)
                    return false;
                return instance._watch.ElapsedMilliseconds - instance._frameTime >= AssetsInternal.GetLoadingMaxTimeSlice();
            }
        }
        private long _frameTime;

        private void Update()
        {
            _frameTime = _watch.ElapsedMilliseconds;

            for (int i = yieldOperations.Count - 1; i >= 0; i--)
            {
                var operation = yieldOperations[i];
                operation.NormalLoop();
                if (operation.isDone)
                {
                    yieldOperations.RemoveAt(i);
                }
            }
        }
        public static void AddOperation(YieldOperation op)
        {
            instance.yieldOperations.Add(op);
        }
    }

}
