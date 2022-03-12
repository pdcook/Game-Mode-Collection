using GameModeCollection.GameModes.TRT.Cards;
using GameModeCollection.GameModeHandlers;
using GameModeCollection.Utils;
using ItemShops.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TMPro;
using UnboundLib;
using UnityEngine;
using ItemShops.Extensions;
using GameModeCollection.Extensions;

namespace GameModeCollection.GameModes.TRT
{
    public static class TRTShopHandler
    {
        public const float TRT_Perc_Inno_For_Reward = 0.35f; // every time 35% of the innocents are killed, the traitors are rewarded with a credit

        public const string TRT_Currency = "TRT_Currency";

        public static readonly Tag TraitorTag = new Tag("Traitor");
        public static readonly Tag DetectiveTag = new Tag("Detective");
        public static readonly Tag ZombieTag = new Tag("Zombie");
        public static readonly Tag WeaponTag = new Tag("Weapon");
        public static readonly Tag EquipmentTag = new Tag("Equipment");

        private static Shop ZShop;
        private static Shop TShop;
        private static Shop DShop;
        private static Shop TDShop;

        private static Dictionary<CardCategory, Tag[]> CategoriesToTags = new Dictionary<CardCategory, Tag[]>()
        {
            {TRTCardCategories.TRT_Traitor, new Tag[] {TraitorTag} },
            {TRTCardCategories.TRT_Detective, new Tag[]{ DetectiveTag } },
            {TRTCardCategories.TRT_Zombie, new Tag[]{ ZombieTag } },
            {TRTCardCategories.TRT_Slot_0, new Tag[]{ EquipmentTag } },
            {TRTCardCategories.TRT_Slot_1, new Tag[]{ EquipmentTag } },
            {TRTCardCategories.TRT_Slot_2, new Tag[]{ WeaponTag } },
            {TRTCardCategories.TRT_Slot_3, new Tag[]{ EquipmentTag } }
        };
        internal static void GiveCreditToPlayer(Player player, int amount = 1)
        {
            player.GetAdditionalData().bankAccount.Deposit(new Dictionary<string, int> { { TRT_Currency, amount } });
            if (player.data.view.IsMine)
            {
                TRTHandler.SendChat(null, $"{RoleManager.GetRoleColoredName(RoleManager.GetPlayerRole(player).Appearance)}, you have been awarded {(amount == 1 ? "one" : $"{amount}")} credit{(amount==1?"":"s")} for your performance.", true);
            }
        }

        private static Tag[] TagsFromCardCategory(CardInfo card)
        {
            List<Tag> tags = new List<Tag>() { };
            foreach (CardCategory category in card.categories)
            {
                if (category != null && CategoriesToTags.TryGetValue(category, out Tag[] categoryTags) && categoryTags != null)
                {
                    tags.AddRange(categoryTags);
                }
            }
            return tags.ToArray();
        }

        internal static void BuildTRTShops()
        {
            List<CardInfo> ActiveAndHiddenCards = (List<CardInfo>)typeof(ModdingUtils.Utils.Cards).GetProperty("ACTIVEANDHIDDENCARDS", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(ModdingUtils.Utils.Cards.instance, null);
            List<CardInfo> TraitorCards = ActiveAndHiddenCards.Where(c => c.categories.Contains(TRTCardCategories.TRT_Traitor)).Distinct().ToList();
            List<CardInfo> DetectiveCards = ActiveAndHiddenCards.Where(c => c.categories.Contains(TRTCardCategories.TRT_Detective)).Distinct().ToList();
            List<CardInfo> TDCards = TraitorCards.Concat(DetectiveCards).Distinct().ToList();
            List<CardInfo> ZombieCards = ActiveAndHiddenCards.Where(c => c.categories.Contains(TRTCardCategories.TRT_Zombie)).Distinct().ToList();

            if (TShop is null)
            {
                TShop = ShopManager.instance.CreateShop("TRT Traitor Shop");
                TShop.AddItems(TraitorCards.Select(c => new PurchasableTRTCard(c, new Dictionary<string, int>() { { TRT_Currency, 1 } }, TagsFromCardCategory(c))).ToArray());
            }

            if (DShop is null)
            {
                DShop = ShopManager.instance.CreateShop("TRT Detective Shop");
                DShop.AddItems(DetectiveCards.Select(c => new PurchasableTRTCard(c, new Dictionary<string, int>() { { TRT_Currency, 1 } }, TagsFromCardCategory(c))).ToArray());
            }

            if (TDShop is null)
            {
                TDShop = ShopManager.instance.CreateShop("TRT Shop");
                TDShop.AddItems(TDCards.Select(c => new PurchasableTRTCard(c, new Dictionary<string, int>() { { TRT_Currency, 1 } }, TagsFromCardCategory(c))).ToArray());
            }

            if (ZShop is null)
            {
                ZShop = ShopManager.instance.CreateShop("TRT Zombie Shop");
                ZShop.AddItems(ZombieCards.Select(c => new PurchasableTRTCard(c, new Dictionary<string, int>() { { TRT_Currency, 1 } }, TagsFromCardCategory(c))).ToArray());
            }
        }
        
        internal static void ToggleTraitorShop(Player player)
        {
            if (TShop.IsOpen || player is null)
            {
                TShop.Hide();
            }
            else
            {
                TShop.Show(player);
            }
        }
        internal static void ToggleDetectiveShop(Player player)
        {
            if (DShop.IsOpen || player is null)
            {
                DShop.Hide();
            }
            else
            {
                DShop.Show(player);
            }
        }
        internal static void ToggleTDShop(Player player)
        {
            if (TDShop.IsOpen || player is null)
            {
                TDShop.Hide();
            }
            else
            {
                TDShop.Show(player);
            }
        }
        internal static void ToggleZombieShop(Player player)
        {
            if (ZShop.IsOpen || player is null)
            {
                ZShop.Hide();
            }
            else
            {
                ZShop.Show(player);
            }
        }
        internal static void CloseAllShops()
        {
            if (TShop?.IsOpen ?? false)
            {
                TShop.Hide();
            }
            if (DShop?.IsOpen ?? false)
            {
                DShop.Hide();
            }
            if (TDShop?.IsOpen ?? false)
            {
                TDShop.Hide();
            }
            if (ZShop?.IsOpen ?? false)
            {
                ZShop.Hide();
            }
        }
    }
    public class PurchasableTRTCard : PurchasableCard
    {
        public PurchasableTRTCard(CardInfo card, Dictionary<string, int> cost, Tag[] tags) : base(card, cost, tags)
        { }
        public PurchasableTRTCard(CardInfo card, Dictionary<string, int> cost, Tag[] tags, string name) : base(card, cost, tags, name)
        { }

