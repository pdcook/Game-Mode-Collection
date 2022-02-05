using UnityEngine;
using UnityEngine.UI;
using UnboundLib;
using System.Reflection;
namespace GameModeCollection.Objects
{
    static class Sprites
    {
        public static Sprite Circle => PlayerAssigner.instance.playerPrefab.transform.GetChild(0).GetChild(0).gameObject.GetComponent<SpriteRenderer>().sprite;
        public static Sprite Box => ((GameObject)Resources.Load("4 map objects/Box")).GetComponentInChildren<SpriteRenderer>().sprite;
        public static Sprite Triangle => ((CardInfo)typeof(Unbound).GetField("templateCard", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null))?.cardBase?.transform?.Find("Canvas/Front/Background/UI_ParticleSystem")?.GetComponent<GeneralParticleSystem>()?.particleObject?.GetComponent<Image>()?.sprite;
    }
}
