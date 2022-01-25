using HarmonyLib;
using UnityEngine;
using GameModeCollection.Objects;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Linq;

namespace GameModeCollection.Patches
{
    [HarmonyPatch(typeof(Explosion), nameof(Explosion.Explode))]
    class Explosion_Patch_Explode
    {
        // patch to allow explosions to effect PhysicsItems
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {

            /* NOTES:
             * the "array" of Collider2Ds can be accessed with ldloc.1
             * the current index of the loop can be access with ldloc.2
             * the sequence: ldloc.1, ldloc.2, ldelem.ref will load a reference to the collider at index i in the array, as type "object"
             */


            return instructions;
        }
    }
}
