using System;
using System.Runtime;
using System.Reflection;
namespace GameModeCollection.GameModes.TRT.RoundEvents
{
    public class SuicideEvent : RoundEvent
    {
        public const string ID = "SuicideEvent";

        private int suicides = 0;
        public override int Priority
        {
            get
            {
                if (suicides == 0)
                    return int.MinValue;
                else
                    return (suicides-1)*10;
            }
        }
        public override void LogEvent(int playerID, params object[] args)
        {
            this.suicides++;
        }
        public override string EventMessage()
        {
            if (suicides == 0)
                return "";
            else if (suicides == 1)
                return "Killed themselves.";
            else
                return "Somehow killed themselves " + suicides + " times.";

        }
    }
}
