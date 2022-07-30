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
        public static readonly Color DEFAULTCOLOR = new Color(0.3922f, 0.3922f, 0.3922f, OPACITY);

        public static ControlHelpHandler Instance;
        public GameObject Controller => this.transform.GetChild(0).GetChild(0).gameObject;
        public GameObject Keyboard => this.transform.GetChild(0).GetChild(1).gameObject;
        public GameObject Mouse => this.transform.GetChild(0).GetChild(2).gameObject;
        public GameObject KeybindTexts => this.transform.GetChild(0).GetChild(3).gameObject;

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
                this.colorIdx = mod(this.colorIdx + 1, Colors.Count);
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
        }
        public void CreateControlHelp(InputDeviceClass device)
        {
            this.ResetAllColors();
            if (this.KeybindTexts != null)
            {
                this.KeybindTexts.DestroyAllChildren();
            }
            switch (device)
            {
                case InputDeviceClass.Keyboard:
                case InputDeviceClass.Mouse:
                    this.Controller?.SetActive(false);
                    this.Keyboard?.SetActive(true);
                    this.Mouse?.SetActive(true);
                    this.CreateKeybindsForDevice(device);
                    break;
                case InputDeviceClass.Controller:
                    this.Controller?.SetActive(true);
                    this.Keyboard?.SetActive(false);
                    this.Mouse?.SetActive(false);
                    this.CreateKeybindsForDevice(device);
                    break;
                case InputDeviceClass.Unknown:
                case InputDeviceClass.Remote:
                case InputDeviceClass.FlightStick:
                default:
                    this.gameObject?.SetActive(false);
                    UnityEngine.Debug.LogError($"[ControllerHelpHandler] Control type {device} not supported.");
                    return;
            }
            this.gameObject.GetOrAddComponent<SetOpacityOfAllChildrenUI>().SetOpacity(OPACITY);
        }
        private void ResetAllColors()
        {
            this.colorIdx = -1;
            foreach (ControlButton cb in this.gameObject.GetComponentsInChildren<ControlButton>(true))
            {
                cb.SetColor(DEFAULTCOLOR);
            }
        }
        private void CreateKeybindsForDevice(InputDeviceClass device)
        {
            List<PlayerActionSet> playerActionSets = (List<PlayerActionSet>)typeof(InputManager).GetField("playerActionSets", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);


            switch (device)
            {
                case InputDeviceClass.Keyboard:
                case InputDeviceClass.Mouse:
                    Lookup<string, Key[]> keys = GetKeys(playerActionSets.ToArray());
                    Lookup<string, Mouse[]> mouseButtons = GetMouseButtons(playerActionSets.ToArray());
                    this.DrawKeyboard(keys);
                    this.DrawMouse(mouseButtons);
                    break;
                case InputDeviceClass.Controller:
                    Lookup<string, InputControlType[]> inputControlTypes = GetControllerButtons(playerActionSets.ToArray());
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
        private void DrawKeyboard(Lookup<string, Key[]> bindings)
        {
            ControlButton[] controls = this.Keyboard.GetComponentsInChildren<ControlButton>(true);
            Dictionary<Key, string[]> binds = GetBinds<Key>(bindings);
            Dictionary<ControlButton, List<string>> bindsByButton = new Dictionary<ControlButton, List<string>>() { };
            foreach (KeyValuePair<Key, string[]> binding in binds)
            {
                foreach (ControlButton controlButton in controls.Where(c => c.BoundKeyboard.Contains(binding.Key)))
                {
                    if (!bindsByButton.ContainsKey(controlButton))
                    {
                        bindsByButton.Add(controlButton, binding.Value.ToList());
                    }
                    else
                    {
                        bindsByButton[controlButton].AddRange(binding.Value);
                    }
                }
            }
            foreach (KeyValuePair<ControlButton, List<string>> binding in bindsByButton)
            {
                Color color = this.NextColor;
                binding.Key.SetColor(color);
                this.CreateKeybindText("[" + binding.Key.Name + "] " + string.Join(", ", binding.Value), color);
            }
        }
        private void DrawMouse(Lookup<string, Mouse[]> bindings)
        {
            ControlButton[] controls = this.Mouse.GetComponentsInChildren<ControlButton>(true);
            Dictionary<Mouse, string[]> binds = GetBinds<Mouse>(bindings);
            Dictionary<ControlButton, List<string>> bindsByButton = new Dictionary<ControlButton, List<string>>() { };
            foreach (KeyValuePair<Mouse, string[]> binding in binds)
            {
                foreach (ControlButton controlButton in controls.Where(c => c.BoundMouse.Contains(binding.Key)))
                {
                    if (!bindsByButton.ContainsKey(controlButton))
                    {
                        bindsByButton.Add(controlButton, binding.Value.ToList());
                    }
                    else
                    {
                        bindsByButton[controlButton].AddRange(binding.Value);
                    }
                }
            }
            foreach (KeyValuePair<ControlButton, List<string>> binding in bindsByButton)
            {
                Color color = this.NextColor;
                binding.Key.SetColor(color);
                this.CreateKeybindText("[" + binding.Key.Name + "] " + string.Join(", ", binding.Value), color);
            }
        }
        private void DrawController(Lookup<string, InputControlType[]> bindings)
        {
            ControlButton[] controls = this.Controller.GetComponentsInChildren<ControlButton>(true);
            Dictionary<InputControlType, string[]> binds = GetBinds<InputControlType>(bindings);
            Dictionary<ControlButton, List<string>> bindsByButton = new Dictionary<ControlButton, List<string>>() { };
            foreach (KeyValuePair<InputControlType, string[]> binding in binds)
            {
                foreach (ControlButton controlButton in controls.Where(c => c.BoundController.Contains(binding.Key)))
                {
                    if (!bindsByButton.ContainsKey(controlButton))
                    {
                        bindsByButton.Add(controlButton, binding.Value.ToList());
                    }
                    else
                    {
                        bindsByButton[controlButton].AddRange(binding.Value);
                    }
                }
            }
            foreach (KeyValuePair<ControlButton, List<string>> binding in bindsByButton)
            {
                Color color = this.NextColor;
                binding.Key.SetColor(color);
                this.CreateKeybindText("["+ binding.Key.Name + "] " + string.Join(", ", binding.Value), color);
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
            tmp.fontSizeMax = 72;
            tmp.alignment = TextAlignmentOptions.MidlineLeft;
            tmp.fontStyle = FontStyles.SmallCaps | FontStyles.Bold;
        }
        private static Dictionary<T, string[]> GetBinds<T>(Lookup<string, T[]> keys) where T: Enum
        {
            Dictionary<T, List<string>> binds = new Dictionary<T, List<string>>() { };

            foreach (IGrouping<string, T[]> keyBind in keys)
            {
                foreach (T boundKey in keyBind.SelectMany(c=>c))
                {
                    if (binds.ContainsKey(boundKey))
                    {
                        binds[boundKey].Add(keyBind.Key);
                    }
                    else
                    {
                        binds.Add(boundKey, new List<string>() { keyBind.Key });
                    }
                }
            }

            return binds.ToDictionary(kv => kv.Key, kv => kv.Value.ToArray());
        }
        private static Lookup<string, Key[]> GetKeys(PlayerActionSet[] playerActionSets)
        {
            return (Lookup<string, Key[]>)playerActionSets.SelectMany(p => p.Actions)
                .ToLookup(a => a.Name, a => a.Bindings.OfType<KeyBindingSource>()
                    .Select(b => ((KeyBindingSource)b).Control)
                    .SelectMany(c => Enumerable.Range(0, c.IncludeCount).Select(i => c.GetInclude(i))).ToArray());
        }
        private static Lookup<string, Mouse[]> GetMouseButtons(PlayerActionSet[] playerActionSets)
        {
            return (Lookup<string, Mouse[]>)(playerActionSets.SelectMany(p => p.Actions)
                .Where(a => a.Bindings.OfType<MouseBindingSource>().Any())
                .ToLookup(a => a.Name, a => a.Bindings.OfType<MouseBindingSource>()
                    .Select(b => ((MouseBindingSource)b).Control).ToArray()));
        }
        private static Lookup<string, InputControlType[]> GetControllerButtons(PlayerActionSet[] playerActionSets)
        {
            return (Lookup<string, InputControlType[]>)playerActionSets.SelectMany(p => p.Actions)
                .Where(a => a.Bindings.OfType<DeviceBindingSource>().Any())
                .ToLookup(a => a.Name, a => a.Bindings.OfType<DeviceBindingSource>()
                    .Select(b => ((DeviceBindingSource)b).Control).ToArray());
        }
    }
}
