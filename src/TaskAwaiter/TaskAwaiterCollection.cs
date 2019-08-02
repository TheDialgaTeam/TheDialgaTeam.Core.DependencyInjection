using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace TheDialgaTeam.Core.DependencyInjection.TaskAwaiter
{
    internal class TaskAwaiterCollection : ITaskAwaiter, IDisposable
    {
        private CancellationTokenSource CancellationTokenSource { get; }

        private List<Task> TaskToAwait { get; } = new List<Task>();

        public TaskAwaiterCollection(CancellationTokenSource cancellationTokenSource)
        {
            CancellationTokenSource = cancellationTokenSource;
        }

        public Task EnqueueTask(Task taskToAwait)
        {
            TaskToAwait.Add(taskToAwait);
            return taskToAwait;
        }

        public Task EnqueueTask(Func<CancellationToken, Action> taskToAwait)
        {
            var task = Task.Run(taskToAwait(CancellationTokenSource.Token), CancellationTokenSource.Token);
            TaskToAwait.Add(task);
            return task;
        }

        public Task EnqueueTask(Func<CancellationToken, Task> taskToAwait)
        {
            var task = Task.Run(async () => await taskToAwait(CancellationTokenSource.Token).ConfigureAwait(false), CancellationTokenSource.Token);
            TaskToAwait.Add(task);
            return task;
        }

        public Task EnqueueTask<TState>(TState state, Func<CancellationToken, TState, Action> taskToAwait)
        {
            var task = Task.Run(taskToAwait(CancellationTokenSource.Token, state), CancellationTokenSource.Token);
            TaskToAwait.Add(task);
            return task;
        }

        public Task EnqueueTask<TState>(TState state, Func<CancellationToken, TState, Task> taskToAwait)
        {
            var task = Task.Run(async () => await taskToAwait(CancellationTokenSource.Token, state).ConfigureAwait(false), CancellationTokenSource.Token);
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