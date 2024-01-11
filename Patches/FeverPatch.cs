using HarmonyLib;
using Il2Cpp;
using UnityEngine;

namespace Cinema.Patches
{
    [HarmonyPatch(typeof(FeverEffectManager), "ActivateFever")]
    internal class FeverPatch
    {
        // ReSharper disable once InconsistentNaming
        private static void Postfix(Component __instance)
        {
            if (Main.Player == null) return;

            __instance.transform.Find("bg").Find("bg_S").gameObject.SetActive(false);
        }
    }
}