using System.Linq;
using System.Collections.Generic;
namespace GameModeCollection.GameModes.TRT.RoundEvents
{
    public class SwapEvent : RoundEvent
    {
        public const string ID = "SwapEvent";
        public override int Priority { get; } = 30;
        private string swappedRole;
        private string a_or_an;
        public override void LogEvent(int playerID, params object[] args)
        {
            // playerID is the swapper
            // args[0] (TRT_Role_Appearance) is the original role of the killer, which is the new role of the swapper

            this.swappedRole = RoleManager.GetRoleColoredName((TRT_Role_Appearance)args[0]);
            this.a_or_an = (new List<char> { 'a', 'e', 'i', 'o', 'u' }).Contains(((TRT_Role_Appearance)args[0]).Name.ToLower().First()) ? "an" : "a";
        }
        public override string EventMessage()
        {
            return $"Tricked {this.a_or_an} {this.swappedRole} into swapping with them.";
        }
    }
}
