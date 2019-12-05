using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace TheDialgaTeam.Core.DependencyInjection
{
    internal class TaskCollection : ITaskAwaiter, IDisposable
    {
        private class EnqueueTaskState
        {
            public Action<CancellationToken> TaskToAwait { get; }

            public CancellationToken CancellationToken { get; }

            public EnqueueTaskState(Action<CancellationToken> taskToAwait, CancellationToken cancellationToken)
            {
                TaskToAwait = taskToAwait;
                CancellationToken = cancellationToken;
            }
        }

        private class EnqueueTaskState<TState>
        {
            public TState State { get; }

            public Action<CancellationToken, TState> TaskToAwait { get; }

            public CancellationToken CancellationToken { get; }

            public EnqueueTaskState(TState state, Action<CancellationToken, TState> taskToAwait, CancellationToken cancellationToken)
            {
                State = state;
                TaskToAwait = taskToAwait;
                CancellationToken = cancellationToken;
            }
        }

        private class EnqueueTaskAsyncState
        {
            public Func<CancellationToken, Task> TaskToAwait { get; }

            public CancellationToken CancellationToken { get; }

            public EnqueueTaskAsyncState(Func<CancellationToken, Task> taskToAwait, CancellationToken cancellationToken)
            {
                TaskToAwait = taskToAwait;
                CancellationToken = cancellationToken;
            }
        }

        private class EnqueueTaskAsyncState<TState>
        {
            public TState State { get; }

            public Func<CancellationToken, TState, Task> TaskToAwait { get; }

            public CancellationToken CancellationToken { get; }

            public EnqueueTaskAsyncState(TState state, Func<CancellationToken, TState, Task> taskToAwait, CancellationToken cancellationToken)
            {
                State = state;
                TaskToAwait = taskToAwait;
                CancellationToken = cancellationToken;
            }
        }

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

        public Task EnqueueTask(Action<CancellationToken> taskToAwait)
        {
            var task = Task.Factory.StartNew(stateObj =>
            {
                if (stateObj is EnqueueTaskState state)
                    state.TaskToAwait(state.CancellationToken);
            }, new EnqueueTaskState(taskToAwait, CancellationTokenSource.Token), CancellationTokenSource.Token);

            TaskToAwait.Add(task);

            return task;
        }

        public Task EnqueueTask(Func<CancellationToken, Task> taskToAwait)
        {
            var task = Task.Factory.StartNew(async stateObj =>
            {
                if (stateObj is EnqueueTaskAsyncState state)
                    await state.TaskToAwait(state.CancellationToken).ConfigureAwait(false);
            }, new EnqueueTaskAsyncState(taskToAwait, CancellationTokenSource.Token), CancellationTokenSource.Token).Unwrap();

            TaskToAwait.Add(task);

            return task;
        }

        public Task EnqueueTask<TState>(TState state, Action<CancellationToken, TState> taskToAwait)
        {
            var task = Task.Factory.StartNew(stateObj =>
            {
                if (stateObj is EnqueueTaskState<TState> state2)
                    state2.TaskToAwait(state2.CancellationToken, state2.State);
            }, new EnqueueTaskState<TState>(state, taskToAwait, CancellationTokenSource.Token), CancellationTokenSource.Token);

            TaskToAwait.Add(task);

            return task;
        }

        public Task EnqueueTask<TState>(TState state, Func<CancellationToken, TState, Task> taskToAwait)
        {
            var task = Task.Factory.StartNew(async stateObj =>
            {
                if (stateObj is EnqueueTaskAsyncState<TState> state2)
                    await state2.TaskToAwait(state2.CancellationToken, state2.State).ConfigureAwait(false);
            }, new EnqueueTaskAsyncState<TState>(state, taskToAwait, CancellationTokenSource.Token), CancellationTokenSource.Token).Unwrap();

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