using UnityEngine;
using UnityModManagerNet;

using static UnityModManagerNet.UnityModManager;

// TODO : Add settings here
// trail width
// initial alpha ?

namespace LightTrails
{
    public class Settings : ModSettings, IDrawable
    {
        // [Draw(DrawType.)]

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
        [Draw(DrawType.Slider, Min = 2, Max = 10)]
        public int trailSmoothness = 6;
        [Draw(DrawType.Slider, Min = 1, Max = 5)]
        public float trailMaxLength = 3;

        [Header("Debug")]
        [Draw(DrawType.Toggle)]
        public bool showMarkers;
        [Draw(DrawType.Toggle)]
        //public bool disableInfoLogs = true;
        public bool disableInfoLogs = false;

        public override void Save(ModEntry modEntry) => Save(this, modEntry);

        public void OnChange()
        {
            Main.SetMarkers(showMarkers);

            TrailsSpawner.leftTrail.RefreshSettings();
            TrailsSpawner.rightTrail.RefreshSettings();
        }
    }
}
