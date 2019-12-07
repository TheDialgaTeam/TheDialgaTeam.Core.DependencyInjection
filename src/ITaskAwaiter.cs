using System;
using System.Threading;

namespace TheDialgaTeam.Core.DependencyInjection
{
    public interface ITaskAwaiter
    {
        System.Threading.Tasks.Task EnqueueTask(System.Threading.Tasks.Task taskToAwait);

        System.Threading.Tasks.Task EnqueueTask(Action<CancellationToken> taskToAwait);

        System.Threading.Tasks.Task EnqueueTask(Func<CancellationToken, System.Threading.Tasks.Task> taskToAwait);

        System.Threading.Tasks.Task EnqueueTask<TState>(TState state, Action<CancellationToken, TState> taskToAwait);

        System.Threading.Tasks.Task EnqueueTask<TState>(TState state, Func<CancellationToken, TState, System.Threading.Tasks.Task> taskToAwait);
    }
}