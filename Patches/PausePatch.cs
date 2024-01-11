using HarmonyLib;
using Il2CppFormulaBase;

namespace Cinema.Patches
{
    [HarmonyPatch(typeof(StageBattleComponent), "Pause")]
    internal class PausePatch
    {
        private static void Postfix()
        {
            if (Main.Player == null) return;

            Main.Player.Pause();
        }
    }
}