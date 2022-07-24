using UnityEngine;
using InControl;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Reflection;
using System;
using UnityEngine.UI;
using TMPro;
namespace GMCUnityScripts
{
    public class ControlHelpHandler : MonoBehaviour
    {
        public const float OPACITY = 0.75f;
        public static readonly Color DEFAULTCOLOR = new Color32(200, 200, 200, 255);

        public static ControlHelpHandler Instance;
        public GameObject Controller { get; private set; }
        public GameObject Keyboard { get; private set; }
        public GameObject Mouse { get; private set; }
        public GameObject KeybindTexts { get; private set; }

        private static readonly List<Color> Colors = new List<Color>()
        {
            new Color(0.6392157f, 0.2862745f, 0.1686275f, 1f),
            new Color(0.1647059f, 0.3098039f, 0.5843138f, 1f),
            new Color(0.6313726f, 0.2705882f, 0.2705882f, 1f),
            new Color(0.2627451f, 0.5372549f, 0.3254902f, 1f),
            new Color(0.6235294f, 0.6392157f, 0.172549f, 1f),
            new Color(0.3607843f, 0.172549f, 0.6392157f, 1f),
            new Color(0.6392157f, 0.172549f, 0.3960784f, 1f),
            new Color(0.172549f, 0.6392157f, 0.6117647f, 1f),
        };

        private int colorIdx = -1;
        private Color NextColor
        {
            get
            {
                this.colorIdx = mod(this.colorIdx, Colors.Count);
                return Colors[colorIdx];
            }
        }
        public static int mod(int x, int m)
        {
            int r = x % m;
            return r < 0 ? r + m : r;
        }

