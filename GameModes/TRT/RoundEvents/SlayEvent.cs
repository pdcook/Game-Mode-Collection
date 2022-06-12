using System;
using System.Runtime;
using System.Reflection;
namespace GameModeCollection.GameModes.TRT.RoundEvents
{
    public class SlayEvent : RoundEvent
    {
        public const string ID = "SlayEvent";
        public override int Priority { get; } = 0;
        public override void LogEvent(int playerID, params object[] args) { }
        public override string EventMessage() { return "Was slain as a punishment for excessive teamkilling."; }
    }
}
