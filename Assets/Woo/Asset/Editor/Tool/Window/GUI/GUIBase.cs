using System;
using UnityEngine;

namespace WooAsset
{
    public abstract class GUIBase:IDisposable
    {
        public Rect position { get; private set; }

        public virtual void OnGUI(Rect position) 
        {
            this.position = position;
        }
        protected abstract void OnDispose();
        public void Dispose()
        {
            OnDispose();
        }
    }

}
