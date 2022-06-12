using System.Linq;
using System.Collections.Generic;
namespace GameModeCollection.GameModes.TRT.RoundEvents
{
    public class KilledChaosEvent : RoundEvent
    {
        public const string ID = "KilledChaosEvent";
        public override int Priority { get; } = 40;
        private string killedRole;
        private string a_or_an;
        public override void LogEvent(int playerID, params object[] args)
        {
            // playerID is the killer
            // args[0] (TRT_Role_Appearance) is the role of the player they killed

            this.killedRole = RoleManager.GetRoleColoredName((TRT_Role_Appearance)args[0]);
            this.a_or_an = (new List<char> { 'a', 'e', 'i', 'o', 'u' }).Contains(((TRT_Role_Appearance)args[0]).Name.ToLower().First()) ? "an" : "a";
        }
        public override string EventMessage()
        {
            return $"Was tricked by {this.a_or_an} {this.killedRole}.";
        }
    }
}
