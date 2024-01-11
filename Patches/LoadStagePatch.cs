using Cinema.Data;
using CustomAlbums.Managers;
using HarmonyLib;
using Il2CppAssets.Scripts.Database;
using Il2CppFormulaBase;
using UnityEngine;

namespace Cinema.Patches
{
    [HarmonyPatch(typeof(StageBattleComponent), "Load")]
    internal static class LoadStagePatch
    {
        internal static string LastMusicUid = "";

        private static void Postfix()
        {
            var musicUid = DataHelper.selectedMusicUid;
            if (!musicUid.StartsWith("999")) return;

            var currentAlbum =
                AlbumManager.LoadedAlbums.Values.SingleOrDefault(a => a.Index.ToString() == musicUid.Remove(0, 4));
            if (currentAlbum == null) return;

            var cinemaInfo = new CinemaInfo(currentAlbum);
            if (!cinemaInfo.Enabled) return;

            if (LastMusicUid != musicUid)
            {
                var videoBytes = cinemaInfo.GetVideo();
                File.WriteAllBytes(Application.dataPath + "/cinema.mp4", videoBytes);
                LastMusicUid = musicUid;
            }

            Main.InitCamera(cinemaInfo.Opacity);
            Main.HideSceneElements();
        }
    }
}