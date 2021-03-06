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

        private static readonly Vector3 horizOffset = new Vector3(0f, 0f, 0f);
        private static readonly Vector3 vertOffset = new Vector3(0f, -28.835f, 0f);

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
                pointInfoHolder.localPosition = new Vector3(40f, pointInfoHolder.localPosition.y, 0f);
                pointInfoHolder.localScale = Vector3.one;
                var background = new GameObject("Background", typeof(UnityEngine.UI.Image), typeof(SizeFitter)).transform;
                background.SetParent(pointInfoHolder);
                background.SetAsFirstSibling();
                background.GetComponent<UnityEngine.UI.Image>().color = Color.clear;
                background.localScale = Vector3.one;
            }
            pointInfoHolder.GetComponentInChildren<SizeFitter>().CheckForChanges();
            pointInfoHolder.Find("Background").SetAsFirstSibling();
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
                teamClock.SetSiblingIndex(teamID + 1);
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
            teamClock.transform.localPosition = horizOffset + teamID * vertOffset;
            return teamClock;
        }
        public static Transform TeamText(this RoundCounter instance, int teamID)
        {
            Transform teamText = instance.PointInfoHolder().Find($"P{teamID}-Text");
            if (teamText == null) 
            {
                teamText = new GameObject($"P{teamID}-Text", typeof(TextMeshProUGUI)).transform;
                teamText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
                teamText.GetComponent<TextMeshProUGUI>().fontSize = 10f;
                teamText.GetComponent<TextMeshProUGUI>().fontSizeMin = 5f;
                teamText.GetComponent<TextMeshProUGUI>().margin = new Vector4(5f, 5f, 5f, 5f);
                teamText.GetComponent<TextMeshProUGUI>().color = PlayerManager.instance.GetPlayersInTeam(teamID).First().GetTeamColors().color;
                teamText.SetParent(instance.PointInfoHolder());
                teamText.SetSiblingIndex(teamID + 1);
            }
            teamText.transform.localScale = 1 / 8f * Vector3.one;
            teamText.transform.localPosition = horizOffset + teamID * vertOffset;
            return teamText;
        }

        public static void UpdateClock(this RoundCounter instance, int teamID, float perc, Color? colorToSet = null, Vector2? scale = null)
        {
            Color color = colorToSet ?? PlayerManager.instance.GetPlayersInTeam(teamID).First().GetTeamColors().color;

            GameObject teamClock = instance.TeamClock(teamID).gameObject;
            teamClock.SetActive(true);

            perc = UnityEngine.Mathf.Clamp01(perc);

            teamClock.transform.Find("Fill").GetComponent<ProceduralImage>().color = color;
            teamClock.transform.Find("Border").GetComponent<ProceduralImage>().color = color;
            teamClock.transform.Find("Mid").GetComponent<ProceduralImage>().enabled = false;
            teamClock.transform.Find("Fill").GetComponent<ProceduralImage>().fillAmount = perc;
            if(scale != null)
            {
                teamClock.transform.localScale = scale.Value;
            }
        }
        public static void RemoveClock(this RoundCounter instance, int teamID)
        {
            instance.TeamClock(teamID).gameObject.SetActive(false);
        }

        public static void UpdateText(this RoundCounter instance, int teamID, string text, Color? colorToSet = null, float? fontSize = null, Vector3? scale = null, Color? backgroundColorToSet = null, bool? autoSize = null)
        {
            Color color = colorToSet ?? PlayerManager.instance.GetPlayersInTeam(teamID).First().GetTeamColors().color;

            GameObject teamText = instance.TeamText(teamID).gameObject;
            TextMeshProUGUI tmpro = teamText.GetComponent<TextMeshProUGUI>();
            tmpro.text = text;
            tmpro.color = color;
            if (fontSize != null) { tmpro.fontSize = (float)fontSize; }
            if (autoSize != null) { tmpro.enableAutoSizing = (bool)autoSize; }
            if (scale != null) { tmpro.gameObject.transform.localScale = (Vector3)scale; }
            if (backgroundColorToSet != null)
            {
                instance.PointInfoHolder().Find("Background").GetComponent<UnityEngine.UI.Image>().color = (Color)backgroundColorToSet;
            }
        }
        public static void ClearTexts(this RoundCounter instance)
        {
            foreach (TextMeshProUGUI text in instance.GetData().teamTexts.Values)
            {
                UnityEngine.GameObject.DestroyImmediate(text.gameObject);
            }
            instance.GetData().teamTexts.Clear();
        }
        class SizeFitter : MonoBehaviour
        {
            public void CheckForChanges()
            {
                TextMeshProUGUI[] children = transform.parent.GetComponentsInChildren<TextMeshProUGUI>();

                float min_x, max_x, min_y, max_y;
                min_x = max_x = transform.localPosition.x;
                min_y = max_y = transform.localPosition.y;

                foreach (RectTransform child in children.Select(c => c.rectTransform))
                {
                    Vector2 scale = child.sizeDelta;
                    float temp_min_x, temp_max_x, temp_min_y, temp_max_y;

                    temp_min_x = child.localPosition.x - (scale.x / 2);
                    temp_max_x = child.localPosition.x + (scale.x / 2);
                    temp_min_y = child.localPosition.y - (scale.y / 2);
                    temp_max_y = child.localPosition.y + (scale.y / 2);

                    if (temp_min_x < min_x)
                        min_x = temp_min_x;
                    if (temp_max_x > max_x)
                        max_x = temp_max_x;

                    if (temp_min_y < min_y)
                        min_y = temp_min_y;
                    if (temp_max_y > max_y)
                        max_y = temp_max_y;
                }
                GetComponent<RectTransform>().sizeDelta = new Vector2(max_x - min_x, max_y - min_y);
                this.transform.localPosition = horizOffset + vertOffset * (this.transform.parent.childCount-2) / 2f;
            }
        }
    }
}
