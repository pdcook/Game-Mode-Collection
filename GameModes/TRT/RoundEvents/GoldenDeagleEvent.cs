using System.Collections.Generic;
using System.Linq;
namespace GameModeCollection.GameModes.TRT.RoundEvents
{
    public class GoldenDeagleEvent : RoundEvent
    {
        public enum Result
        {
            Success, // golden deagle was used to kill a Traitor / Killer
            Fail, // golden deagle was used to kill an innocent, so instead killed the user
            Chaos, // golden deagle was used to kill a Jester/Swapper, so killed them and the user
            Suicide, // golden deagle was used to kill themselves...
            None
        }

        public const string ID = "GoldenDeagleEvent";
        private Result _result = Result.None;
        private string _victimRole;
        private string a_or_an;
        public override int Priority
        {
            get
            {
                switch (this._result)
                {
                    case Result.Success:
                        return 10;
                    case Result.Fail:
                        return 20;
                    case Result.Chaos:
                        return 30;
                    case Result.Suicide:
                        return 50;
                    default:
                        return int.MinValue;
                }

            }
        }
        public override void LogEvent(int playerID, params object[] args)
        {
            // args[0] (GoldenDeagleEvent.Result) is the result of the golden deagle kill
            this._result = (Result)args[0];

            // args[1] (TRT_Role_Appearance) is the role of the player they hit
            this._victimRole = RoleManager.GetRoleColoredName((TRT_Role_Appearance)args[1]);
            this.a_or_an = (new List<char> { 'a', 'e', 'i', 'o', 'u' }).Contains(((TRT_Role_Appearance)args[1]).Name.ToLower().First()) ? "an" : "a";
        }
        public override string EventMessage()
        {
            switch (_result)
            {
                case Result.Success:
                    return $"Used the Golden Deagle to kill {a_or_an} {_victimRole}.";
                case Result.Fail:
                    return $"Died trying to kill {a_or_an} {_victimRole} with the Golden Deagle.";
                case Result.Chaos:
                    return $"Sacrificed themselves to kill {a_or_an} {_victimRole} with the Golden Deagle.";
                case Result.Suicide:
                    return "Managed to kill themselves with the Golden Deagle.";
                default:
                    return "";
            }
        }
    }
}
