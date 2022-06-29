namespace GameModeCollection.GameModes.TRT
{
    public interface IRoleHelp
    {
        TRT_Role_Appearance RoleAppearance { get; }
        Alignment RoleAlignment { get; }
        TRT_Role_Appearance[] AlliedRoles { get; }
        TRT_Role_Appearance[] OpposingRoles { get; }
        string WinCondition { get; }
        string Description { get; }
    }
}
