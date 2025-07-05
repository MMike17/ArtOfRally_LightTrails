using UnityEngine;
using UnityModManagerNet;

using static UnityModManagerNet.UnityModManager;

// TODO : Add settings here
// trail width
// initial alpha ?
// show on brake only / show all the time

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
        [Draw(DrawType.Slider, Min = 1, Max = 5)]
        public int trailSmoothness = 3;
        [Draw(DrawType.Slider, Min = 1, Max = 10)]
        public float trailMaxLength = 5;

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
