using System.Collections.Generic;
using UnityEngine;

namespace LightTrails
{
    /// <summary>Animates light trails depending on settings</summary>
    public class TrailAnimator : MonoBehaviour
    {
        private List<TrackedLine> releasedLines;
        private TrackedLine currentLine;
        private bool lastBrakeInput;

        private void Awake()
        {
            releasedLines = new List<TrackedLine>();
        }

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
                {
                    // TODO : spawn line
                }
            }
            else if (lastBrakeInput)
                releasedLines.Add(currentLine);

            // TODO : update released lines

            lastBrakeInput = brakeInput;
        }

        private void SpawnTrail()
        {
            // TODO : Finish this
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

        public class TrackedLine
        {
            private List<Vector3> points;
            public TrailRenderer line;
            public float fadeSpeed;

            public TrackedLine(TrailRenderer line, float fadeSpeed)
            {
                this.line = line;
                this.fadeSpeed = fadeSpeed;

                Vector3[] extractedPoints = new Vector3[line.positionCount];
                line.GetPositions(extractedPoints);
                points = new List<Vector3>(extractedPoints);
            }

            public void PlaceLast()
            {
                // TODO : Finish this
            }

            public void Update(float distance, bool updateLast)
            {
                // TODO : Finish this
            }
        }
    }
}
