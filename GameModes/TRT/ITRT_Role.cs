using UnityEngine;
namespace GameModeCollection.GameModes.TRT
{
    public enum Alignment
    {
        Innocent,
        Traitor,
        Chaos,
        Killer
    }
    public class TRT_Role_Appearance
    {
        public Alignment Alignment;
        public string Name;
        public char Abbr;
        /// <summary>
        /// The color used in the player's name, display, chat, and handbook
        /// </summary>
        public Color Color; 

        public TRT_Role_Appearance(Alignment Alignment, string Name, char Abbr, Color Color)
        {
            this.Alignment = Alignment;
            this.Name = Name;
            this.Abbr = Abbr;
            this.Color = Color;
        }

        public override string ToString()
        {
            return this.RoleColoredName();
        }
        public string RoleColoredName()
        {
            return this is null ? "" : $"<b><color={this.RoleColorHTML()}>{this.Name}</color></b>";
        }

        public string RoleColorHTML()
        {
            return this is null ? "white" : "#" + ColorUtility.ToHtmlStringRGB(this.Color);
        }
    }
    public interface ITRT_Role
    {
        TRT_Role_Appearance Appearance { get; }
        Alignment Alignment { get; }
        int MaxCards { get; }
        float BaseHealth { get; }
        bool CanDealDamageAndTakeEnvironmentalDamage { get; }
        float KarmaChange { get; }
        int StartingCredits { get; }

        /// <summary>
        /// What role should this role appear as to a given alignment?
        /// 
        /// For example:
        ///     To Traitors, other Traitors should appear as Traitors, so Traitor::AppearTo(Traitor) = Traitor
        ///     To Traitors, Swappers should appear as Jesters, so Swapper::AppearTo(Traitor) = Jester
        ///     To Jesters, Traitors should NOT appear at all, so Traitor::AppearTo(Jester) = null
        /// 
        /// </summary>
        /// <param name="alignment"></param>
        /// <returns>
        /// null - if the role should be hidden from the given alignment
        /// TRT_Role_Appearance - the role that this role should appear as
        /// </returns>
        TRT_Role_Appearance AppearToAlignment(Alignment alignment);

        /// <summary>
        /// Should the given alignment be notified that this role exists?
        /// 
        /// For example:
        ///     - All players are notified of the Detective, so Detective::Alert([anything]) = true
        ///     - Traitors are notified of other Traitors (and derivatives) so Traitor::Alert(Traitor) = true
        ///     - Traitors are notified of Jesters so Jester::Alert(Traitor) = true
        ///     - Innocents are not notified to anyone, so Innocent::Alert([anything]) = false
        /// </summary>
        /// <param name="alignment"></param>
        /// <returns></returns>
        bool AlertAlignment(Alignment alignment);

        void OnAnyPlayerDied(Player deadPlayer, ITRT_Role[] rolesRemaining);

        void OnKilledPlayer(Player killedPlayer);
        void OnKilledByPlayer(Player killingPlayer);

        /// <summary>
        /// Action ran when a player interacts with or inspects a body
        /// </summary>
        /// <param name="corpse">The corpse the player is interacting/inspecting</param>
        /// <param name="interact">Is the player trying to perform a role-specific action? For example: a vampire eating a body instead of inspecting it.</param>
        void OnInteractWithCorpse(TRT_Corpse corpse, bool interact);

        void OnCorpseInteractedWith(Player player);

        bool WinConditionMet(Player[] playersRemaining);

        /// <summary>
        /// what should happen when this player presses the shop button
        /// this method should handle both the opening and the closing of the shop
        /// </summary>
        void TryShop();

    }
}
