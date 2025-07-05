using System;
using System.Collections.Generic;
using UnityEngine;

namespace LightTrails
{
    /// <summary>Animates light trails depending on settings</summary>
    public class TrailAnimator : MonoBehaviour
    {
        private static ConditionTypes.Weather currentWeather = ConditionTypes.Weather.None;

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

        private void Update()
        {
            bool brakeInput = GameEntryPoint.EventManager.playerManager.carcontroller.brakeKey;

            if (brakeInput)
            {
                if (lastBrakeInput)
                {
                    currentLine.Update();
                    currentLine.PlaceLast();
                }
                else
                    SpawnTrail();
            }
            else if (lastBrakeInput)
            {
                //Main.Log("Released line");
                //currentLine.ReleaseLine(
                //    () =>
                //    {
                //        // TODO : Will this capture correctly ?
                //        TrackedLine line = currentLine;
                //        releasedLines.Remove(line);
                //    }
                //);
                //releasedLines.Add(currentLine);
                //currentLine = null;
            }

            // TODO : update released lines

            lastBrakeInput = brakeInput;
        }

        private void SpawnTrail()
        {
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

            LineRenderer line = Instantiate(Main.trailPrefab, transform).GetComponent<LineRenderer>();
            currentLine = new TrackedLine(line);
        }

        public void RefreshSettings()
        {
            currentLine.UpdateSettings();
            releasedLines.ForEach(line => line.UpdateSettings());
        }

        public class TrackedLine
        {
            private List<Vector3> points;
            private LineRenderer line;
            private Action OnDestroy;
            private Vector3 firstPointPos;
            private Vector3 lastPos;
            private float pointDistance;
            private float currentDistance;
            private float fadeSpeed;
            private int maxPointsCount;

            public TrackedLine(LineRenderer line)
            {
                this.line = line;

                lastPos = line.transform.position;
                points = new List<Vector3>();
                firstPointPos = lastPos;

                points.Add(lastPos);
                line.SetPositions(points.ToArray());

                UpdateSettings();
            }

            public void UpdateSettings()
            {
                maxPointsCount = Mathf.FloorToInt(Main.settings.trailSmoothness * Main.settings.trailMaxLength);
                pointDistance = 1f / Main.settings.trailSmoothness;
            }

            public void ReleaseLine(Action OnDestroy)
            {
                this.OnDestroy = OnDestroy;

                if (points.Count < 2)
                {
                    Destroy(line.gameObject);
                    OnDestroy();
                }
                else
                    fadeSpeed = Vector3.Distance(points[points.Count - 1], points[points.Count - 2]) / Time.deltaTime;
            }

            public void PlaceLast() => points[points.Count - 1] = line.transform.position;

            public void Update()
            {
                float distance = Vector3.Distance(line.transform.position, lastPos);

                if (fadeSpeed == 0)
                {
                    currentDistance += distance;

                    if (currentDistance >= pointDistance)
                    {
                        currentDistance = currentDistance % pointDistance;
                        points.Add(line.transform.position);

                        if (points.Count == 1)
                            firstPointPos = points[0];
                    }
                }
                else
                {
                    currentDistance += fadeSpeed * Time.deltaTime;

                    if (currentDistance >= pointDistance)
                    {
                        currentDistance = currentDistance % pointDistance;
                        maxPointsCount--;
                    }
                }

                if (maxPointsCount == 1)
                {
                    Destroy(line.gameObject);
                    OnDestroy();
                    return;
                }

                if (points.Count > maxPointsCount)
                    points.RemoveAt(0);

                if (points.Count == maxPointsCount)
                    points[0] = Vector3.Lerp(firstPointPos, points[1], currentDistance / pointDistance);

                line.positionCount = points.Count;
                line.SetPositions(points.ToArray());
                lastPos = line.transform.position;
            }
        }
    }
}
