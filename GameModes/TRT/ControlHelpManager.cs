using UnityEngine;
using InControl;
using GMCUnityScripts;
namespace GameModeCollection.GameModes.TRT
{
    public static class ControlHelpManager
    {
        static GameObject _Help = null;
        static GameObject Help
        {
            get
            {
                if (_Help is null)
                {
                    GameObject prefab = GameModeCollection.TRT_Assets.LoadAsset<GameObject>("ControlHelp");
                    _Help = GameObject.Instantiate(prefab);
                    GameObject.DontDestroyOnLoad(_Help);
                    _Help.SetActive(false);
                }
                return _Help;
            }
        }
        static ControlHelpHandler Handler => Help.GetComponent<ControlHelpHandler>();

        public static void ShowHelp(InputDeviceClass device)
        {
            if (!Help.activeSelf)
            {
                Help.SetActive(true);
                Handler.CreateControlHelp(device);
            }
        }
        public static void HideHelp()
        {
            if (Help.activeSelf)
            {
                Help.SetActive(false);
            }
        }
    }
}
