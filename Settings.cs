using UnityEngine;
using UnityModManagerNet;

using static UnityModManagerNet.UnityModManager;

namespace LightTrails
{
    public class Settings : ModSettings, IDrawable
    {
        [Header("General")]
        [Draw(DrawType.Toggle)]
        public bool showMorning;
        [Draw(DrawType.Toggle)]
        public bool showAfternoon;
        [Draw(DrawType.Toggle)]
        public bool showSunset;
        [Draw(DrawType.Toggle)]
        public bool showNight;
        [Draw(DrawType.Toggle)]
        public bool showFog;
        [Draw(DrawType.Toggle)]
        public bool showRain;
        [Draw(DrawType.Toggle)]
        public bool showSnow;

        [Header("Trails")]
        [Draw(DrawType.Slider, Min = 1, Max = 4)]
        public int trailSmoothness = 3;
        [Draw(DrawType.Slider, Min = 2, Max = 20)]
        public float trailMaxLength = 10;
        [Draw(DrawType.Slider, Min = 0.1f, Max = 0.6f)]
        public float trailWidth = 0.3f;
        [Draw(DrawType.Slider, Min = 0.1f, Max = 1)]
        public float trailAlpha = 0.8f;
        [Draw(DrawType.Toggle)]
        public bool brakeTrailOnly = false;

        [Header("Debug")]
        [Draw(DrawType.Toggle)]
        public bool disableInfoLogs = true;

        public override void Save(ModEntry modEntry) => Save(this, modEntry);

        public void OnChange()
        {
            trailMaxLength = SnapValue(trailMaxLength, 10, 20 - 2, 0.1f);
            trailWidth = SnapValue(trailWidth, 0.3f, 0.6f - 0.1f, 0.1f);
            trailAlpha = SnapValue(trailAlpha, 0.8f, 1 - 0.1f, 0.1f);

            TrailsSpawner.leftTrail.RefreshSettings();
            TrailsSpawner.rightTrail.RefreshSettings();
        }

        private float SnapValue(float value, float snapValue, float range, float snapPercent)
        {
            float snapDiff = range * snapPercent;
            float minTarget = snapValue - snapDiff / 2;
            float maxTarget = snapValue + snapDiff / 2;
            return value <= maxTarget && value >= minTarget ? snapValue : value;
        }
    }
}
