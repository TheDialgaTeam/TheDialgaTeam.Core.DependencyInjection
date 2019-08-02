using System;
using System.Threading;
using System.Threading.Tasks;

namespace TheDialgaTeam.Core.DependencyInjection.TaskAwaiter
{
    public interface ITaskAwaiter
    {
        Task EnqueueTask(Task taskToAwait);

        Task EnqueueTask(Func<CancellationToken, Action> taskToAwait);

        Task EnqueueTask(Func<CancellationToken, Task> taskToAwait);

        Task EnqueueTask<TState>(TState state, Func<CancellationToken, TState, Action> taskToAwait);

        Task EnqueueTask<TState>(TState state, Func<CancellationToken, TState, Task> taskToAwait);
    }
}