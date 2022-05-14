using System.Reflection;
using BepInEx;
using HarmonyLib;
using MapsExt;
using MapsExt.MapObjects;
using UnboundLib;
using UnityEngine;
using Jotunn.Utils;

namespace GameModeCollection.GMCObjects
{
    [BepInDependency("com.willis.rounds.unbound")]
    [BepInPlugin(ModId, ModName, Version)]
    [BepInProcess("Rounds.exe")]
    public class GMCObjects : BaseUnityPlugin
    {
        private const string ModId = "com.pykess.rounds.GMCObjects";
        private const string ModName = "GMCObjects";
        public const string Version = "1.1.0";

        public static GMCObjects instance;

        public GameObject CardSpot;

        private void Awake()
        {
            CardSpot = new GameObject("CardSpawnPoint");
        }

        private void Start()
        {
            instance = this;
            MapsExtended.instance.RegisterMapObjects();
        }
    }
}