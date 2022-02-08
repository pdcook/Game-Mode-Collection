using System.Collections;
using UnityEngine;
using UnboundLib;
using System.Collections.Generic;
using Sonigon;

namespace GameModeCollection.Extensions
{
    static class PointVisualizerExtensions
    {
        // Overload for the existing DoSequence method for custom sequence messages
        public static IEnumerator DoSequence(this PointVisualizer instance, string text, Color? color = null, float fontSize = 100f)
        {
            yield return new WaitForSecondsRealtime(0.45f);

            SoundManager.Instance.Play(instance.soundWinRound, instance.transform);
            instance.bg.SetActive(true);

            // hide balls
            for (int i = 0; i < instance.transform.GetChild(1).childCount; i++)
            {
                instance.transform.GetChild(1).GetChild(i).gameObject.SetActive(false);
            }

            yield return new WaitForSecondsRealtime(0.2f);

            GamefeelManager.instance.AddUIGameFeelOverTime(10f, 0.1f);
            instance.text.text = text;
            instance.text.color = color ?? new Color32(230,230,230,255);
            instance.text.fontSize = fontSize;

            yield return new WaitForSecondsRealtime(1.8f);

            yield return new WaitForSecondsRealtime(0.25f);

            instance.InvokeMethod("Close");
        }
    }
}
