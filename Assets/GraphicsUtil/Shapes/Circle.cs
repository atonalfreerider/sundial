using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.GraphicsUtil.Shapes
{
    public class Circle : Polygon
    {
        // the amount of straight lines per 1 meter of an arc
        public int sidePer1M = 16;

        // the current radius and percent completion of a circle or ring
        public float curPrct;

        public float curR;

        public void Draw(float radius)
        {
            // Draws a full circle (100%) with no height (not a cylinder)
            DrawCirc(radius, 1, 0);
        }

        public void DrawCirc(float R, float prct, float h, float offset = 0f)
        {
            curPrct = prct;
            curR = R;

            // calculate the number of sides to this circle
            // there is one less side because a full circle fills the last side
            int side = Mathf.RoundToInt(Mathf.Pow(R + 1f, .8f) * sidePer1M * prct);

            if (side <= 0) return;

            // if an incomplete circle, draw one more side
            if (prct < 1f)
                side += 1;

            // generate vertices and indices
            Vector3[] skinList = new Vector3[side + 1];
            int[] indList = new int[side * 3];

            skinList[0] = Vector3.zero;
            float alpha = 360f / (side - (prct < 1 ? 1 : 0)); // a positive alpha is drawn clockwise

            int count = 1;
            for (int ii = side - 1; ii >= 0; ii--)
            {
                skinList[count] = VectArc(R, ii * alpha, prct, offset, 0, 0, 0);
                int[] insInd = { 0, count, count + 1 };
                insInd.CopyTo(indList, (count - 1) * 3);
                count++;
            }

            indList[^1] = 1;

            if (h <= float.Epsilon)
            {
                // a circle with 0 depth - only draw two sides
                Draw3DPoly(skinList, MirrorIndices(indList, 0));
            }
            else
            {
                Extrude(skinList, indList, h, true, true, 0);
            }
        }

        public void DrawRing(float R1, float R2, float prct, float h, float spiralH = 0, bool diminishTail = false)
        {
            curR = (R1 + R2) * .5f;
            prct = Mathf.Abs(prct);
            curPrct = prct;
            float spirBit = 0;
            float diminishingR = 0;

            // calculate the number of sides to this ring;
            // there is one less side because a full ring fills the last side;
            int side = Mathf.RoundToInt(Mathf.Pow(R1 + 1f, .8f) * sidePer1M * prct);
            if (side <= 0)
                return;

            // if an incomplete ring, draw one more side;
            if (prct < 1f)
            {
                side += 1;
            }

            // generate vertices and indices;
            Vector3[] skinList = new Vector3[side * 2];
            int[] indList = new int[side * 6];
            int[] insInd;
            float alpha = 360f / (side - (prct < 1f ? 1 : 0)); // a positive alpha is drawn clockwise;

            for (int ii = 0; ii < side; ii++)
            {
                spirBit = -ii * spiralH / side;
                if (diminishTail)
                {
                    diminishingR = Mathf.Lerp(0, R1 - R2, (float)ii / side);
                }

                skinList[ii] = VectArc(R1 - diminishingR, ii * alpha, prct, 0, 0, spirBit, 0);
                skinList[side * 2 - ii - 1] = VectArc(R2, ii * alpha, prct, 0, 0, spirBit, 0);

                insInd = new[] { ii, side * 2 - ii - 2, side * 2 - ii - 1 };
                insInd.CopyTo(indList, ii * 6);

                insInd = new[] { ii, ii + 1, side * 2 - ii - 2 };
                insInd.CopyTo(indList, ii * 6 + 3);
            }

            if (prct >= 1f - float.Epsilon)
            {
                // if a complete ring - connect the last segment of the ring to the beginning segment;
                indList[^1] = side * 2 - 1;
                indList[^2] = 0;

                indList[^5] = side * 2 - 1;
            }
            else if (indList.Length > 6)
            {
                // remove the last segment of the incomplete ring;
                int[] truncList = new int[indList.Length - 3];
                for (int zz = 0; zz < truncList.Length; zz++)
                    truncList[zz] = indList[zz];

                indList = truncList;
            }

            if (h < float.Epsilon)
            {
                // a ring with 0 depth - only draw two sides;
                Draw3DPoly(skinList, MirrorIndices(indList, 0));
            }
            else
            {
                Extrude(skinList, indList, h, false, true, 0f);
            }
        }

        static Vector3 VectArc(float R, float alpha, float prct, float offSet, float cX, float cY, float cZ)
        {
            return new Vector3(
                R * Mathf.Sin(
                    alpha * prct * Mathf.PI / 180f +
                    offSet * 2f * Mathf.PI) +
                cX,
                cY,
                R * Mathf.Cos(
                    alpha * prct * Mathf.PI / 180f +
                    offSet * 2f * Mathf.PI) +
                cZ
            );
        }

        static SprocketTick SprockTick(float R, float H, float alpha, float aDelt, float sprockTh, float baseAl,
            float pointAl, int counter, float spiralH)
        {
            List<Vector3> pointList = new List<Vector3>();
            List<int> indList = new List<int>();

            pointList.Add(new Vector3(R * Mathf.Sin(alpha + baseAl), spiralH, R * Mathf.Cos(alpha + baseAl)));
            pointList.Add(new Vector3((R - sprockTh) * Mathf.Sin(alpha + baseAl), spiralH,
                (R - sprockTh) * Mathf.Cos(alpha + baseAl)));

            pointList.Add(new Vector3((R - H) * Mathf.Sin(alpha + pointAl), spiralH,
                (R - H) * Mathf.Cos(alpha + pointAl)));
            pointList.Add(new Vector3((R - H) * Mathf.Sin(alpha - pointAl), spiralH,
                (R - H) * Mathf.Cos(alpha - pointAl)));

            pointList.Add(new Vector3(R * Mathf.Sin(alpha - baseAl), spiralH, R * Mathf.Cos(alpha - baseAl)));
            pointList.Add(new Vector3((R - sprockTh) * Mathf.Sin(alpha - baseAl), spiralH,
                (R - sprockTh) * Mathf.Cos(alpha - baseAl)));

            indList.Add(counter + 0);
            indList.Add(counter + 1);
            indList.Add(counter + 4);

            indList.Add(counter + 1);
            indList.Add(counter + 5);
            indList.Add(counter + 4);

            indList.Add(counter + 1);
            indList.Add(counter + 2);
            indList.Add(counter + 5);

            indList.Add(counter + 2);
            indList.Add(counter + 3);
            indList.Add(counter + 5);

            counter += 6;
            // in-fill;       
            float tempA = alpha - .03f;
            while (tempA > aDelt + .03f)
            {
                pointList.Add(new Vector3(R * Mathf.Sin(tempA), 0, R * Mathf.Cos(tempA)));
                pointList.Add(new Vector3((R - sprockTh) * Mathf.Sin(tempA), 0, (R - sprockTh) * Mathf.Cos(tempA)));
                indList.Add(counter - 2);
                indList.Add(counter - 1);
                indList.Add(counter + 0);

                indList.Add(counter - 1);
                indList.Add(counter + 1);
                indList.Add(counter + 0);

                counter += 2;
                tempA -= .03f;
            }

            indList.Add(counter - 2);
            indList.Add(counter - 1);
            indList.Add(counter + 0);

            indList.Add(counter - 1);
            indList.Add(counter + 1);
            indList.Add(counter + 0);

            return new SprocketTick(pointList, indList, counter);
        }

        struct SprocketTick
        {
            public readonly List<Vector3> pointList;
            public readonly List<int> indexList;
            public readonly int pointCounter;

            public SprocketTick(List<Vector3> pointList, List<int> indexList, int pointCounter)
            {
                this.pointList = pointList;
                this.indexList = indexList;
                this.pointCounter = pointCounter;
            }
        }

        public void DrawSprocket(float R, int majorTickCount, int minorTickCount, int innie,
            float sprockTh, float baseAl, float pointAl, float smallH, float bigH,
            float specialInterval = 0, int endSnip = 6)
        {
            // create point cloud for earth sprocket mesh;
            List<Vector3> pointList = new List<Vector3>();
            List<int> indList = new List<int>();
            SprocketTick sprocketTick;
            int pointCounter = 0;
            int counter = 0;
            float alpha;
            float stepDivisor = specialInterval > float.Epsilon
                ? specialInterval
                : majorTickCount * (minorTickCount + 1);
            float stepDistance = 360f / stepDivisor * Mathf.PI / 180f;

            for (int majorTicks = 0; majorTicks < majorTickCount; majorTicks++)
            {
                alpha = -counter * stepDistance;
                sprocketTick = SprockTick(R,
                    innie * bigH,
                    alpha,
                    -(counter + 1) * stepDistance,
                    innie * sprockTh,
                    baseAl,
                    pointAl,
                    pointCounter,
                    0);
                pointList.AddRange(sprocketTick.pointList);
                indList.AddRange(sprocketTick.indexList);
                pointCounter = sprocketTick.pointCounter;
                counter++;

                for (int minorTicks = 0; minorTicks < minorTickCount; minorTicks++)
                {
                    alpha = -counter * stepDistance;
                    sprocketTick = SprockTick(R,
                        innie * smallH,
                        alpha,
                        -(counter + 1) * stepDistance,
                        innie * sprockTh,
                        baseAl,
                        pointAl,
                        pointCounter,
                        0);
                    pointList.AddRange(sprocketTick.pointList);
                    indList.AddRange(sprocketTick.indexList);
                    pointCounter = sprocketTick.pointCounter;
                    counter++;
                }
            }

            indList.RemoveRange(indList.Count - endSnip, endSnip);

            Draw3DPoly(pointList.ToArray(), MirrorIndices(indList.ToArray(), 0));
            name = "Sprocket";
        }

        public void DrawSunSprocket(float R, DateTime passDate,
            float sprockTh, float baseAl, float pointAl, float smallH, float medH, float bigH,
            float specialInterval, int endSnip = 6)
        {
            int[] daysInMonth = { 31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31 };
            if (DateTime.IsLeapYear(passDate.Year))
            {
                // add one day to February
                daysInMonth[1]++;
            }

            // determine start point of first day of year
            DateTime jan1OfDate = new(passDate.Year, 1, 1);
            int dayCounter = Calendar.ConvertDaytoInt(jan1OfDate.DayOfWeek.ToString());

            // create point cloud for sprocket mesh
            List<Vector3> pointList = new List<Vector3>();
            List<int> indList = new List<int>();
            SprocketTick retArr;
            int counter = 0;
            int pointCounter = 0;
            float alpha;
            float step = 360f / specialInterval * Mathf.PI / 180f;

            foreach (int diM in daysInMonth)
            {
                alpha = -counter * step;
                if (dayCounter > 6)
                {
                    // set back to Monday
                    dayCounter = 0;
                }

                // add first-of-month tick
                retArr = SprockTick(
                    R,
                    bigH,
                    alpha,
                    -(counter + 1) * step,
                    sprockTh,
                    baseAl,
                    pointAl,
                    pointCounter,
                    0);
                pointList.AddRange(retArr.pointList);
                indList.AddRange(retArr.indexList);
                pointCounter = retArr.pointCounter;
                counter++;
                dayCounter++;
                for (int dd = 0; dd < diM - 1; dd++)
                {
                    alpha = -counter * step;
                    if (dayCounter == 7)
                    {
                        // add Sunday tick
                        retArr = SprockTick(
                            R,
                            medH,
                            alpha,
                            -(counter + 1) * step,
                            sprockTh,
                            baseAl,
                            pointAl,
                            pointCounter,
                            0);
                        dayCounter = 0;
                    }
                    else
                    {
                        // add day tick
                        retArr = SprockTick(
                            R,
                            smallH,
                            alpha,
                            -(counter + 1) * step,
                            sprockTh,
                            baseAl,
                            pointAl,
                            pointCounter,
                            0);
                    }

                    pointList.AddRange(retArr.pointList);
                    indList.AddRange(retArr.indexList);
                    pointCounter = retArr.pointCounter;
                    counter++;
                    dayCounter++;
                }
            }

            indList.RemoveRange(indList.Count - endSnip, endSnip);
            Draw3DPoly(pointList.ToArray(), indList.ToArray());
        }
        
        public static class NewCylinder
        {
            public static Circle cylinder;
            public static Circle circle;

            public static void Init(PolygonFactory polygonFactory)
            {
                Color shapeColor = Color.white;

                cylinder = PolygonFactory.NewCirclePoly(polygonFactory.mainMat);
                cylinder.sidePer1M = 8;
                cylinder.DrawRing(1, .85f, 1, .3f);
                cylinder.name = "cylinder";
                cylinder.SetColor(shapeColor);
                cylinder.transform.SetParent(polygonFactory.transform, false);
                cylinder.gameObject.SetActive(false);

                circle = PolygonFactory.NewCirclePoly(polygonFactory.mainMat);
                circle.name = "RootDot";
                circle.DrawCirc(.035f * .5f, 1, 0);
                circle.SetColor(shapeColor);
                circle.transform.SetParent(polygonFactory.transform, false);
                circle.gameObject.SetActive(false);
            }
        }
    }
}