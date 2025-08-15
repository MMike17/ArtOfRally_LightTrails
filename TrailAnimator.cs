using System.Collections.Generic;
using UnityEngine;

namespace LightTrails
{
    /// <summary>Animates light trails depending on settings</summary>
    public class TrailAnimator : MonoBehaviour
    {
        const float MIN_FADE_SPEED = 5;

        private static ConditionTypes.Weather currentWeather = ConditionTypes.Weather.None;

        private List<TrackedLine> releasedLines;
        private TrackedLine currentLine;
        private CarController controller;
        private bool lastBrakeInput;

        private void Awake()
        {
            releasedLines = new List<TrackedLine>();
            controller = GameEntryPoint.EventManager.playerManager.carcontroller;
            RallyData data = GameModeManager.GetRallyDataCurrentGameMode();

            if (data != null)
                currentWeather = data.GetCurrentStage().Weather;

            SpawnTrail(false);
        }

        private void Update()
        {
            Main.Try("Animator update", () =>
            {
                if (ReplayManager.Instance() == null || ReplayManager.Instance().CurrentState == ReplayManager.ReplayState.Playback)
                    return;

                if (currentLine == null || controller == null || releasedLines == null)
                    Awake();

                bool brakeInput = controller.brakeKey;

                if (brakeInput != lastBrakeInput)
                {
                    ReleaseLine();
                    SpawnTrail(brakeInput);
                }

                currentLine.PlaceLast();
                currentLine.Update();
                currentLine.CheckLineColor(brakeInput);

                List<TrackedLine> toRemove = new List<TrackedLine>();
                releasedLines.ForEach(line =>
                {
                    if (line.Update())
                        toRemove.Add(line);
                });
                toRemove.ForEach(line => releasedLines.Remove(line));

                lastBrakeInput = brakeInput;
            });
        }

        private void SpawnTrail(bool asBrakes)
        {
            LineRenderer line = Instantiate(Main.trailPrefab, transform).GetComponent<LineRenderer>();
            line.widthMultiplier = Main.settings.trailWidth;

            currentLine = new TrackedLine(line, asBrakes);
            bool shouldDisplay = false;

            switch (currentWeather)
            {
                case ConditionTypes.Weather.None:
                    shouldDisplay = true;
                    break;

                case ConditionTypes.Weather.Morning:
                    shouldDisplay = Main.settings.showMorning;
                    break;

                case ConditionTypes.Weather.Afternoon:
                    shouldDisplay = Main.settings.showAfternoon;
                    break;

                case ConditionTypes.Weather.Sunset:
                    shouldDisplay = Main.settings.showSunset;
                    break;

                case ConditionTypes.Weather.Night:
                    shouldDisplay = Main.settings.showNight;
                    break;

                case ConditionTypes.Weather.Fog:
                    shouldDisplay = Main.settings.showFog;
                    break;

                case ConditionTypes.Weather.Rain:
                    shouldDisplay = Main.settings.showRain;
                    break;

                case ConditionTypes.Weather.Snow:
                    shouldDisplay = Main.settings.showSnow;
                    break;
            }

            if (Main.settings.brakeTrailOnly)
                shouldDisplay &= asBrakes;

            currentLine.SetVisibility(Main.enabled && shouldDisplay && ReplayManager.Instance().CurrentState != ReplayManager.ReplayState.Playback);
        }

        private void ReleaseLine()
        {
            currentLine.ReleaseLine();
            releasedLines.Add(currentLine);
            currentLine = null;
        }

        public void SetVisibility(bool value)
        {
            currentLine.SetVisibility(value);
            releasedLines.ForEach(line => line.SetVisibility(value));
        }

        public void RefreshSettings()
        {
            currentLine.UpdateSettings();
            releasedLines.ForEach(line => line.UpdateSettings());
        }

        public void ResetTrails()
        {
            currentLine.Destroy();

            releasedLines.ForEach(item => item.Destroy());
            releasedLines.Clear();

            Awake();
        }

        public class TrackedLine
        {
            private List<Vector3> points;
            private LineRenderer line;
            private Vector3 firstPointPos;
            private Vector3 lastPos;
            private float pointDistance;
            private float currentDistance;
            private float fadeSpeed;
            private int maxPointsCount;

            public TrackedLine(LineRenderer line, bool asBrakes)
            {
                this.line = line;

                lastPos = line.transform.position;
                points = new List<Vector3>();
                firstPointPos = lastPos;

                points.Add(lastPos);
                line.SetPositions(points.ToArray());

                UpdateSettings();
                SetColor(asBrakes);
            }

            // this is only for bug fixing
            private void SetColor(bool asBrakes)
            {
                if (line == null)
                    return;

                Color lineColor = asBrakes ? Color.white : Color.black;
                lineColor.a = Main.settings.trailAlpha;
                line.material.color = lineColor;
            }

            public void UpdateSettings()
            {
                maxPointsCount = Mathf.FloorToInt(Main.settings.trailSmoothness * Main.settings.trailMaxLength);
                pointDistance = 1f / Main.settings.trailSmoothness;
            }

            public void ReleaseLine()
            {
                if (line == null)
                    return;

                if (points.Count < 2)
                    Destroy();
                else
                    fadeSpeed = Mathf.Max(MIN_FADE_SPEED, Vector3.Distance(points[points.Count - 1], points[points.Count - 2]) / Time.deltaTime);
            }

            public void SetVisibility(bool value)
            {
                if (line != null)
                    line.enabled = value;
            }

            public void PlaceLast()
            {
                if (line != null)
                    points[points.Count - 1] = line.transform.position;
            }

            public bool Update()
            {
                if (line == null)
                    return true;

                float distance = Vector3.Distance(line.transform.position, lastPos);

                if (fadeSpeed == 0)
                {
                    currentDistance += distance;

                    if (currentDistance >= pointDistance)
                    {
                        currentDistance %= pointDistance;
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
                    Destroy();
                    return true;
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

                return false;
            }

            // this is only intended to be called on the current line
            public void CheckLineColor(bool brakeInput)
            {
                if (line == null)
                    return;

                Color targetColor = brakeInput ? Color.white : Color.black;
                Color currentColor = line.material.color;
                currentColor.a = 1;

                if (currentColor != targetColor)
                    SetColor(brakeInput);
            }

            public void Destroy()
            {
                if (line != null)
                    GameObject.Destroy(line.gameObject);
            }
        }
    }
}
