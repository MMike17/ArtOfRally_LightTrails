using HarmonyLib;

namespace LightTrails
{
    // Patch model
    // [HarmonyPatch(typeof(), nameof())]
    // [HarmonyPatch(typeof(), MethodType.)]
    // static class type_method_Patch
    // {
    // 	static void Prefix()
    // 	{
    // 		//
    // 	}

    //	this will negate the method
    //  	static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    //  	{
    //      	foreach (var instruction in instructions)
    //          	yield return new CodeInstruction(OpCodes.Ret);
    //  	}

    // 	static void Postfix()
    // 	{
    // 		//
    // 	}
    // }

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
