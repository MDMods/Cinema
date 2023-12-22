using System;
using Assets.Scripts.Database;
using Assets.Scripts.PeroTools.Commons;
using CustomAlbums;
using FormulaBase;
using HarmonyLib;
using MelonLoader;
using System.IO;
using System.Linq;
using GameLogic;
using UnityEngine;
using UnityEngine.Video;
using Object = UnityEngine.Object;

namespace Cinema
{
    public class Main : MelonMod
    {
        public static bool GameStarted = false;
        public static VideoPlayer Player;
        public static CinemaInfo CinInfo;
        public static bool Christmas;

        public static void InitCamera()
        {
            string musicUid = DataHelper.selectedMusicUid;
            if (!musicUid.StartsWith("999")) return;

            Album currentAlbum = AlbumManager.LoadedAlbums.Values.First(a => a.Index.ToString() == musicUid.Remove(0, 4));
            if (currentAlbum == null) return;

            CinInfo = new CinemaInfo(currentAlbum);
            if (CinInfo.jsonData == null || !CinInfo.CinemaEnabled) return;

            byte[] videoBytes = currentAlbum.IsPackaged ? CinInfo.GetArchiveVideoBytes() : File.ReadAllBytes(CinInfo.FilePath);
            File.WriteAllBytes(Application.persistentDataPath + "/cinema.mp4", videoBytes);


            // Find the camera
            Camera camToCopy = null;
            foreach (var cam in Camera.allCameras)
            {
                if (cam.name == "Camera_3D")
                {
                    camToCopy = cam;
                    break;
                }
            }
            if (camToCopy == null) return;

            // Create the new camera for cinema injection
            var camera = Object.Instantiate(camToCopy);
            camera.name = "Camera_Cinema";
            camera.depth = -2;
            camera.transform.position = new Vector3(0, 0, -9001);
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.orthographic = true;
            camera.orthographicSize = Camera.main.orthographicSize;
            camera.cullingMask = -1;
            camera.backgroundColor = Color.black;
            Camera.main.clearFlags = CameraClearFlags.Depth;

            // Add video player to new camera
            Player = camera.gameObject.AddComponent<VideoPlayer>();
            Player.playOnAwake = false;
            Player.targetCameraAlpha = CinInfo.Opacity;
            Player.audioOutputMode = VideoAudioOutputMode.None;
            Player.aspectRatio = VideoAspectRatio.FitOutside;
            Player.url = Application.persistentDataPath + "/cinema.mp4";

            SingletonMonoBehaviour<GameSceneContainer>.instance.sprLightness.color = new Color(0, 0, 0, 0);

            // Support for Miku scene and Christmas scene
            var sceneNumber = currentAlbum.Info.scene.Split('_')[1];
            HideAll($"scene_{sceneNumber}{(!Christmas ? string.Empty : "_christmas")}");
        }

        public static void HideAll(string sceneName)
        {
            Transform sceneObject = GameObject.Find("SceneObjectController").transform.Find(sceneName);
            if (sceneObject == null) return;

            sceneObject.gameObject.SetActive(false);

            for (int i = 0; i < sceneObject.childCount; i++)
            {
                sceneObject.GetChild(i).gameObject.SetActive(false);
            }
        }
    }

    [HarmonyPatch(typeof(StageBattleComponent), "Load")]
    internal static class Load_Patch
    {
        [HarmonyPostfix]
        private static void Postfix()
        {
            Main.InitCamera();
        }
    }

    [HarmonyPatch(typeof(GameMusicScene), nameof(GameMusicScene.SceneFestival))]
    internal static class SceneFestival_Patch
    {
        [HarmonyFinalizer]
        private static void Finalizer(string __result)
        {
            // Checks if Christmas scene
            Main.Christmas = __result.EndsWith("christmas");
        }
    }

    [HarmonyPatch(typeof(SceneChangeController), "OnControllerStart")]
    internal static class OnControllerStart_Patch
    {
        [HarmonyPrefix]
        private static void Prefix()
        {
            if (Main.Player == null) return;

            // Support for Miku scene and Christmas scene
            var sceneSuffix = SceneChangeController.curScene.ToString().Length == 1
                ? $"0{SceneChangeController.curScene}"
                : SceneChangeController.curScene.ToString();
            Main.HideAll($"scene_{sceneSuffix}{(!Main.Christmas ? string.Empty : "_christmas")}");
        }

        [HarmonyPostfix]
        private static void Postfix()
        {
            if (Main.Player == null) return;

            // Support for Miku scene and Christmas scene
            var sceneSuffix = SceneChangeController.curScene.ToString().Length == 1
                ? $"0{SceneChangeController.curScene}"
                : SceneChangeController.curScene.ToString();
            Main.HideAll($"scene_{sceneSuffix}{(!Main.Christmas ? string.Empty : "_christmas")}");
        }
    }

    [HarmonyPatch(typeof(StageBattleComponent), "Pause")]
    internal static class Pause_Patch
    {
        [HarmonyPrefix]
        private static void Prefix()
        {
            if (Main.Player != null) Main.Player.Pause();
        }
    }

    [HarmonyPatch(typeof(StageBattleComponent), "Resume")]
    internal static class Resume_Patch
    {
        [HarmonyPrefix]
        private static void Prefix()
        {
            if (!Main.GameStarted) return;
            if (Main.Player != null) Main.Player.Play();
        }
    }

    [HarmonyPatch(typeof(StageBattleComponent), "GameStart")]
    internal static class GameStart_Patch
    {
        [HarmonyPostfix]
        private static void Postfix()
        {
            if (Main.Player == null) return;
            Main.GameStarted = true;
            Main.Player.Play();
        }
    }

    [HarmonyPatch(typeof(StageBattleComponent), "GameRestart")]
    internal static class ReEnter_Patch
    {
        [HarmonyPrefix]
        private static void Prefix()
        {
            Main.GameStarted = false;
        }
    }

    [HarmonyPatch(typeof(StageBattleComponent), "Exit")]
    internal static class Exit_Patch
    {
        [HarmonyPostfix]
        private static void Postfix()
        {
            Main.Player = null;
            Main.CinInfo = null;
            Main.GameStarted = false;
        }
    }

    [HarmonyPatch(typeof(FeverEffectManager), "ActivateFever")]
    internal static class RemoveFeverBg_Patch
    {
        [HarmonyPostfix]
        private static void Postfix(FeverEffectManager __instance)
        {
            if (Main.Player == null) return;
            __instance.transform.Find("bg").Find("bg_S").gameObject.SetActive(false);
        }
    }
}
