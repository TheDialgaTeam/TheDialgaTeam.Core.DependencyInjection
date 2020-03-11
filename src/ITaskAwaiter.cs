using System;
using System.Threading;
using System.Threading.Tasks;

namespace TheDialgaTeam.Core.DependencyInjection
{
    public interface ITaskAwaiter
    {
        Task EnqueueTask(Action<CancellationToken> taskToAwait);

        Task EnqueueTask(Func<CancellationToken, Task> taskToAwait);

        Task EnqueueTask<TState>(TState state, Action<CancellationToken, TState> taskToAwait);

        Task EnqueueTask<TState>(TState state, Func<CancellationToken, TState, Task> taskToAwait);
    }
}