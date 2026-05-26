namespace Slottet.Realtime;

public sealed class OverviewUpdateNotifier
{
    public event Func<OverviewChangedMessage, Task>? OverviewChanged;

    public async Task NotifyOverviewChangedAsync(OverviewChangedMessage message)
    {
        var handlers = OverviewChanged?.GetInvocationList()
            .Cast<Func<OverviewChangedMessage, Task>>()
            .Select(handler => InvokeHandlerAsync(handler, message)) ?? [];

        await Task.WhenAll(handlers);
    }

    private static async Task InvokeHandlerAsync(
        Func<OverviewChangedMessage, Task> handler,
        OverviewChangedMessage message)
    {
        try
        {
            await handler(message);
        }
        catch
        {
            // Ignore stale dashboard subscriptions so one disconnected circuit does not block updates for others.
        }
    }
}