        void Awake()
        {
            Instance = this;
        }
        void Start()
        {
            this.Controller = this.transform.GetChild(0).GetChild(0).gameObject;
            this.Keyboard = this.transform.GetChild(0).GetChild(1).gameObject;
            this.Mouse = this.transform.GetChild(0).GetChild(2).gameObject;
            this.KeybindTexts = this.transform.GetChild(0).GetChild(3).gameObject;
        }
        public void CreateControlHelp(InputDeviceClass device)
        {
            this.ResetAllColors();
            this.KeybindTexts.DestroyAllChildren();
            switch (device)
            {
                case InputDeviceClass.Keyboard:
                case InputDeviceClass.Mouse:
                    this.Controller.SetActive(false);
                    this.Keyboard.SetActive(true);
                    this.Mouse.SetActive(true);
                    this.CreateKeybindsForDevice(device);
                    break;
                case InputDeviceClass.Controller:
                    this.Controller.SetActive(true);
                    this.Keyboard.SetActive(false);
                    this.Mouse.SetActive(false);
                    this.CreateKeybindsForDevice(device);
                    break;
                case InputDeviceClass.Unknown:
                case InputDeviceClass.Remote:
                case InputDeviceClass.FlightStick:
                default:
                    this.gameObject.SetActive(false);
                    UnityEngine.Debug.LogError($"[ControllerHelpHandler] Control type {device} not supported.");
                    return;
            }
            this.gameObject.GetOrAddComponent<SetOpacityOfAllChildrenUI>().SetOpacity(OPACITY);
        }
        private void ResetAllColors()
        {
            foreach (Image im in this.gameObject.GetComponentsInChildren<Image>(true))
            {
                im.color = DEFAULTCOLOR;
            }

        }
        private void CreateKeybindsForDevice(InputDeviceClass device)
        {
            List<PlayerActionSet> playerActionSets = (List<PlayerActionSet>)typeof(InputManager).GetField("playerActionSets", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);

            foreach (PlayerActionSet playerActionSet in playerActionSets)
            {
                switch (device)
                {
                    case InputDeviceClass.Keyboard:
                    case InputDeviceClass.Mouse:
                        Dictionary<string, Key[]> keys = GetKeys(playerActionSet);
                        Dictionary<string, Mouse[]> mouseButtons = GetMouseButtons(playerActionSet);
                        this.DrawKeyboard(keys);
                        this.DrawMouse(mouseButtons);
                        break;
                    case InputDeviceClass.Controller:
                        Dictionary<string, InputControlType[]> inputControlTypes = GetControllerButtons(playerActionSet);
                        this.DrawController(inputControlTypes);
                        break;
                    case InputDeviceClass.Unknown:
                    case InputDeviceClass.Remote:
                    case InputDeviceClass.FlightStick:
                    default:
                        this.gameObject.SetActive(false);
                        UnityEngine.Debug.LogError($"[ControllerHelpHandler] Control type {device} not supported.");
                        return;
                }
            }
        }
        private void DrawKeyboard(Dictionary<string, Key[]> bindings)
        {
            ControlButton[] controls = this.Keyboard.GetComponentsInChildren<ControlButton>(true);
            foreach (KeyValuePair<string, Key[]> binding in bindings)
            {
                foreach (ControlButton controlButton in controls.Where(c => c.BoundKeyboard.Intersect(binding.Value).Any()))
                {
                    Color color = this.NextColor;
                    controlButton.SetColor(color);
                    //this.CreateKeybindText("["+string.Join(", ", controlButton.BoundKeyboard.Select(k => k.ToString()) + "] " + binding.Key), color);
                    this.CreateKeybindText("["+ controlButton.Name + "] " + binding.Key, color);
                }
            }
        }
        private void DrawMouse(Dictionary<string, Mouse[]> bindings)
        {
            ControlButton[] controls = this.Mouse.GetComponentsInChildren<ControlButton>(true);
            foreach (KeyValuePair<string, Mouse[]> binding in bindings)
            {
                foreach (ControlButton controlButton in controls.Where(c => c.BoundMouse.Intersect(binding.Value).Any()))
                {
                    Color color = this.NextColor;
                    controlButton.SetColor(color);
                    //this.CreateKeybindText("["+string.Join(", ", controlButton.BoundMouse.Select(k => k.ToString()) + "] " + binding.Key), color);
                    this.CreateKeybindText("["+ controlButton.Name + "] " + binding.Key, color);
                }
            }
        }
        private void DrawController(Dictionary<string, InputControlType[]> bindings)
        {
            ControlButton[] controls = this.Controller.GetComponentsInChildren<ControlButton>(true);
            foreach (KeyValuePair<string, InputControlType[]> binding in bindings)
            {
                foreach (ControlButton controlButton in controls.Where(c => c.BoundController.Intersect(binding.Value).Any()))
                {
                    Color color = this.NextColor;
                    controlButton.SetColor(color);
                    //this.CreateKeybindText("["+string.Join(", ", controlButton.BoundController.Select(k => k.ToString()) + "] " + binding.Key), color);
                    this.CreateKeybindText("["+ controlButton.Name + "] " + binding.Key, color);
                }
            }
        }
        private void CreateKeybindText(string text, Color color)
        {
            TextMeshProUGUI tmp = new GameObject("Text", typeof(TextMeshProUGUI)).GetComponent<TextMeshProUGUI>();
            tmp.transform.SetParent(this.KeybindTexts.transform);
            tmp.text = text;
            tmp.color = color;
            tmp.enableAutoSizing = true;
            tmp.fontSizeMin = 10;
            tmp.fontSizeMax = 36;
            tmp.alignment = TextAlignmentOptions.MidlineLeft;
        }
        private static Dictionary<string, Key[]> GetKeys(PlayerActionSet playerActionSet)
        {
            return playerActionSet.Actions.ToDictionary(a => a.Name, a => a)
                .ToDictionary(kv => kv.Key, kv => kv.Value.Bindings) // get all the bindings from all the actions
                .Where(kv => kv.Value.GetType() == typeof(KeyBindingSource)) // get only the KeyBindings
                .ToDictionary(kv => kv.Key, kv => kv.Value.Select(b => ((KeyBindingSource)b).Control)) // cast them and get their KeyCombos
                .ToDictionary(kv => kv.Key, kv => kv.Value.SelectMany(c => Enumerable.Range(0, c.IncludeCount).Select(i => c.GetInclude(i)))) // get all the Keys from the bindings
                .ToDictionary(kv => kv.Key, kv => kv.Value.ToArray()); // return them as an array
        }
        private static Dictionary<string , Mouse[]> GetMouseButtons(PlayerActionSet playerActionSet)
        {
            return playerActionSet.Actions.ToDictionary(a => a.Name, a => a)
                .ToDictionary(kv => kv.Key, kv => kv.Value.Bindings) // get all the bindings from all the actions
                .Where(kv => kv.Value.GetType() == typeof(MouseBindingSource)) // get only the MouseBindings
                .ToDictionary(kv => kv.Key, kv => kv.Value.Select(b => ((MouseBindingSource)b).Control)) // cast them and get their Mouse s
                .ToDictionary(kv => kv.Key, kv => kv.Value.ToArray()); // return them as an array
        }
        private static Dictionary<string, InputControlType[]> GetControllerButtons(PlayerActionSet playerActionSet)
        {
            return playerActionSet.Actions.ToDictionary(a => a.Name, a => a)
                .ToDictionary(kv => kv.Key, kv => kv.Value.Bindings) // get all the bindings from all the actions
                .Where(kv => kv.Value.GetType() == typeof(DeviceBindingSource)) // get only the DeviceBindings
                .ToDictionary(kv => kv.Key, kv => kv.Value.Select(b => ((DeviceBindingSource)b).Control)) // cast them and get their InputControlType s
                .ToDictionary(kv => kv.Key, kv => kv.Value.ToArray()); // return them as an array
        }
    }
}
