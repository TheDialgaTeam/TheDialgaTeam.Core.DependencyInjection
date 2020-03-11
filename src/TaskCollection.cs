using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TheDialgaTeam.Core.Tasks;

namespace TheDialgaTeam.Core.DependencyInjection
{
    internal class TaskCollection : ITaskAwaiter, IDisposable
    {
        private readonly CancellationToken _cancellationToken;

        private readonly List<Task> _taskToAwait = new List<Task>();

        private bool _isDisposed;

        public TaskCollection(CancellationTokenSource cancellationTokenSource)
        {
            _cancellationToken = cancellationTokenSource.Token;
        }

        public Task EnqueueTask(Action<CancellationToken> action)
        {
            var task = TaskState.Run<(Action<CancellationToken> action, CancellationToken cancellationToken)>((action, _cancellationToken), innerState => { innerState.action(innerState.cancellationToken); }, _cancellationToken);
            _taskToAwait.Add(task);
            return task;
        }

        public Task EnqueueTask(Func<CancellationToken, Task> function)
        {
            var task = TaskState.Run<(Func<CancellationToken, Task> function, CancellationToken cancellationToken), Task>((function, _cancellationToken), innerState => innerState.function(innerState.cancellationToken), _cancellationToken).Unwrap();
            _taskToAwait.Add(task);
            return task;
        }

        public Task EnqueueTask<TState>(TState state, Action<CancellationToken, TState> action)
        {
            var task = TaskState.Run<(TState state, Action<CancellationToken, TState> action, CancellationToken cancellationToken)>((state, action, _cancellationToken), innerState => { innerState.action(innerState.cancellationToken, innerState.state); }, _cancellationToken);
            _taskToAwait.Add(task);
            return task;
        }

        public Task EnqueueTask<TState>(TState state, Func<CancellationToken, TState, Task> function)
        {
            var task = TaskState.Run<(TState state, Func<CancellationToken, TState, Task> function, CancellationToken cancellationToken), Task>((state, function, _cancellationToken), innerState => innerState.function(innerState.cancellationToken, innerState.state), _cancellationToken).Unwrap();
            _taskToAwait.Add(task);
            return task;
        }

        public void WaitAll()
        {
            var taskToAwait = _taskToAwait.FindAll(a => !a.IsCompleted);

            while (taskToAwait.Count > 0)
            {
                Task.WaitAll(taskToAwait.ToArray());
                taskToAwait = _taskToAwait.FindAll(a => !a.IsCompleted);
            }
        }

        public void Dispose()
        {
            if (_isDisposed) return;
            _isDisposed = true;

            var taskToAwait = _taskToAwait;

            foreach (var task in taskToAwait)
            {
                task.Dispose();
            }
        }
    }
}