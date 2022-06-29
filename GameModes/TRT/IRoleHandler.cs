using UnityEngine;
namespace GameModeCollection.GameModes.TRT
{
    public interface IRoleHandler
    {
        IRoleHelp RoleHelp { get; }
        Alignment RoleAlignment { get; }
        string WinMessage { get; }
        Color WinColor { get; }
        string RoleName { get; }
        string RoleID { get; }
        int MinNumberOfPlayersForRole { get; }
        /// <summary>
        /// percentage chance of the role to spawn in a round
        /// </summary>
        float Rarity { get; }
        string[] RoleIDsToOverwrite { get; }
        Alignment? AlignmentToReplace { get; }
        void AddRoleToPlayer(Player player);
    }
}
