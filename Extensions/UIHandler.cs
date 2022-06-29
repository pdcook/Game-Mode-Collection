using UnityEngine;
using TMPro;
using System.Collections.Generic;
using RWF;
using SoundImplementation;
using Sonigon;

namespace GameModeCollection.Extensions
{
	public static class UIHandlerExtensions
	{
		public static void DisplayRoundStartText(this UIHandler instance, string text, Color color, Vector3 screenPosition)
		{
			var uiGo = GameObject.Find("/Game/UI");
			var gameGo = uiGo.transform.Find("UI_Game").Find("Canvas").gameObject;
			var roundStartTextGo = gameGo.transform.Find("RoundStartText");

			roundStartTextGo.position = MainCam.instance.cam.ScreenToWorldPoint(Vector3.Scale(screenPosition, new Vector3(Screen.width, Screen.height, 0f)));

			var roundStartTextPart = roundStartTextGo.GetComponentInChildren<GeneralParticleSystem>();
			var roundStartText = roundStartTextGo.GetComponent<TextMeshProUGUI>();
			var roundStartPulse = roundStartTextGo.GetComponent<RWF.UI.ScalePulse>();

			roundStartTextPart.particleSettings.color = color;
			roundStartTextPart.duration = 60f;
			roundStartTextPart.loop = true;
			roundStartTextPart.Play();
			roundStartText.text = text;
			instance.StopAllCoroutines();
			instance.StartCoroutine(roundStartPulse.StartPulse());
		}
        // overload for custom fonts
        public static void ShowJoinGameText(this UIHandler instance, string text, Color color, TMP_FontAsset font)
        {
            SoundManager.Instance.Play(instance.soundTextAppear, instance.transform);
            if (text != "")
            {
                instance.jointGameText.text = text;
            }
            if (color != Color.black)
            {
                instance.joinGamePart.particleSettings.color = color;
            }
            if (font != null)
            {
                instance.jointGameText.font = font;
            }
            instance.joinGamePart.loop = true;
            instance.joinGamePart.Play();
        }
    }
}
