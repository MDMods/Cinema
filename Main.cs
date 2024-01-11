using Il2Cpp;
using Il2CppAssets.Scripts.PeroTools.Commons;
using MelonLoader;
using UnityEngine;
using UnityEngine.Video;
using Object = UnityEngine.Object;

namespace Cinema
{
    public class Main : MelonMod
    {
        internal static bool GameStarted;
        internal static VideoPlayer Player;
        internal static bool Christmas;

        public override void OnInitializeMelon()
        {
            base.OnInitializeMelon();

            // Clean up old file from v1.1.x and below
            if (File.Exists(Application.persistentDataPath + "/cinema.mp4"))
                File.Delete(Application.persistentDataPath + "/cinema.mp4");

            if (File.Exists(Application.dataPath + "/cinema.mp4"))
                File.Delete(Application.dataPath + "/cinema.mp4");
        }

        public override void OnDeinitializeMelon()
        {
            base.OnDeinitializeMelon();

            if (File.Exists(Application.dataPath + "/cinema.mp4"))
                File.Delete(Application.dataPath + "/cinema.mp4");
        }

        internal static void InitCamera(float opacity)
        {
            var camera = Camera.allCameras.Single(c => c.name == "Camera_3D");

            // Create new cinema camera
            var videoCamera = Object.Instantiate(camera);
            videoCamera.name = "Camera_Cinema";
            videoCamera.depth = -2;
            videoCamera.transform.position = new Vector3(0, 0, -9001);
            videoCamera.clearFlags = CameraClearFlags.SolidColor;
            videoCamera.orthographic = true;
            videoCamera.orthographicSize = Camera.main.orthographicSize;
            videoCamera.cullingMask = -1;
            videoCamera.backgroundColor = Color.black;
            Camera.main.clearFlags = CameraClearFlags.Depth;

            // Add video player to new camera
            Player = videoCamera.gameObject.AddComponent<VideoPlayer>();
            Player.playOnAwake = false;
            Player.targetCameraAlpha = opacity;
            Player.audioOutputMode = VideoAudioOutputMode.None;
            Player.aspectRatio = VideoAspectRatio.FitOutside;
            Player.url = Application.dataPath + "/cinema.mp4";
        }

        internal static void HideSceneElements()
        {
            var sceneName = SceneChangeController.curScene > 9
                ? $"scene_{SceneChangeController.curScene}"
                : $"scene_0{SceneChangeController.curScene}";
            if (Christmas && sceneName == "scene_05") sceneName += "_christmas";

            MelonLogger.Msg("SCENE TO HIDE: " + sceneName);

            var sceneObject = GameObject.Find("SceneObjectController").transform.Find(sceneName);
            if (sceneObject == null) return;

            SingletonMonoBehaviour<GameSceneContainer>.instance.sprLightness.color = new Color(0, 0, 0, 0);

            for (var i = 0; i < sceneObject.childCount; i++)
                sceneObject.GetChild(i).gameObject.SetActive(false);

            sceneObject.gameObject.SetActive(false);
        }
    }
}