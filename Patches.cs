using HarmonyLib;

namespace LightTrails
{
    [HarmonyPatch(typeof(EventManager), nameof(EventManager.StartEvent))]
    static class TrailsSpawner
    {
        public static TrailAnimator leftTrail;
        public static TrailAnimator rightTrail;

        static void Postfix(EventManager __instance)
        {
            Main.OnToggle += value =>
            {
                leftTrail?.SetVisibility(value);
                rightTrail?.SetVisibility(value);
            };

            BrakeEffects brakelights = __instance.playerManager.carcontroller.GetComponentInChildren<BrakeEffects>();

            leftTrail = brakelights.LeftBrakeLightTransform.gameObject.AddComponent<TrailAnimator>();
            rightTrail = brakelights.RightBrakeLightTransform.gameObject.AddComponent<TrailAnimator>();
        }
    }

    [HarmonyPatch(typeof(StageScreen), nameof(StageScreen.Restart))]
    static class RestartTrailReseter
    {
        static void Postfix()
        {
            TrailsSpawner.leftTrail?.ResetTrails();
            TrailsSpawner.rightTrail?.ResetTrails();
        }
    }

    [HarmonyPatch(typeof(OutOfBoundsManager))]
    static class RecoverTrailReseter
    {
        [HarmonyPatch("ResetProperties")]
        [HarmonyPostfix]
        static void RecoverPostfix()
        {
            TrailsSpawner.leftTrail?.ResetTrails();
            TrailsSpawner.rightTrail?.ResetTrails();
        }

        [HarmonyPatch(nameof(OutOfBoundsManager.SetResettingInProgress))]
        [HarmonyPostfix]
        static void OutOfBoundsPostfix()
        {
            TrailsSpawner.leftTrail?.ResetTrails();
            TrailsSpawner.rightTrail?.ResetTrails();
        }
    }
}
