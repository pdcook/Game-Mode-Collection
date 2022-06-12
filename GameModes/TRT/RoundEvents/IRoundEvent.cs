namespace GameModeCollection.GameModes.TRT.RoundEvents
{
    public interface IRoundEvent
    {
        int Priority { get; }
        void LogEvent(int playerID, params object[] args);
        string EventMessage();
    }
}
