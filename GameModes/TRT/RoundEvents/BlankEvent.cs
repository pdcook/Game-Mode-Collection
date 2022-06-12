using System;
using System.Runtime;
using System.Reflection;
namespace GameModeCollection.GameModes.TRT.RoundEvents
{
    public class BlankEvent : RoundEvent
    {
        public const string ID = "BlankEvent";
        public override int Priority { get; } = int.MinValue;
        public override void LogEvent(int playerID, params object[] args) { }
        public override string EventMessage() { return ""; }
    }
}
