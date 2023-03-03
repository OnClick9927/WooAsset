using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using static WooAsset.Assets;
using static WooAsset.AssetsInternal;

namespace WooAsset
{
    public static class AssetsAsyncSupport
    {

        public interface IAwaiter<out TResult> : INotifyCompletion
        {
            bool IsCompleted { get; }
            TResult GetResult();
        }
        public struct AssetOperationAwaiter<T> : IAwaiter<T>, ICriticalNotifyCompletion where T : AssetOperation
        {
            private T task;
            private Queue<Action> calls;
            public AssetOperationAwaiter(T task)
            {
                if (task == null) throw new ArgumentNullException("task");
                this.task = task;
                calls = new Queue<Action>();
                this.task.completed += Task_completed;
            }

            private void Task_completed()
            {
                while (calls.Count != 0)
                {
                    calls.Dequeue()?.Invoke();
                }
            }

            public bool IsCompleted => task.isDone;

            public T GetResult()
            {
                if (!IsCompleted)
                    throw new Exception("The task is not finished yet");
                return task;
            }

            public void OnCompleted(Action continuation)
            {
                UnsafeOnCompleted(continuation);
            }

            public void UnsafeOnCompleted(Action continuation)
            {
                if (continuation == null)
                    throw new ArgumentNullException("continuation");

                calls.Enqueue(continuation);
            }
        }
        public struct AsyncOperationAwaiter<T> : IAwaiter<T>, ICriticalNotifyCompletion where T : UnityEngine.AsyncOperation
        {
            private T task;
            private Queue<Action> calls;
            public AsyncOperationAwaiter(T task)
            {
                if (task == null) throw new ArgumentNullException("task");
                this.task = task;
                calls = new Queue<Action>();
                this.task.completed += Task_completed;
            }

            private void Task_completed(AsyncOperation op)
            {
                while (calls.Count != 0)
                {
                    calls.Dequeue()?.Invoke();
                }
            }

            public bool IsCompleted => task.isDone;

            public T GetResult()
            {
                if (!IsCompleted)
                    throw new Exception("The task is not finished yet");
                return task;
            }

            public void OnCompleted(Action continuation)
            {
                UnsafeOnCompleted(continuation);
            }

            public void UnsafeOnCompleted(Action continuation)
            {
                if (continuation == null)
                    throw new ArgumentNullException("continuation");

                calls.Enqueue(continuation);
            }
        }


        public static IAwaiter<AssetBundleRequest> GetAwaiter(this AssetBundleRequest target)
        {
            return new AsyncOperationAwaiter<AssetBundleRequest>(target);
        }
        public static IAwaiter<AssetBundleCreateRequest> GetAwaiter(this AssetBundleCreateRequest target)
        {
            return new AsyncOperationAwaiter<AssetBundleCreateRequest>(target);
        }
        public static IAwaiter<ResourceRequest> GetAwaiter(this ResourceRequest target)
        {
            return new AsyncOperationAwaiter<ResourceRequest>(target);
        }
        public static IAwaiter<AsyncOperation> GetAwaiter(this AsyncOperation target)
        {
            return new AsyncOperationAwaiter<AsyncOperation>(target);
        }

        public static IAwaiter<CopyBundleOperation> GetAwaiter(this CopyBundleOperation target)
        {
            return new AssetOperationAwaiter<CopyBundleOperation>(target);
        }
        public static IAwaiter<CheckBundleVersionOperation> GetAwaiter(this CheckBundleVersionOperation target)
        {
            return new AssetOperationAwaiter<CheckBundleVersionOperation>(target);
        }
        public static IAwaiter<DownLoadBundleOperation> GetAwaiter(this DownLoadBundleOperation target)
        {
            return new AssetOperationAwaiter<DownLoadBundleOperation>(target);
        }
        public static IAwaiter<LoadManifestOperation> GetAwaiter(this LoadManifestOperation target)
        {
            return new AssetOperationAwaiter<LoadManifestOperation>(target);

        }
        public static IAwaiter<Asset> GetAwaiter(this Asset target)
        {
            return new AssetOperationAwaiter<Asset>(target);
        }
        public static IAwaiter<ResourcesAsset> GetAwaiter(this ResourcesAsset target)
        {
            return new AssetOperationAwaiter<ResourcesAsset>(target);
        }
        public static IAwaiter<SceneAsset> GetAwaiter(this SceneAsset target)
        {
            return new AssetOperationAwaiter<SceneAsset>(target);
        }
        public static IAwaiter<Bundle> GetAwaiter(this Bundle target)
        {
            return new AssetOperationAwaiter<Bundle>(target);

        }
        public static IAwaiter<AssetsGroupOperation> GetAwaiter(this AssetsGroupOperation target)
        {
            return new AssetOperationAwaiter<AssetsGroupOperation>(target);

        }
        public static IAwaiter<InstantiateObjectOperation> GetAwaiter(this InstantiateObjectOperation target)
        {
            return new AssetOperationAwaiter<InstantiateObjectOperation>(target);
        }
    }
}
