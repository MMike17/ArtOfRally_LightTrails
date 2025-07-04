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

    [HarmonyPatch(typeof(PlayerManager), "Init")]
    static class TrailsSpawner
    {
        public static ConditionTypes.Weather weather;

        static void Postfix(PlayerManager __instance, ConditionTypes.Weather _weather)
        {
            weather = _weather;

            BrakeEffects brakeEffects = __instance.PlayerObject.GetComponentInChildren<BrakeEffects>();
            brakeEffects.LeftBrakeLightTransform.gameObject.AddComponent<TrailAnimator>();
            brakeEffects.RightBrakeLightTransform.gameObject.AddComponent<TrailAnimator>();
        }
    }
}
