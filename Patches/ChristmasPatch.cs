using HarmonyLib;
using Il2CppGameLogic;

namespace Cinema.Patches
{
    [HarmonyPatch(typeof(GameMusicScene), nameof(GameMusicScene.SceneFestival))]
    internal class ChristmasPatch
    {
        // ReSharper disable once InconsistentNaming
        private static void Postfix(string __result)
        {
            if (__result.EndsWith("christmas"))
                Main.Christmas = true;
        }
    }
}