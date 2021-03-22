namespace MassTransitSyncContext
{
    public interface BaseMessage { }

    public interface TestMessage : BaseMessage
    {
        string Timestamp { get; }
    }
}
