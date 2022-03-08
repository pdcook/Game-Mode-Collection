using System.Runtime.CompilerServices;
using System.Linq;
using GameModeCollection.GameModes;
using UnityEngine;
using TMPro;
using UnboundLib;
using GameModeCollection.Objects;

namespace GameModeCollection.Extensions
{
    public class CharacterDataAdditionalData
    {
        public int maxAllowedCards = GM_TRT.BaseMaxCards;
        public float TRT_Karma = 1f;
        public PlayerFace CurrentFace;
    }
    public static class CharacterDataExtensions
    {
        private static readonly ConditionalWeakTable<CharacterData, CharacterDataAdditionalData> additionalData = new ConditionalWeakTable<CharacterData, CharacterDataAdditionalData>();
        public static CharacterDataAdditionalData GetData(this CharacterData instance)
        {
            return additionalData.GetOrCreateValue(instance);
        }
        public static void SetCurrentFace(this CharacterData instance, PlayerFace face)
        {
            instance.GetData().CurrentFace = face;
        }
        public static PlayerFace GetCurrentFace(this CharacterData instance)
        {
            return instance.GetData().CurrentFace ?? new PlayerFace() { eyeID = 0, detail2ID = 0, detail2Offset = Vector2.zero, detailID = 0, detailOffset = Vector2.zero, mouthID = 0, eyeOffset = Vector2.zero, mouthOffset = Vector2.zero };
        }
        public static void SetMaxCards(this CharacterData instance, int max)
        {
            instance.GetData().maxAllowedCards = max;
        }
        public static int MaxCards(this CharacterData instance)
        {
            return instance.GetData().maxAllowedCards;
        }
        public static bool CanHaveMoreCards(this CharacterData instance)
        {
            return instance.currentCards.Count(c => !c.categories.Contains(CardItem.IgnoreMaxCardsCategory)) < instance.GetData().maxAllowedCards;
        }
        public static float TRT_Karma(this CharacterData instance)
        {
            return instance.GetData().TRT_Karma; 
        }
        public static void TRT_ResetKarma(this CharacterData instance)
        {
            instance.GetData().TRT_Karma = 1f;
        }
        public static void TRT_ChangeKarma(this CharacterData instance, float amount_to_add, float minimum = 0f)
        {
            instance.GetData().TRT_Karma = UnityEngine.Mathf.Clamp(instance.GetData().TRT_Karma + amount_to_add, minimum, 1f);
        }

        public static void SetNameBackground(this CharacterData instance, Color color)
        {
            TextMeshProUGUI nameText = instance?.GetComponentInChildren<PlayerName>()?.GetComponent<TextMeshProUGUI>();
            if (nameText is null)
            {
                GameModeCollection.LogWarning($"NAME FOR PLAYER {instance?.player?.playerID} IS NULL");
                return;
            }
            Transform background = nameText.transform.parent.Find("Background");
            if (background is null)
            {
                background = new GameObject("Background", typeof(UnityEngine.UI.Image), typeof(PlayerNameSizeFitter)).transform;
                background.SetParent(nameText.transform.parent);
                background.SetAsFirstSibling();
                background.localPosition = Vector3.zero;
                background.localScale = Vector3.one;
            }
            background.GetComponent<UnityEngine.UI.Image>().color = color;
            background.GetComponent<PlayerNameSizeFitter>().CheckForChanges();

        }
        class PlayerNameSizeFitter : MonoBehaviour
        {
            public const float pad = 50f;
            public void CheckForChanges()
            {
                TextMeshProUGUI nameText = transform.parent.GetComponentInChildren<PlayerName>().GetComponent<TextMeshProUGUI>();
                if (nameText is null) { return; }
                nameText.ForceMeshUpdate();
                GetComponent<RectTransform>().sizeDelta = nameText.textBounds.size + pad * Vector3.one;
                this.transform.localPosition = nameText.transform.localPosition + nameText.textBounds.center;
            }
        }
    }
}
