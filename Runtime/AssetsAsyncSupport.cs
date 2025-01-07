using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using static WooAsset.AssetsAsyncSupport;


namespace WooAsset
{
    public static class AssetsAsyncSupport2
    {
        private struct AsyncOperationAwaiter<T> : IAwaiter<T> where T : AsyncOperation
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

        public static IAwaiter<T> GetAwaiter<T>(this T target) where T : AsyncOperation => new AsyncOperationAwaiter<T>(target);

    }
    public static class AssetsAsyncSupport
    {
        public interface IAwaiter<out TResult> : INotifyCompletion, ICriticalNotifyCompletion
        {
            bool IsCompleted { get; }
            TResult GetResult();
        }
        private struct AssetOperationAwaiter<T> : IAwaiter<T> where T : Operation
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

            private void Task_completed(Operation operation)
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
        public static IAwaiter<T> GetAwaiter<T>(this T target)where T: Operation  =>  new AssetOperationAwaiter<T>(target); 
        public static void WarpErr(this Operation self) { }

    }
}
