using System;
using HarmonyLib;
using GameModeCollection.Extensions;
using System.Reflection;
using InControl;

namespace GameModeCollection.Patches
{
    // postfix PlayerActions constructor to add controls
    [HarmonyPatch(typeof(PlayerActions))]
    [HarmonyPatch(MethodType.Constructor)]
    [HarmonyPatch(new Type[] { })]
    class PlayerActionsPatchPlayerActions
    {
        private static void Postfix(PlayerActions __instance)
        {
            __instance.GetAdditionalData().trt_inspect_body = (PlayerAction)typeof(PlayerActions).InvokeMember("CreatePlayerAction",
                                    BindingFlags.Instance | BindingFlags.InvokeMethod |
                                    BindingFlags.NonPublic, null, __instance, new object[] { "Inspect Body (TRT)" });
            __instance.GetAdditionalData().interact = (PlayerAction)typeof(PlayerActions).InvokeMember("CreatePlayerAction",
                                    BindingFlags.Instance | BindingFlags.InvokeMethod |
                                    BindingFlags.NonPublic, null, __instance, new object[] { "Interact" });
            __instance.GetAdditionalData().trt_traitor_chat_ptt = (PlayerAction)typeof(PlayerActions).InvokeMember("CreatePlayerAction",
                                    BindingFlags.Instance | BindingFlags.InvokeMethod |
                                    BindingFlags.NonPublic, null, __instance, new object[] { "Traitor Chat Push-To-Talk" });
            __instance.GetAdditionalData().trt_radio_imwith_mod_item1 = (PlayerAction)typeof(PlayerActions).InvokeMember("CreatePlayerAction",
                                    BindingFlags.Instance | BindingFlags.InvokeMethod |
                                    BindingFlags.NonPublic, null, __instance, new object[] { "Radio \"I'm with ___.\" (TRT) [Mod: Item 1]" });
            __instance.GetAdditionalData().trt_radio_suspect_mod_item3 = (PlayerAction)typeof(PlayerActions).InvokeMember("CreatePlayerAction",
                                    BindingFlags.Instance | BindingFlags.InvokeMethod |
                                    BindingFlags.NonPublic, null, __instance, new object[] { "Radio \"___ is suspicious.\" (TRT) [Mod: Item 3]" });
            __instance.GetAdditionalData().trt_radio_traitor_mod_item2 = (PlayerAction)typeof(PlayerActions).InvokeMember("CreatePlayerAction",
                                    BindingFlags.Instance | BindingFlags.InvokeMethod |
                                    BindingFlags.NonPublic, null, __instance, new object[] { "Radio \"___ is a traitor!\" (TRT) [Mod: Item 2]" });
            __instance.GetAdditionalData().trt_radio_innocent_mod_item4 = (PlayerAction)typeof(PlayerActions).InvokeMember("CreatePlayerAction",
                                    BindingFlags.Instance | BindingFlags.InvokeMethod |
                                    BindingFlags.NonPublic, null, __instance, new object[] { "Radio \"___ is innocent.\" (TRT) [Mod: Item 4]" });
            __instance.GetAdditionalData().trt_shop_mod_item5 = (PlayerAction)typeof(PlayerActions).InvokeMember("CreatePlayerAction",
                                    BindingFlags.Instance | BindingFlags.InvokeMethod |
                                    BindingFlags.NonPublic, null, __instance, new object[] { "Open Shop (TRT) [Mod: Item 5]" });
            __instance.GetAdditionalData().discard_last_card_mod_item0 = (PlayerAction)typeof(PlayerActions).InvokeMember("CreatePlayerAction",
                                    BindingFlags.Instance | BindingFlags.InvokeMethod |
                                    BindingFlags.NonPublic, null, __instance, new object[] { "Drop Last Card [Mod: Item 0]" });

            // modifier
            __instance.GetAdditionalData().modifier = (PlayerAction)typeof(PlayerActions).InvokeMember("CreatePlayerAction",
                                    BindingFlags.Instance | BindingFlags.InvokeMethod |
                                    BindingFlags.NonPublic, null, __instance, new object[] { "Modifier" });
           // items 
            __instance.GetAdditionalData().trt_item0 = (PlayerAction)typeof(PlayerActions).InvokeMember("CreatePlayerAction",
                                    BindingFlags.Instance | BindingFlags.InvokeMethod |
                                    BindingFlags.NonPublic, null, __instance, new object[] { "Item 0" });
            __instance.GetAdditionalData().trt_item1 = (PlayerAction)typeof(PlayerActions).InvokeMember("CreatePlayerAction",
                                    BindingFlags.Instance | BindingFlags.InvokeMethod |
                                    BindingFlags.NonPublic, null, __instance, new object[] { "Item 1" });
            __instance.GetAdditionalData().trt_item2 = (PlayerAction)typeof(PlayerActions).InvokeMember("CreatePlayerAction",
                                    BindingFlags.Instance | BindingFlags.InvokeMethod |
                                    BindingFlags.NonPublic, null, __instance, new object[] { "Item 2" });
            __instance.GetAdditionalData().trt_item3 = (PlayerAction)typeof(PlayerActions).InvokeMember("CreatePlayerAction",
                                    BindingFlags.Instance | BindingFlags.InvokeMethod |
                                    BindingFlags.NonPublic, null, __instance, new object[] { "Item 3" });
            __instance.GetAdditionalData().trt_item4 = (PlayerAction)typeof(PlayerActions).InvokeMember("CreatePlayerAction",
                                    BindingFlags.Instance | BindingFlags.InvokeMethod |
                                    BindingFlags.NonPublic, null, __instance, new object[] { "Item 4" });
            __instance.GetAdditionalData().trt_item5 = (PlayerAction)typeof(PlayerActions).InvokeMember("CreatePlayerAction",
                                    BindingFlags.Instance | BindingFlags.InvokeMethod |
                                    BindingFlags.NonPublic, null, __instance, new object[] { "Item 5" });
           
            __instance.GetAdditionalData().toggle_summary = (PlayerAction)typeof(PlayerActions).InvokeMember("CreatePlayerAction",
                        BindingFlags.Instance | BindingFlags.InvokeMethod |
                        BindingFlags.NonPublic, null, __instance, new object[] { "Show Summary" });
            
            __instance.GetAdditionalData().role_help = (PlayerAction)typeof(PlayerActions).InvokeMember("CreatePlayerAction",
                        BindingFlags.Instance | BindingFlags.InvokeMethod |
                        BindingFlags.NonPublic, null, __instance, new object[] { "Show Help" });

        }
    }
    // postfix PlayerActions to add controller controls
    [HarmonyPatch(typeof(PlayerActions), "CreateWithControllerBindings")]
    class PlayerActionsPatchCreateWithControllerBindings
    {
        private static void Postfix(ref PlayerActions __result)
        {

            __result.GetAdditionalData().trt_inspect_body.AddDefaultBinding(InputControlType.RightStickButton);

            __result.GetAdditionalData().interact.AddDefaultBinding(InputControlType.LeftStickButton);

            __result.GetAdditionalData().trt_radio_imwith_mod_item1.AddDefaultBinding(InputControlType.DPadLeft);

            __result.GetAdditionalData().trt_radio_suspect_mod_item3.AddDefaultBinding(InputControlType.DPadRight);

            __result.GetAdditionalData().trt_radio_traitor_mod_item2.AddDefaultBinding(InputControlType.DPadUp);

            __result.GetAdditionalData().trt_radio_innocent_mod_item4.AddDefaultBinding(InputControlType.DPadDown);

            __result.Fire.RemoveBinding(new DeviceBindingSource(InputControlType.Action3));

            __result.GetAdditionalData().trt_shop_mod_item5.AddDefaultBinding(InputControlType.Action3);

            __result.GetAdditionalData().discard_last_card_mod_item0.AddDefaultBinding(InputControlType.Action4);

            __result.Jump.RemoveBinding(new DeviceBindingSource(InputControlType.LeftBumper));

            __result.GetAdditionalData().modifier.AddDefaultBinding(InputControlType.LeftBumper);
          
            __result.GetAdditionalData().role_help.AddDefaultBinding(InputControlType.Select);
            __result.GetAdditionalData().role_help.AddDefaultBinding(InputControlType.Minus);
            __result.GetAdditionalData().role_help.AddDefaultBinding(InputControlType.Options);
            __result.GetAdditionalData().role_help.AddDefaultBinding(InputControlType.View);
        }
    }
    // postfix PlayerActions to add keyboard controls
    [HarmonyPatch(typeof(PlayerActions), "CreateWithKeyboardBindings")]
    class PlayerActionsPatchCreateWithKeyboardBindings
    {
        private static void Postfix(ref PlayerActions __result)
        {
            __result.GetAdditionalData().trt_inspect_body.AddDefaultBinding(Key.E);

            __result.GetAdditionalData().interact.AddDefaultBinding(Key.F);

            __result.GetAdditionalData().trt_traitor_chat_ptt.AddDefaultBinding(Key.V);

            __result.GetAdditionalData().trt_radio_imwith_mod_item1.AddDefaultBinding(Key.X);

            __result.GetAdditionalData().trt_radio_traitor_mod_item2.AddDefaultBinding(Key.R);
            
            __result.GetAdditionalData().trt_radio_suspect_mod_item3.AddDefaultBinding(Key.G);

            __result.GetAdditionalData().trt_radio_innocent_mod_item4.AddDefaultBinding(Key.Z);

            __result.GetAdditionalData().discard_last_card_mod_item0.AddDefaultBinding(Key.Q);

            __result.GetAdditionalData().trt_shop_mod_item5.AddDefaultBinding(Key.C);

            __result.GetAdditionalData().trt_item0.AddDefaultBinding(Key.Backquote);
            __result.GetAdditionalData().trt_item1.AddDefaultBinding(Key.Key1);
            __result.GetAdditionalData().trt_item2.AddDefaultBinding(Key.Key2);
            __result.GetAdditionalData().trt_item3.AddDefaultBinding(Key.Key3);
            __result.GetAdditionalData().trt_item4.AddDefaultBinding(Key.Key4);
            __result.GetAdditionalData().trt_item5.AddDefaultBinding(Key.Key5);

            __result.GetAdditionalData().toggle_summary.AddDefaultBinding(Key.Slash);

            __result.GetAdditionalData().role_help.AddDefaultBinding(Key.H);
        }
    }
}
