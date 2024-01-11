using HarmonyLib;
using Il2CppFormulaBase;

namespace Cinema.Patches
{
    [HarmonyPatch(typeof(StageBattleComponent), "Exit")]
    internal class ExitPatch
    {
        private static void Postfix()
        {
            Main.Player = null;
            Main.GameStarted = false;
            Main.Christmas = false;
        }
    }
}