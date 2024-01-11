using HarmonyLib;
using Il2CppFormulaBase;

namespace Cinema.Patches
{
    [HarmonyPatch(typeof(StageBattleComponent), "Resume")]
    internal class ResumePatch
    {
        private static void Postfix()
        {
            if (Main.Player == null) return;
            if (!Main.GameStarted) return;

            Main.Player.Play();
            ;
        }
    }
}