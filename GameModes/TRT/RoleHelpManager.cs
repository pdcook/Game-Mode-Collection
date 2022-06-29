using GameModeCollection;
using UnityEngine;
using TMPro;

namespace GameModeCollection.GameModes.TRT
{
    static class RoleHelpManager
    {
        /// the handler responsible for creating and hiding help text for TRT roles

        static GameObject _Help = null;
        
        static GameObject Help
        {
            get
            {
                if (_Help is null)
                {
                    GameObject prefab = GameModeCollection.TRT_Assets.LoadAsset<GameObject>("RoleHelp");
                    _Help = GameObject.Instantiate(prefab);
                    GameObject.DontDestroyOnLoad(_Help);
                    _Help.SetActive(false);
                }
                return _Help;
            }
        }
        static TextMeshProUGUI RoleName
        {
            get
            {
                return Help?.transform.Find("Canvas/Columns/Column 1/RoleName")?.GetComponent<TextMeshProUGUI>();
            }
        }
        static TextMeshProUGUI Alignment
        {
            get
            {
                return Help?.transform.Find("Canvas/Columns/Column 1/Alignment")?.GetComponent<TextMeshProUGUI>();
            }
        }
        static TextMeshProUGUI Description
        {
            get
            {
                return Help?.transform.Find("Canvas/Columns/Column 2/Description")?.GetComponent<TextMeshProUGUI>();
            }
        }

        public static void ShowHelp(IRoleHelp roleHelp)
        {
            if (Help.activeSelf || roleHelp is null) { return; }

            RoleName.text = roleHelp.RoleAppearance.ToString();
            Alignment.text = $"Team - {RoleManager.GetAlignmentColoredName(roleHelp.RoleAlignment)}";
            Description.text = roleHelp.Description;

            Help.SetActive(true);
        }
        public static void ShowHelp(IRoleHandler roleHandler)
        {
            if (roleHandler is null) { return; }
            ShowHelp(roleHandler.RoleHelp);
        }
        public static void ShowHelp(Player player)
        {
            if (player is null) { return; }
            string roleID = RoleManager.GetPlayerRoleID(player);
            if (roleID is null) { return; }
            ShowHelp(RoleManager.GetHandler(roleID));
        }
        public static void HideHelp()
        {
            if (!Help.activeSelf) { return; }
            Help.SetActive(false);
        }

    }
}
