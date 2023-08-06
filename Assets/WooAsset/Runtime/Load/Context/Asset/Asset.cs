using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;
namespace WooAsset
{
    public class Asset : AssetHandle
    {

        private Object[] assets;

        private AssetBundleRequest loadOp;
        private float directProgress;
        public Asset(AssetLoadArgs loadArgs) : base(loadArgs)
        {

        }
        public override float progress
        {
            get
            {
                if (isDone) return 1;
                if (direct)
                    return directProgress;
                if (async)
                {

                    if (loadOp == null)
                        return bundle.progress * 0.25f + dpProgress * 0.5f;
                    return 0.25f + 0.25f * loadOp.progress + dpProgress * 0.5f;
                }
                return bundle.progress * 0.5f + dpProgress * 0.5f;
            }
        }

        public virtual T GetAsset<T>() where T : Object => isDone && !unloaded ? value as T : null;
        public virtual Type GetAssetType() => isDone && !isErr && !unloaded ? value.GetType() : null;
        public virtual Object[] allAssets => isDone && !isErr && !unloaded ? assets : null;
        public IReadOnlyList<T> GetSubAssets<T>() where T : Object => !isDone || isErr || unloaded
                ? null
                : allAssets
                .Where(x => x is T)
                .Select(x => x as T)
                .ToArray();
        public T GetSubAsset<T>(string name) where T : Object => !isDone || isErr || unloaded
            ? null :
            allAssets
            .Where(x => x.name == name)
            .FirstOrDefault() as T;


        private async void LoadFile()
        {
            if (File.Exists(path))
            {
                FileObject obj = ScriptableObject.CreateInstance<FileObject>();
                obj.path = path;
                obj.fileInfo = new FileInfo(path);
                using (var stream = File.OpenRead(path))
                {
                    if (async)
                    {
                        long total = stream.Length;
                        long last = total;
                        int offset = 0;
                        int n = AssetsInternal.GetReadFileBlockSize();
                        obj.bytes = new byte[total];
                        while (last > 0)
                        {
                            var read = stream.Read(obj.bytes, offset, (int)Mathf.Min(n, last));
                            offset += read;
                            last -= read;
                            directProgress = offset / (float)total;
                            await new YieldOperation();
                        }
                    }
                    else
                    {
                        obj.bytes = File.ReadAllBytes(path);
                    }
                }

                SetResult(obj);
            }
            else
            {
                SetErr($"file not exist {path}");
            }
            InvokeComplete();
        }

        protected virtual async void LoadUnityObject()
        {
            await LoadBundle();
            if (bundle.unloaded)
            {
                this.SetErr($"bundle Has been Unloaded  {path}");
                InvokeComplete();
                return;
            }
            if (bundle.isErr)
            {
                this.SetErr(bundle.error);
                InvokeComplete();
                return;
            }
            if (async)
            {
                loadOp = bundle.LoadAssetAsync(path, typeof(UnityEngine.Object));
                await loadOp;
                assets = loadOp.allAssets;
                SetResult(loadOp.asset);
            }
            else
            {
                var result = bundle.LoadAsset(path, typeof(UnityEngine.Object));
                assets = result;
                SetResult(result[0]);
            }
        }
        protected sealed override void OnLoad()
        {
            if (!direct)
                LoadUnityObject();
            else
                LoadFile();
        }

    }

}
