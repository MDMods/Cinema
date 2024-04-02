using Il2CppFormulaBase;
using MelonLoader;
using UnityEngine;

namespace Cinema
{
    static class DriftCorrector
    {
        enum SpeedState
        {
            Slow,
            Normal,
            Fast
        }
        static StageBattleComponent BattleComponent;
        static bool IsInit = false;
        internal static void Init()
        {
            if (IsInit)
            {
                return;
            }
            InitAverageDelta();
            IsInit = true;
        }
        internal static void Stop()
        {
            var del = LateUpdate;
            if (MelonEvents.OnLateUpdate.CheckIfSubscribed(del.Method))
            {
#if DEBUG
                MelonLogger.Msg("Disabled drift detection");
#endif
                MelonEvents.OnLateUpdate.Unsubscribe(LateUpdate);
            };
            if (Main.Player is not null)
            {
                Main.Player.playbackSpeed = OriginalPlaybackSpeed;
            }
        }

        internal static void Run()
        {
            OriginalPlaybackSpeed = Main.Player.playbackSpeed;
            LastUpdateTime = 0;
            BattleComponent = StageBattleComponent.instance;
            CurrentSpeed = SpeedState.Normal;
#if DEBUG
            MelonLogger.Msg("Enabled drift detection");
            MelonLogger.Msg("Original speed: " + OriginalPlaybackSpeed);
#endif
            MelonEvents.OnLateUpdate.Subscribe(LateUpdate);
        }
        static readonly float[] DeltaSamples = new float[50];
        static int DeltaSampleIndex;
        // higher == more precise, but may have lower performance
        static readonly float CorrectionPrecision = 1f;
        static readonly int ForceSetThreshold = 20;
        static readonly float DisableCoefficient = 1.1f;
        static SpeedState CurrentSpeed = SpeedState.Normal;
        static void CalculateAverageDelta()
        {
            DeltaSamples[DeltaSampleIndex++] = Time.deltaTime;
            if (DeltaSampleIndex == DeltaSamples.Length)
            {
                DeltaSampleIndex = 0;
            }
        }
        static void InitAverageDelta()
        {
            CalculateAverageDelta();
            for (int i = 1; i < DeltaSamples.Length; i++)
            {
                DeltaSamples[i] = DeltaSamples[0];
            }
        }
        /// <summary>
        /// Calculates playback speed using the given parameters
        /// </summary>
        static float CalculateSpeed(float deltaTime, float drift)
        {
            var averageScaled = deltaTime / 10;
            var relativeDrift = Math.Abs(drift) + 1;
            var speed = Math.Min((1 + averageScaled) * relativeDrift, 1.05f);
            if (drift > 0)
            {
                speed = 2-speed;
            };
            return speed*OriginalPlaybackSpeed;
        }
        /// <summary>
        /// In case another mod wants to change the playback speed (such as PracticeMod),
        /// <br/>
        /// we store the original playback speed and use it in our calculations.
        /// </summary>
        static float OriginalPlaybackSpeed = 1f;
        static double LastUpdateTime = 0;
        static void LateUpdate()
        {
            if (Main.Player is null || Main.Player.time == LastUpdateTime)
            {
                return;
            }
            if (Main.Player.time >= Main.Player.length)
            {
                Stop();
                return;
            }
            CalculateAverageDelta();
            LastUpdateTime = Main.Player.time;

            var averageDelta = DeltaSamples.Average();
            var drift = (float)(LastUpdateTime - BattleComponent.timeFromMusicStart);
            if (Math.Abs(drift) > averageDelta*ForceSetThreshold)
            {
                // If the game freezes, (e.g. extreme drift) instantly set the player's time,
                // instead of slightly speeding up, like when we correct small drifts.
                // This correction may cause a few Update loops worth of drift,
                // but is preferrable to a multiple second drift.
                var correct = BattleComponent.timeFromMusicStart + (averageDelta * ForceSetThreshold);
#if DEBUG
                MelonLogger.Msg($"Hard-correcting time ({Main.Player.time}) to {BattleComponent.timeFromMusicStart} ({correct})");
#endif
                Main.Player.time = correct;
                return;
            }
            var maxDifference = averageDelta / CorrectionPrecision;
            var disableThreshold = maxDifference / DisableCoefficient;
            switch (CurrentSpeed)
            {
                case SpeedState.Slow:
                    if (drift > disableThreshold)
                    {
                        return;
                    }
                    break;
                case SpeedState.Fast:
                    if (drift < -disableThreshold)
                    {
                        return;
                    }
                    break;
                default:
                    break;
            }
            if (drift < -maxDifference)
            {
                CurrentSpeed = SpeedState.Fast;
                var speed = CalculateSpeed(averageDelta, drift);
                Main.Player.playbackSpeed = speed;
#if DEBUG
                MelonLogger.Msg($"Correcting negative drift of {drift}... (avg={averageDelta}, speed={speed})");
#endif
                return;
            }
            if (drift > maxDifference)
            {
                CurrentSpeed = SpeedState.Slow;
                var speed = CalculateSpeed(averageDelta, drift);
                Main.Player.playbackSpeed = speed;
#if DEBUG
                MelonLogger.Msg($"Correcting positive drift of {drift}... (avg={averageDelta}, speed={speed})");
#endif
                return;
            }
            if (CurrentSpeed is SpeedState.Normal)
            {
                return;
            }
#if DEBUG
            MelonLogger.Msg("Drift corrected.");
#endif

            Main.Player.playbackSpeed = OriginalPlaybackSpeed;
            CurrentSpeed = SpeedState.Normal;
        }
    }
}