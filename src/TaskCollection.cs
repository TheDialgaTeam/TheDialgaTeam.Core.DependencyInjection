using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TheDialgaTeam.Core.Tasks;

namespace TheDialgaTeam.Core.DependencyInjection
{
    internal class TaskCollection : ITaskAwaiter, IDisposable
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
            var task = TaskState.Run<(Action<CancellationToken> action, CancellationTokenSource cancellationTokenSource)>((action, _cancellationTokenSource), innerState => { innerState.action(innerState.cancellationTokenSource.Token); }, _cancellationTokenSource.Token);
            _taskToAwait.Add(task);
            return task;
        }

        public Task EnqueueTask(Func<CancellationToken, Task> function)
        {
            var task = TaskState.Run<(Func<CancellationToken, Task> function, CancellationTokenSource cancellationTokenSource), Task>((function, _cancellationTokenSource), innerState => innerState.function(innerState.cancellationTokenSource.Token), _cancellationTokenSource.Token).Unwrap();
            _taskToAwait.Add(task);
            return task;
        }

        public Task EnqueueTask<TState>(TState state, Action<CancellationToken, TState> action)
        {
            var task = TaskState.Run<(TState state, Action<CancellationToken, TState> action, CancellationTokenSource cancellationTokenSource)>((state, action, _cancellationTokenSource), innerState => { innerState.action(innerState.cancellationTokenSource.Token, innerState.state); }, _cancellationTokenSource.Token);
            _taskToAwait.Add(task);
            return task;
        }

        public Task EnqueueTask<TState>(TState state, Func<CancellationToken, TState, Task> function)
        {
            var task = TaskState.Run<(TState state, Func<CancellationToken, TState, Task> function, CancellationTokenSource cancellationTokenSource), Task>((state, function, _cancellationTokenSource), innerState => innerState.function(innerState.cancellationTokenSource.Token, innerState.state), _cancellationTokenSource.Token).Unwrap();
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
            if (_isDisposed)
                return;

            _isDisposed = true;

            foreach (var task in _taskToAwait)
                task.Dispose();
        }
    }
}