using System;
using System.Runtime;
using System.Reflection;
namespace GameModeCollection.GameModes.TRT.RoundEvents
{
    public class ReviveEvent : RoundEvent
    {
        public const string ID = "ReviveEvent";

        private int revives = 0;
        public override int Priority
        {
            get
            {
                if (revives == 0)
                    return int.MinValue;
                else
                    return revives*10;
            }
        }
        public override void LogEvent(int playerID, params object[] args)
        {
            this.revives++;
        }
        public override string EventMessage()
        {
            if (revives == 0)
                return "";
            else if (revives == 1)
                return "Got a second chance at life.";
            else
            {
                string msg = "Got a 2nd chance at life.";
                for (int i = 3; i < revives + 2; i++)
                    msg += $" And a {AddOrdinal(i)}.";
                return msg;
            }

        }
        public static string AddOrdinal(int num)
        {
            if (num <= 0) return num.ToString();

            switch (num % 100)
            {
                case 11:
                case 12:
                case 13:
                    return num + "th";
            }

            switch (num % 10)
            {
                case 1:
                    return num + "st";
                case 2:
                    return num + "nd";
                case 3:
                    return num + "rd";
                default:
                    return num + "th";
            }
        }
    }
}
