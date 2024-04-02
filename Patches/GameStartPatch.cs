using HarmonyLib;
using Il2CppFormulaBase;

namespace Cinema.Patches
{
    [HarmonyPatch(typeof(StageBattleComponent), "GameStart")]
    internal class GameStartPatch
    {
        private static void Postfix()
        {
            if (Main.Player == null) return;

            Main.GameStarted = true;
            Main.Player.Play();
            DriftCorrector.Run();
        }
    }
}