using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using static WooAsset.Assets;
using static WooAsset.AssetsHelper;

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


        public static IAwaiter<AssetBundleRequest> GetAwaiter(this AssetBundleRequest target) => new AsyncOperationAwaiter<AssetBundleRequest>(target);
        public static IAwaiter<AssetBundleCreateRequest> GetAwaiter(this AssetBundleCreateRequest target) => new AsyncOperationAwaiter<AssetBundleCreateRequest>(target);
        public static IAwaiter<ResourceRequest> GetAwaiter(this ResourceRequest target) => new AsyncOperationAwaiter<ResourceRequest>(target);
        public static IAwaiter<AsyncOperation> GetAwaiter(this AsyncOperation target) => new AsyncOperationAwaiter<AsyncOperation>(target);

        public static IAwaiter<AssetOperation> GetAwaiter(this AssetOperation target) => new AssetOperationAwaiter<AssetOperation>(target);

        public static IAwaiter<CopyBundleOperation> GetAwaiter(this CopyBundleOperation target) => new AssetOperationAwaiter<CopyBundleOperation>(target);
        public static IAwaiter<CheckBundleVersionOperation> GetAwaiter(this CheckBundleVersionOperation target) => new AssetOperationAwaiter<CheckBundleVersionOperation>(target);
        public static IAwaiter<DownLoadBundleOperation> GetAwaiter(this DownLoadBundleOperation target) => new AssetOperationAwaiter<DownLoadBundleOperation>(target);
        public static IAwaiter<VersionCompareOperation> GetAwaiter(this VersionCompareOperation target) => new AssetOperationAwaiter<VersionCompareOperation>(target);

        public static IAwaiter<Asset> GetAwaiter(this Asset target) => new AssetOperationAwaiter<Asset>(target);
        public static IAwaiter<ResourcesAsset> GetAwaiter(this ResourcesAsset target) => new AssetOperationAwaiter<ResourcesAsset>(target);
        public static IAwaiter<SceneAsset> GetAwaiter(this SceneAsset target) => new AssetOperationAwaiter<SceneAsset>(target);
        public static IAwaiter<Bundle> GetAwaiter(this Bundle target) => new AssetOperationAwaiter<Bundle>(target);
        public static IAwaiter<AssetHandle> GetAwaiter(this AssetHandle target) => new AssetOperationAwaiter<AssetHandle>(target);
        public static IAwaiter<AssetsGroupOperation> GetAwaiter(this AssetsGroupOperation target) => new AssetOperationAwaiter<AssetsGroupOperation>(target);
        public static IAwaiter<InstantiateObjectOperation> GetAwaiter(this InstantiateObjectOperation target) => new AssetOperationAwaiter<InstantiateObjectOperation>(target);

        public static IAwaiter<ReadFileOperation> GetAwaiter(this ReadFileOperation target) => new AssetOperationAwaiter<ReadFileOperation>(target);

        public static void WarpErr(this AssetOperation self) { }

    }
}
