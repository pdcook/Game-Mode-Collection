﻿using System.Reflection;
using BepInEx;
using HarmonyLib;
using MapsExt;
using MapsExt.MapObjects;
using UnboundLib;
using UnityEngine;

namespace GMCObjects
{
    [BepInDependency("com.willis.rounds.unbound")]
    [BepInPlugin(ModId, ModName, Version)]
    [BepInProcess("Rounds.exe")]
    public class GMCObjects : BaseUnityPlugin
    {
        private const string ModId = "com.pykessandbosssloth.rounds.GMCObjects";
        private const string ModName = "GMCObjects";
        public const string Version = "1.0.0";

        public static GMCObjects instance;

        public GameObject CardSpot;

        private void Awake()
        {
            CardSpot = new GameObject("CardSpawnPoint");
        }

        private void Start()
        {
            var harmony = new Harmony(ModId);
            harmony.PatchAll();

            instance = this;
            MapsExtended.instance.RegisterMapObjects();
        }
    }
}