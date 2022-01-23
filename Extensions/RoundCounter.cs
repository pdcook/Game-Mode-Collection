using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Runtime.CompilerServices;
using System.Linq;
using UnboundLib;
using UnityEngine.UI.ProceduralImage;

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

        public static Transform PointInfoHolder(this RoundCounter instance)
        {
            var pointInfoHolder = instance.p1Parent.parent.parent.Find("PointInfoHolder");
            if (pointInfoHolder == null)
            {
                pointInfoHolder = new GameObject("PointInfoHolder").transform;
                pointInfoHolder.SetParent(instance.p1Parent.parent.parent);
                pointInfoHolder.position = instance.p1Parent.position;
            }
            pointInfoHolder.localScale = Vector3.one;
            return pointInfoHolder;
        }
        public static Transform TeamClock(this RoundCounter instance, int teamID)
        {
            Transform teamClock = instance.PointInfoHolder().Find($"P{teamID}-Clock");
            if (teamClock == null) 
            {
                teamClock = GameObject.Instantiate(PointVisualizer.instance.transform.GetChild(1).Find("Orange").gameObject, instance.PointInfoHolder()).transform;
                teamClock.name = $"P{teamID}-Clock";
                teamClock.SetParent(instance.PointInfoHolder());
                teamClock.SetSiblingIndex(teamID);
                teamClock.transform.Find("Fill").localRotation = Quaternion.Euler(new Vector3(0f, 0f, 180f));
                teamClock.transform.Find("Fill").GetComponent<ProceduralImage>().color = PlayerManager.instance.GetPlayersInTeam(teamID).First().GetTeamColors().color;
                teamClock.transform.Find("Border").GetComponent<ProceduralImage>().color = PlayerManager.instance.GetPlayersInTeam(teamID).First().GetTeamColors().color;
                teamClock.transform.Find("Border").GetComponent<ProceduralImage>().BorderWidth = 20f;
                teamClock.transform.Find("Border").GetComponent<ProceduralImage>().FalloffDistance = 5f;
                teamClock.transform.Find("Mid").GetComponent<ProceduralImage>().enabled = false;
                teamClock.transform.Find("bg").gameObject.SetActive(true);
                teamClock.transform.Find("bg").GetComponent<ProceduralImage>().enabled = true;
                teamClock.transform.Find("Fill").GetComponent<ProceduralImage>().fillAmount = 0f;
                teamClock.transform.Find("Fill").GetComponent<ProceduralImage>().fillMethod = UnityEngine.UI.Image.FillMethod.Radial360;
                teamClock.gameObject.SetActive(true);
            }
            teamClock.transform.localScale = 1 / 8f * Vector3.one;
            float deltaY = instance.p1Parent.parent.Find("P2").position.y - instance.p1Parent.parent.Find("P1").position.y;
            teamClock.transform.position = instance.p1Parent.GetChild(instance.p1Parent.childCount - 1).position + offset + new Vector3(0f, teamID * deltaY, 0f);
            return teamClock;
        }
        public static Transform TeamText(this RoundCounter instance, int teamID)
        {
            Transform teamText = instance.PointInfoHolder().Find($"P{teamID}-Text");
            if (teamText == null) 
            {
                teamText = new GameObject($"P{teamID}-Clock", typeof(TextMeshProUGUI)).transform;
                teamText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
                teamText.GetComponent<TextMeshProUGUI>().fontSize = 10f;
                teamText.GetComponent<TextMeshProUGUI>().color = PlayerManager.instance.GetPlayersInTeam(teamID).First().GetTeamColors().color;
                teamText.SetParent(instance.PointInfoHolder());
                teamText.SetSiblingIndex(teamID);
            }
            teamText.transform.localScale = 1 / 8f * Vector3.one;
            float deltaY = instance.p1Parent.parent.Find("P2").position.y - instance.p1Parent.parent.Find("P1").position.y;
            teamText.transform.position = instance.p1Parent.GetChild(instance.p1Parent.childCount - 1).position + offset + new Vector3(0f, teamID * deltaY, 0f);
            return teamText;
        }

        public static void UpdateClock(this RoundCounter instance, int teamID, float perc, Color? colorToSet = null)
        {
            Color color = colorToSet ?? PlayerManager.instance.GetPlayersInTeam(teamID).First().GetTeamColors().color;

            GameObject teamClock = instance.TeamClock(teamID).gameObject;

            perc = UnityEngine.Mathf.Clamp01(perc);

            teamClock.transform.Find("Fill").GetComponent<ProceduralImage>().color = color;
            teamClock.transform.Find("Border").GetComponent<ProceduralImage>().color = color;
            teamClock.transform.Find("Mid").GetComponent<ProceduralImage>().enabled = false;
            teamClock.transform.Find("Fill").GetComponent<ProceduralImage>().fillAmount = perc;
        }

        public static void UpdateText(this RoundCounter instance, int teamID, string text, Color? colorToSet = null)
        {
            Color color = colorToSet ?? PlayerManager.instance.GetPlayersInTeam(teamID).First().GetTeamColors().color;

            GameObject teamText = instance.TeamText(teamID).gameObject;
            TextMeshProUGUI tmpro = teamText.GetComponent<TextMeshProUGUI>();
            tmpro.text = text;
            tmpro.color = color;
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
