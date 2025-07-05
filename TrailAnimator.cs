using System;
using System.Collections.Generic;
using UnityEngine;

namespace LightTrails
{
    /// <summary>Animates light trails depending on settings</summary>
    public class TrailAnimator : MonoBehaviour
    {
        private static ConditionTypes.Weather currentWeather = ConditionTypes.Weather.None;

        public enum Weather
        {
            Morning,
            Afternoon,
            Sunset,
            Night,
            Fog,
            Rain,
            Snow
        }

        private List<TrackedLine> releasedLines;
        private TrackedLine currentLine;
        private bool lastBrakeInput;

        private void Awake()
        {
            releasedLines = new List<TrackedLine>();

            if (currentWeather == ConditionTypes.Weather.None)
            {
                RallyData data = GameModeManager.GetRallyDataCurrentGameMode();

                if (data != null)
                    currentWeather = data.GetCurrentStage().Weather;
            }
        }

        // detect when the car is braking
        //      spawn a new line render
        //      place points
        //          first point is the end of the line
        //      fade first point of line (move to second then remove a point from the start)
        //      track length of movement to spawn new points
        // detect when brake is released
        //      move this line renderer to "passive" list
        //      keep fade first point of line
        //          when all the points are faded we can destroy the line

        // TODO : Decide fade duration for a line depending of the current speed of the source when we release the brakes

        private void Update()
        {
            bool brakeInput = GameEntryPoint.EventManager.playerManager.carcontroller.brakeKey;

            if (brakeInput)
            {
                if (lastBrakeInput)
                {
                    // TODO : update the current line + track length
                }
                else
                    SpawnTrail();
            }
            else if (lastBrakeInput)
                releasedLines.Add(currentLine);

            // TODO : update released lines

            lastBrakeInput = brakeInput;
        }

        private void SpawnTrail()
        {
            // TODO : Finish this

            bool shouldSpawn = false;

            switch (currentWeather)
            {
                case ConditionTypes.Weather.None:
                    shouldSpawn = true;
                    break;

                case ConditionTypes.Weather.Morning:
                    shouldSpawn = Main.settings.showMorning;
                    break;

                case ConditionTypes.Weather.Afternoon:
                    shouldSpawn = Main.settings.showAfternoon;
                    break;

                case ConditionTypes.Weather.Sunset:
                    shouldSpawn = Main.settings.showSunset;
                    break;

                case ConditionTypes.Weather.Night:
                    shouldSpawn = Main.settings.showNight;
                    break;

                case ConditionTypes.Weather.Fog:
                    shouldSpawn = Main.settings.showFog;
                    break;

                case ConditionTypes.Weather.Rain:
                    shouldSpawn = Main.settings.showRain;
                    break;

                case ConditionTypes.Weather.Snow:
                    shouldSpawn = Main.settings.showSnow;
                    break;
            }

            if (!shouldSpawn)
                return;

            LineRenderer line = Instantiate(Main.trailPrefab).GetComponent<LineRenderer>();
            currentLine = new TrackedLine(line);
        }

        private Weather ConvertWeather(ConditionTypes.Weather weather) => (Weather)Enum.Parse(typeof(Weather), weather.ToString());

        public class TrackedLine
        {
            public LineRenderer line;

            private List<Vector3> points;
            private float currentDistance;
            private float fadeSpeed;

            public TrackedLine(LineRenderer line)
            {
                this.line = line;

                Vector3[] extractedPoints = new Vector3[line.positionCount];
                line.GetPositions(extractedPoints);
                points = new List<Vector3>(extractedPoints);
            }

            public void SetFadeSpeed(float fadeSpeed) => this.fadeSpeed = fadeSpeed;

            public void PlaceLast(Vector3 position)
            {
                points[points.Count - 1] = position;
            }

            public void Update(float distance)
            {
                // TODO : Finish this

                currentDistance += distance;

                //if (currentDistance < Main.settings.)
            }
        }
    }
}
