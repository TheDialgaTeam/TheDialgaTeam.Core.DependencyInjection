using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TheDialgaTeam.Core.Tasks;

namespace TheDialgaTeam.Core.DependencyInjection
{
    internal sealed class TaskCollection : ITaskAwaiter, IDisposable
    {
        private readonly CancellationTokenSource _cancellationTokenSource;

        private readonly List<Task> _taskToAwait = new List<Task>();

        private bool _isDisposed;

        public TaskCollection(CancellationTokenSource cancellationTokenSource)
        {
            _cancellationTokenSource = cancellationTokenSource;
        }

        public Task EnqueueTask(Task taskToAwait)
        {
            _taskToAwait.Add(taskToAwait);
            return taskToAwait;
        }

        public Task EnqueueTask(Action<CancellationToken> action)
        {
            var task = Task.Factory.StartNew<(Action<CancellationToken> action, CancellationTokenSource cancellationTokenSource)>(innerState => { innerState.action(innerState.cancellationTokenSource.Token); }, (action, _cancellationTokenSource));
            _taskToAwait.Add(task);
            return task;
        }

        public Task EnqueueTask(Func<CancellationToken, Task> function)
        {
            var task = Task.Factory.StartNew<Task, (Func<CancellationToken, Task> function, CancellationTokenSource cancellationTokenSource)>(innerState => innerState.function(innerState.cancellationTokenSource.Token), (function, _cancellationTokenSource)).Unwrap();
            _taskToAwait.Add(task);
            return task;
        }

        public Task EnqueueTask<TState>(TState state, Action<CancellationToken, TState> action)
        {
            var task = Task.Factory.StartNew<(TState state, Action<CancellationToken, TState> action, CancellationTokenSource cancellationTokenSource)>(innerState => { innerState.action(innerState.cancellationTokenSource.Token, innerState.state); }, (state, action, _cancellationTokenSource));
            _taskToAwait.Add(task);
            return task;
        }

        public Task EnqueueTask<TState>(TState state, Func<CancellationToken, TState, Task> function)
        {
            var task = Task.Factory.StartNew<Task, (TState state, Func<CancellationToken, TState, Task> function, CancellationTokenSource cancellationTokenSource)>(innerState => innerState.function(innerState.cancellationTokenSource.Token, innerState.state), (state, function, _cancellationTokenSource)).Unwrap();
            _taskToAwait.Add(task);
            return task;
        }

        public void WaitAll()
        {
            Task.WaitAll(_taskToAwait.ToArray());
        }

        public void Dispose()
        {
            if (_isDisposed)
                return;

            _isDisposed = true;

            foreach (var task in _taskToAwait)
                task.Dispose();
        }
    }
}