using UnityEngine;
using UnboundLib;
using System.Linq;
namespace GameModeCollection.Extensions
{
    static class CardVisualsExtensions
    {
        public static void AssignCardInfo(this CardVisuals instance, CardInfo cardInfo)
        {
            instance.GetComponentInParent<CardInfo>().rarity = cardInfo.rarity;

            CardVisuals source = cardInfo.sourceCard.GetComponent<CardVisuals>();

            instance.SetFieldValue("defaultColor", CardChoice.instance.GetCardColor(cardInfo.colorTheme));
            instance.SetFieldValue("selectedColor", CardChoice.instance.GetCardColor2(cardInfo.colorTheme));
            if (cardInfo.cardArt)
            {
                Transform artTransform = instance.transform.Find("Canvas/Front/Background/Art");
                GameObject art = GameObject.Instantiate<GameObject>(cardInfo.cardArt, artTransform.position, artTransform.rotation, artTransform);
                art.transform.localPosition = Vector3.zero;
                art.transform.SetAsFirstSibling();
                art.transform.localScale = Vector3.one;
            }
            instance.SetFieldValue("cardAnims", instance.GetComponentsInChildren<CardAnimation>());
            instance.isSelected = false;

            instance.chillColor = source.chillColor;
        }
    }
}
