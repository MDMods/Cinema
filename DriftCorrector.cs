using Il2CppFormulaBase;
using MelonLoader;
using UnityEngine;

namespace Cinema
{
    internal static class DriftCorrector
    {
        // higher == more precise, but may have lower performance
        private const float CorrectionPrecision = 1f;
        private const int ForceSetThreshold = 20;
        private const float DisableCoefficient = 1.1f;
        private static StageBattleComponent _battleComponent;
        private static bool _isInit;
        private static readonly float[] DeltaSamples = new float[50];

        private static int _deltaSampleIndex;
        private static SpeedState _currentSpeed = SpeedState.Normal;

        /// <summary>
        ///     In case another mod wants to change the playback speed (such as PracticeMod),
        ///     <br />
        ///     we store the original playback speed and use it in our calculations.
        /// </summary>
        private static float _originalPlaybackSpeed = 1f;

        private static double _lastUpdateTime;

        internal static void Init()
        {
            if (_isInit) return;
            InitAverageDelta();
            _isInit = true;
        }

        internal static void Stop()
        {
            var del = LateUpdate;
            if (MelonEvents.OnLateUpdate.CheckIfSubscribed(del.Method))
            {
                MelonEvents.OnLateUpdate.Unsubscribe(LateUpdate);
            }

            if (Main.Player is not null) Main.Player.playbackSpeed = _originalPlaybackSpeed;
        }

        internal static void Run()
        {
            _originalPlaybackSpeed = Main.Player.playbackSpeed;
            _lastUpdateTime = 0;
            _battleComponent = StageBattleComponent.instance;
            _currentSpeed = SpeedState.Normal;
            MelonEvents.OnLateUpdate.Subscribe(LateUpdate);
        }

        private static void CalculateAverageDelta()
        {
            DeltaSamples[_deltaSampleIndex++] = Time.deltaTime;
            if (_deltaSampleIndex == DeltaSamples.Length) _deltaSampleIndex = 0;
        }

        private static void InitAverageDelta()
        {
            CalculateAverageDelta();
            for (var i = 1; i < DeltaSamples.Length; i++) DeltaSamples[i] = DeltaSamples[0];
        }

        /// <summary>
        ///     Calculates playback speed using the given parameters
        /// </summary>
        private static float CalculateSpeed(float deltaTime, float drift)
        {
            var averageScaled = deltaTime / 10;
            var relativeDrift = Math.Abs(drift) + 1;
            var speed = Math.Min((1 + averageScaled) * relativeDrift, 1.05f);
            if (drift > 0) speed = 2 - speed;
            ;
            return speed * _originalPlaybackSpeed;
        }

        private static void LateUpdate()
        {
            if (Main.Player is null || Math.Abs(Main.Player.time - _lastUpdateTime) < 0.000001) return;
            if (Main.Player.time >= Main.Player.length)
            {
                Stop();
                return;
            }

            CalculateAverageDelta();
            _lastUpdateTime = Main.Player.time;

            var averageDelta = DeltaSamples.Average();
            var drift = (float)(_lastUpdateTime - _battleComponent.timeFromMusicStart);
            if (Math.Abs(drift) > averageDelta * ForceSetThreshold)
            {
                // If the game freezes, (e.g. extreme drift) instantly set the player's time,
                // instead of slightly speeding up, like when we correct small drifts.
                // This correction may cause a few Update loops worth of drift,
                // but is preferable to a multiple second drift.
                var correct = _battleComponent.timeFromMusicStart + averageDelta * ForceSetThreshold;
                Main.Player.time = correct;
                return;
            }

            var maxDifference = averageDelta / CorrectionPrecision;
            var disableThreshold = maxDifference / DisableCoefficient;
            switch (_currentSpeed)
            {
                case SpeedState.Slow:
                    if (drift > disableThreshold) return;
                    break;
                case SpeedState.Fast:
                    if (drift < -disableThreshold) return;
                    break;
                case SpeedState.Normal:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (drift < -maxDifference)
            {
                _currentSpeed = SpeedState.Fast;
                var speed = CalculateSpeed(averageDelta, drift);
                Main.Player.playbackSpeed = speed;
                return;
            }

            if (drift > maxDifference)
            {
                _currentSpeed = SpeedState.Slow;
                var speed = CalculateSpeed(averageDelta, drift);
                Main.Player.playbackSpeed = speed;
                return;
            }

            if (_currentSpeed is SpeedState.Normal) return;

            Main.Player.playbackSpeed = _originalPlaybackSpeed;
            _currentSpeed = SpeedState.Normal;
        }

        private enum SpeedState
        {
            Slow,
            Normal,
            Fast
        }
    }
}