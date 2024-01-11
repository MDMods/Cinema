using HarmonyLib;
using Il2Cpp;

namespace Cinema.Patches
{
    [HarmonyPatch(typeof(SceneChangeController), "OnControllerStart")]
    internal class SceneChangePatch
    {
        private static void Postfix()
        {
            if (Main.Player == null) return;

            Main.HideSceneElements();
        }
    }
}