        public override void OnPurchase(Player player, Purchasable item)
        {
            CardInfo card = ((PurchasableTRTCard)item).Card;
            CardUtils.AddCardToPlayer_ClientsideCardBar(player, card, false);
        }
        public override GameObject CreateItem(GameObject parent)
        {
            GameObject container = null;
            GameObject holder = null;

            try
            {
                container = GameObject.Instantiate(ItemShops.ItemShops.instance.assets.LoadAsset<GameObject>("Card Container"));
            }
            catch (Exception)
            {

                UnityEngine.Debug.Log("Issue with creating the card container");
            }

            try
            {
                holder = container.transform.Find("Card Holder").gameObject;
            }
            catch (Exception)
            {

                UnityEngine.Debug.Log("Issue with getting the Card Holder");
                holder = container.transform.GetChild(0).gameObject;
            }
            holder.transform.localPosition = new Vector3(0f, -95f, 0f);
            holder.transform.localScale = new Vector3(0.11f, 0.11f, 1f);

            GameObject cardObj = null;

            try
            {
                cardObj = GetCardVisuals(this.Card, holder);
            }
            catch (Exception e)
            {
                UnityEngine.Debug.Log("Issue with getting card visuals");
                UnityEngine.Debug.LogError(e);
            }

            container.transform.SetParent(parent.transform);

            return container;
        }

        public IEnumerator ShowCard(Player player, CardInfo card)
        {
            yield return ModdingUtils.Utils.CardBarUtils.instance.ShowImmediate(player, card, 2f);

            yield break;
        }

        private GameObject GetCardVisuals(CardInfo card, GameObject parent)
        {

            GameObject cardObj = GameObject.Instantiate<GameObject>(card.gameObject, parent.gameObject.transform);
            cardObj.SetActive(true);
            RectTransform rect = cardObj.GetOrAddComponent<RectTransform>();
            rect.localScale = 100f * Vector3.one;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            rect.pivot = new Vector2(0.5f, 0.5f);

            GameObject back = FindObjectInChildren(cardObj, "Back");
            try
            {
                GameObject.Destroy(back);
            }
            catch { }
            FindObjectInChildren(cardObj, "BlockFront")?.SetActive(false);

            var canvasGroups = cardObj.GetComponentsInChildren<CanvasGroup>();
            foreach (var canvasGroup in canvasGroups)
            {
                canvasGroup.alpha = 1;
            }

            var particles = cardObj.GetComponentsInChildren<GeneralParticleSystem>().Select(system => system.gameObject);
            foreach (var particle in particles)
            {
                UnityEngine.GameObject.Destroy(particle);
            }

            var titleText = FindObjectInChildren(cardObj, "Text_Name").GetComponent<TextMeshProUGUI>();

            if ((titleText.color.r < 0.18f) && (titleText.color.g < 0.18f) && (titleText.color.b < 0.18f))
            {
                titleText.color = new Color32(200, 200, 200, 255);
            }

            return cardObj;
        }
        private static GameObject FindObjectInChildren(GameObject gameObject, string gameObjectName)
        {
            Transform[] children = gameObject.GetComponentsInChildren<Transform>(true);
            return (from item in children where item.name == gameObjectName select item.gameObject).FirstOrDefault();
        }
    }
}
