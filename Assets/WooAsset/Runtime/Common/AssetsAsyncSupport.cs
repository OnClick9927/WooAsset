using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;


namespace WooAsset
{
    public static class AssetsAsyncSupport
    {
        public interface IAwaiter<out TResult> : INotifyCompletion, ICriticalNotifyCompletion
        {
            bool IsCompleted { get; }
            TResult GetResult();
        }
        public struct AssetOperationAwaiter<T> : IAwaiter<T> where T : Operation
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
        public struct AsyncOperationAwaiter<T> : IAwaiter<T> where T : AsyncOperation
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


        public static IAwaiter<AsyncOperation> GetAwaiter(this AsyncOperation target) => new AsyncOperationAwaiter<AsyncOperation>(target);

        public static IAwaiter<Operation> GetAwaiter(this Operation target) => new AssetOperationAwaiter<Operation>(target);

        public static IAwaiter<CheckBundleVersionOperation> GetAwaiter(this CheckBundleVersionOperation target) => new AssetOperationAwaiter<CheckBundleVersionOperation>(target);
        public static IAwaiter<VersionCompareOperation> GetAwaiter(this VersionCompareOperation target) => new AssetOperationAwaiter<VersionCompareOperation>(target);

        public static IAwaiter<RawAsset> GetAwaiter(this RawAsset target) => new AssetOperationAwaiter<RawAsset>(target);
        public static IAwaiter<Asset> GetAwaiter(this Asset target) => new AssetOperationAwaiter<Asset>(target);
        public static IAwaiter<SceneAsset> GetAwaiter(this SceneAsset target) => new AssetOperationAwaiter<SceneAsset>(target);
        public static IAwaiter<AssetsGroupOperation> GetAwaiter(this AssetsGroupOperation target) => new AssetOperationAwaiter<AssetsGroupOperation>(target);
        public static IAwaiter<InstantiateObjectOperation> GetAwaiter(this InstantiateObjectOperation target) => new AssetOperationAwaiter<InstantiateObjectOperation>(target);


        public static void WarpErr(this Operation self) { }

    }
}
