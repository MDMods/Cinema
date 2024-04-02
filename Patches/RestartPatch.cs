using HarmonyLib;
using Il2CppFormulaBase;

namespace Cinema.Patches
{
    [HarmonyPatch(typeof(StageBattleComponent), "GameRestart")]
    internal class RestartPatch
    {
        private static void Postfix()
        {
            if (Main.Player == null) return;

            Main.GameStarted = false;
            DriftCorrector.Stop();
        }
    }
}