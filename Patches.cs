using HarmonyLib;

namespace LightTrails
{
    [HarmonyPatch(typeof(BrakeEffects), "Awake")]
    static class TrailsSpawner
    {
        public static TrailAnimator leftTrail;
        public static TrailAnimator rightTrail;

        static void Postfix(BrakeEffects __instance)
        {
            leftTrail = __instance.LeftBrakeLightTransform.gameObject.AddComponent<TrailAnimator>();
            rightTrail = __instance.RightBrakeLightTransform.gameObject.AddComponent<TrailAnimator>();
        }
    }

    [HarmonyPatch(typeof(StageScreen), nameof(StageScreen.Restart))]
    static class RestartStagePatch
    {
        static void Postfix()
        {
            TrailsSpawner.leftTrail.ResetTrails();
            TrailsSpawner.rightTrail.ResetTrails();
        }
    }
}
