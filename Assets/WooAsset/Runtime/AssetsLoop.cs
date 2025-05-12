using System;
using System.Diagnostics;
using UnityEngine;
namespace WooAsset
{
    class AssetsLoop : MonoBehaviour
    {
        private static AssetsLoop _instance;
        public static AssetsLoop instance
        {
            get
            {
                if (!Application.isPlaying)
                {
                    return null;
                }
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
        public bool isBusy
        {
            get
            {
                if (!Application.isPlaying)
                    return false;
                return _busy;
            }
        }
        private bool _busy;

        public Action update;
        private long _frameTime;

        private void Update()
        {
            _frameTime = _watch.ElapsedMilliseconds;
            DownLoaderSystem.Update();
            update?.Invoke();
            _busy = _watch.ElapsedMilliseconds - _frameTime >= AssetsInternal.GetLoadingMaxTimeSlice();
        }
    }

}
