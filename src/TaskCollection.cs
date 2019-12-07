using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TheDialgaTeam.Core.Task;

namespace TheDialgaTeam.Core.DependencyInjection
{
    internal sealed class TaskCollection : ITaskAwaiter, IDisposable
    {
        private CancellationTokenSource CancellationTokenSource { get; }

        private List<System.Threading.Tasks.Task> TaskToAwait { get; } = new List<System.Threading.Tasks.Task>();

        public TaskCollection(CancellationTokenSource cancellationTokenSource)
        {
            CancellationTokenSource = cancellationTokenSource;
        }

        public System.Threading.Tasks.Task EnqueueTask(System.Threading.Tasks.Task taskToAwait)
        {
            TaskToAwait.Add(taskToAwait);
            return taskToAwait;
        }

        public System.Threading.Tasks.Task EnqueueTask(Action<CancellationToken> action)
        {
            var task = System.Threading.Tasks.Task.Factory.StartNew(innerState => { innerState.action(innerState.CancellationTokenSource.Token); }, (action, CancellationTokenSource));
            TaskToAwait.Add(task);
            return task;
        }

        public System.Threading.Tasks.Task EnqueueTask(Func<CancellationToken, System.Threading.Tasks.Task> function)
        {
            var task = System.Threading.Tasks.Task.Factory.StartNew(innerState => innerState.function(innerState.CancellationTokenSource.Token), (function, CancellationTokenSource)).Unwrap();
            TaskToAwait.Add(task);
            return task;
        }

        public System.Threading.Tasks.Task EnqueueTask<TState>(TState state, Action<CancellationToken, TState> action)
        {
            var task = System.Threading.Tasks.Task.Factory.StartNew(innerState => { innerState.action(innerState.CancellationTokenSource.Token, innerState.state); }, (state, action, CancellationTokenSource));
            TaskToAwait.Add(task);
            return task;
        }

        public System.Threading.Tasks.Task EnqueueTask<TState>(TState state, Func<CancellationToken, TState, System.Threading.Tasks.Task> function)
        {
            var task = System.Threading.Tasks.Task.Factory.StartNew(innerState => innerState.function(innerState.CancellationTokenSource.Token, innerState.state), (state, function, CancellationTokenSource)).Unwrap();
            TaskToAwait.Add(task);
            return task;
        }

        public void WaitAll()
        {
            System.Threading.Tasks.Task.WaitAll(TaskToAwait.ToArray());
        }

        public void Dispose()
        {
            foreach (var task in TaskToAwait)
                task.Dispose();
        }
    }
}