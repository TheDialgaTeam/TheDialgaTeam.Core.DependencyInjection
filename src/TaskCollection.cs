using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TheDialgaTeam.Core.Tasks;

namespace TheDialgaTeam.Core.DependencyInjection
{
    internal sealed class TaskCollection : ITaskAwaiter, IDisposable
    {
        private CancellationTokenSource CancellationTokenSource { get; }

        private List<Task> TaskToAwait { get; } = new List<Task>();

        public TaskCollection(CancellationTokenSource cancellationTokenSource)
        {
            CancellationTokenSource = cancellationTokenSource;
        }

        public Task EnqueueTask(Task taskToAwait)
        {
            TaskToAwait.Add(taskToAwait);
            return taskToAwait;
        }

        public Task EnqueueTask(Action<CancellationToken> action)
        {
            var task = Task.Factory.StartNew<(Action<CancellationToken> action, CancellationTokenSource cancellationTokenSource)>(innerState => { innerState.action(innerState.cancellationTokenSource.Token); }, (action, CancellationTokenSource));
            TaskToAwait.Add(task);
            return task;
        }

        public Task EnqueueTask(Func<CancellationToken, Task> function)
        {
            var task = Task.Factory.StartNew<Task, (Func<CancellationToken, Task> function, CancellationTokenSource cancellationTokenSource)>(innerState => innerState.function(innerState.cancellationTokenSource.Token), (function, CancellationTokenSource)).Unwrap();
            TaskToAwait.Add(task);
            return task;
        }

        public Task EnqueueTask<TState>(TState state, Action<CancellationToken, TState> action)
        {
            var task = Task.Factory.StartNew<(TState state, Action<CancellationToken, TState> action, CancellationTokenSource cancellationTokenSource)>(innerState => { innerState.action(innerState.cancellationTokenSource.Token, innerState.state); }, (state, action, CancellationTokenSource));
            TaskToAwait.Add(task);
            return task;
        }

        public Task EnqueueTask<TState>(TState state, Func<CancellationToken, TState, Task> function)
        {
            var task = Task.Factory.StartNew<Task, (TState state, Func<CancellationToken, TState, Task> function, CancellationTokenSource cancellationTokenSource)>(innerState => innerState.function(innerState.cancellationTokenSource.Token, innerState.state), (state, function, CancellationTokenSource)).Unwrap();
            TaskToAwait.Add(task);
            return task;
        }

        public void WaitAll()
        {
            Task.WaitAll(TaskToAwait.ToArray());
        }

        public void Dispose()
        {
            foreach (var task in TaskToAwait)
                task.Dispose();
        }
    }
}