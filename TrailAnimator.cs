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

        // TODO : Add trail with settings
        // TODO : Add trail alpha settings
        // TODO : Fix trails joining in full mode
        // TODO : Fix trails on retry 

        private void Awake()
        {
            releasedLines = new List<TrackedLine>();

            if (currentWeather == ConditionTypes.Weather.None)
            {
                RallyData data = GameModeManager.GetRallyDataCurrentGameMode();

                if (data != null)
                    currentWeather = data.GetCurrentStage().Weather;
            }

            if (!Main.settings.brakeTrailOnly)
                SpawnTrail(false);
        }

        private void Update()
        {
            Main.Try(() =>
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
                    {
                        if (!Main.settings.brakeTrailOnly)
                            ReleaseLine();

                        SpawnTrail(true);
                    }
                }
                else
                {
                    if (lastBrakeInput)
                    {
                        ReleaseLine();

                        if (!Main.settings.brakeTrailOnly)
                            SpawnTrail(false);
                    }

                    if (!Main.settings.brakeTrailOnly)
                    {
                        currentLine.Update();
                        currentLine.PlaceLast();
                    }
                }

                releasedLines.ForEach(line => line.Update());
                lastBrakeInput = brakeInput;
            });
        }

        private void SpawnTrail(bool asBrakes)
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
            line.widthMultiplier = Main.settings.trailWidth;

            Color lineColor = asBrakes ? Color.white : Color.black;
            lineColor.a = Main.settings.trailAlpha;
            line.material.color = lineColor;

            currentLine = new TrackedLine(line);
        }

        private void ReleaseLine()
        {
            TrackedLine line = currentLine;

            currentLine.ReleaseLine(() => releasedLines.Remove(line));
            releasedLines.Add(currentLine);
            currentLine = null;
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
                    fadeSpeed = Mathf.Max(0.1f, Vector3.Distance(points[points.Count - 1], points[points.Count - 2]) / Time.deltaTime);
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
                {
                    points.RemoveAt(0);
                    firstPointPos = points[1];
                }

                if (points.Count == maxPointsCount)
                    points[0] = Vector3.Lerp(firstPointPos, points[1], currentDistance / pointDistance);

                line.positionCount = points.Count;
                line.SetPositions(points.ToArray());
                lastPos = line.transform.position;
            }
        }
    }
}
