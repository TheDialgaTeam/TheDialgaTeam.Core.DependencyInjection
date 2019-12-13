using System.Threading.Tasks;

namespace TheDialgaTeam.Core.DependencyInjection.Executor
{
    internal class WaitForCancellationServiceExecutor : IServiceExecutor
    {
        public void ExecuteService(ITaskAwaiter taskAwaiter)
        {
            taskAwaiter.EnqueueTask(async cancellationToken =>
            {
                try
                {
                    while (!cancellationToken.IsCancellationRequested)
                        await Task.Delay(1, cancellationToken).ConfigureAwait(false);
                }
                catch (TaskCanceledException)
                {
                }
            });
        }
    }
}