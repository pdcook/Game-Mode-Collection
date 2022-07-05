using System.Collections;
using UnityEngine;
using GameModeCollection.GameModes;
using GameModeCollection.GameModeHandlers;
using GameModeCollection.GameModes.TRT;
using GameModeCollection.GameModes.TRT.VoiceChat;
using Sonigon;
using Sonigon.Internal;
using UnboundLib;
using UnboundLib.GameModes;
using GameModeCollection.Utils;
using GameModeCollection.Extensions;
using LocalZoom;

namespace GameModeCollection.GMCObjects
{
    public class LightSwitch : Interactable
    {

        public const float FOV = 90f;
        public const float ViewDistance = 40f;

        public static IEnumerator ForceStop(IGameModeHandler gm)
        {
            if (GameModeManager.CurrentHandlerID == TRTHandler.GameModeID)
            {
                if (LightsOff)
                {
                    LightsOff = false;
                    PlayerManager.instance.ForEachPlayer(p =>
                    {
                        ViewSphere vs = p.GetComponentInChildren<ViewSphere>();
                        if (vs is null) { return; }
                        vs.fov = GM_TRT.DefaultFOV;
                        vs.viewDistance = GM_TRT.DefaultVisibility;

                    });
                        
                }
            }

            yield break;
        }

        public override string HoverText { get; protected set; } = "Lights";
        public override Color TextColor { get; protected set; } = GM_TRT.DullWhite;
        public override Color IconColor { get; protected set; } = GM_TRT.DullWhite;
        public override bool InteractableInEditor { get; protected set; } = false;
        public override Alignment? RequiredAlignment { get; protected set; } = null;
        public override float VisibleDistance { get; protected set; } = 5f;
        public override bool RequireLoS { get; protected set; } = true;

        
        private static bool LightsOff = false;

        public override void OnInteract(Player player)
        {
            LightsOff = !LightsOff;
            // turn on/off the lights
            PlayerManager.instance.ForEachPlayer(p =>
            {
                ViewSphere vs = p.GetComponentInChildren<ViewSphere>();
                if (vs is null) { return; }
                vs.fov = LightsOff ? FOV : GM_TRT.DefaultFOV;
                vs.viewDistance = LightsOff ? ViewDistance : GM_TRT.DefaultVisibility;

            });
        }
    }
}
