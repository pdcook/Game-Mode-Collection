using System;
using System.Collections.Generic;
using UnityEngine;
using GameModeCollection;
using UnboundLib;
using HarmonyLib;
using System.Reflection;
using System.Reflection.Emit;
using System.Linq;

namespace GameModeCollection.Patches
{
    [HarmonyPatch(typeof(SFPolygon), "OnEnable")]
    class SFPolygonPatchOnEnable
    {
        static bool Prefix()
        {
            return !GameModeCollection.DisableMapShadows;
        }        
    }
}
