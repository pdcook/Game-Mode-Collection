using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Runtime.CompilerServices;

namespace GameModeCollection.Extensions
{
    public class RoundCounterAdditionalData
    {
        public Dictionary<int, TextMeshProUGUI> teamTexts = new Dictionary<int, TextMeshProUGUI>() { };
    }
    public static class RoundCounterExtensions
    {

        private static readonly Vector3 offset = new Vector3(2f, 0f, 0f);

        private static readonly ConditionalWeakTable<RoundCounter, RoundCounterAdditionalData> additionalData = new ConditionalWeakTable<RoundCounter, RoundCounterAdditionalData>();
        public static RoundCounterAdditionalData GetData(this RoundCounter instance)
        {
            return additionalData.GetOrCreateValue(instance);
        }
        public static void UpdateText(this RoundCounter instance, int teamID, string text, Color? colorToSet = null)
        {
            Color color = colorToSet ?? new Color32(230, 230, 230, 255);

            var parent = instance.p1Parent.parent.parent.Find("PointTextHolder");
            if (parent == null)
            {
                parent = new GameObject("PointTextHolder").transform;
                parent.SetParent(instance.p1Parent.parent.parent);
                parent.position = instance.p1Parent.position;
            }

            GameObject textHolder;
            if (teamID >= parent.childCount) 
            { 
                textHolder = new GameObject($"P{teamID}", typeof(TextMeshProUGUI));
                textHolder.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
                textHolder.transform.SetParent(parent);
                textHolder.transform.SetSiblingIndex(teamID);
            }
            textHolder = parent.GetChild(teamID).gameObject;
            textHolder.transform.localScale = 1 / 8f * Vector3.one;
            float deltaY = instance.p1Parent.parent.Find("P2").position.y - instance.p1Parent.parent.Find("P1").position.y;
            textHolder.transform.position = instance.p1Parent.GetChild(instance.p1Parent.childCount - 1).position + offset + new Vector3(0f, teamID * deltaY, 0f);
            TextMeshProUGUI tmpro = textHolder.GetComponent<TextMeshProUGUI>();
            tmpro.text = text;
            tmpro.color = color;
            tmpro.fontSize = 10f;
        }
        public static void ClearTexts(this RoundCounter instance)
        {
            foreach (TextMeshProUGUI text in instance.GetData().teamTexts.Values)
            {
                UnityEngine.GameObject.DestroyImmediate(text.gameObject);
            }
            instance.GetData().teamTexts.Clear();
        }
    }
}
