using System.Collections.Generic;
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
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }
        private void Update()
        {
            for (int i = yieldOperations.Count - 1; i >= 0; i--)
            {
                var operation = yieldOperations[i];
                operation.Update();
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
