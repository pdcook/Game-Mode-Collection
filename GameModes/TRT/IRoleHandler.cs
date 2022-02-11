using UnityEngine;
namespace GameModeCollection.GameModes.TRT
{
    public interface IRoleHandler
    {
        Alignment RoleAlignment { get; }
        string WinMessage { get; }
        Color WinColor { get; }
        string RoleName { get; }
        string RoleID { get; }
        int MinNumberOfPlayersForRole { get; }
        int MinNumberOfPlayersWithRole { get; }
        int MaxNumberOfPlayersWithRole { get; }
        float Rarity { get; }
        string[] RoleIDsToOverwrite { get; }
        void AddRoleToPlayer(Player player);
    }
}
