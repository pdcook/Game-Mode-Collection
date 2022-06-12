using System;
using System.Runtime;
using System.Reflection;
namespace GameModeCollection.GameModes.TRT.RoundEvents
{
    public abstract class RoundEvent : IRoundEvent
    {
        public abstract int Priority { get; }
        public abstract void LogEvent(int playerID, params object[] args);
        public abstract string EventMessage();
    }
}
