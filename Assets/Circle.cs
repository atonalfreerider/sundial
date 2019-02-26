using System.Collections.Generic;
using UnityEngine;

namespace Assets.GraphicsUtil.Shapes
{
    public class Circle : Polygon
    {
        // the amount of straight lines per 1 meter of an arc; 
        public int sidePer1M = 16;

        // the current radius and percent completion of a circle or ring;
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

            // calculate the number of sides to this circle;
            // there is one less side because a full circle fills the last side;
            int side = Mathf.RoundToInt(Mathf.Pow(R + 1f, .8f) * sidePer1M * prct);
            if (side <= 0)
                return;

            // if an incomplete circle, draw one more side;
            if (prct < 1f)
                side += 1;

            // generate vertices and indices;
            Vector3[] skinList = new Vector3[side + 1];
            int[] indList = new int[side * 3];

            skinList[0] = Vector3.zero;
            float alpha = 360f / (side - (prct < 1f ? 1 : 0)); // a positive alpha is drawn clockwise;

            int count = 1;
            int[] insInd;
            for (int ii = side - 1; ii >= 0; ii--)
            {
                skinList[count] = VectArc(R, ii * alpha, prct, offset, 0f, 0f);
                insInd = new[] {0, count, count + 1};
                insInd.CopyTo(indList, (count - 1) * 3);
                count++;
            }

            indList[indList.Length - 1] = 1;

            if (h < float.Epsilon)
            {
                // a circle with 0 depth - only draw two sides;
                Draw3DPoly(skinList, MirrorIndices(indList, 0));
            }
            else
            {
                Extrude(skinList, indList, h, true, true, 0f);
            }
        }

        public void DrawRing(float R1, float R2, float prct, float h)
        {
            curR = (R1 + R2) * .5f;
            prct = Mathf.Abs(prct);
            curPrct = prct;

            // calculate the number of sides to this ring;
            // there is one less side because a full ring fills the last side;
            int side = Mathf.RoundToInt(Mathf.Pow(R1 + 1f, .8f) * sidePer1M * prct);
            if (side <= 0)
                return;

            // if an incomplete ring, draw one more side;
            if (prct < 1f)
                side += 1;

            // generate vertices and indices;
            Vector3[] skinList = new Vector3[side * 2];
            int[] indList = new int[side * 6];
            int[] insInd;
            float alpha = 360f / (side - (prct < 1f ? 1 : 0)); // a positive alpha is drawn clockwise;

            for (int ii = 0; ii < side; ii++)
            {
                skinList[ii] = VectArc(R1, ii * alpha, prct, 0f, 0f, 0f);
                skinList[side * 2 - ii - 1] = VectArc(R2, ii * alpha, prct, 0f, 0f, 0f);

                insInd = new[] {ii, side * 2 - ii - 2, side * 2 - ii - 1};
                insInd.CopyTo(indList, ii * 6);

                insInd = new[] {ii, ii + 1, side * 2 - ii - 2};
                insInd.CopyTo(indList, ii * 6 + 3);
            }

            if (prct >= 1f - float.Epsilon)
            {
                // if a complete ring - connect the last segment of the ring to the beginning segment;
                indList[indList.Length - 1] = side * 2 - 1;
                indList[indList.Length - 2] = 0;

                indList[indList.Length - 5] = side * 2 - 1;
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

        static Vector3 VectArc(float R, float alpha, float prct, float offSet, float cX, float cZ)
        {
            return new Vector3(
                R * Mathf.Sin(
                    alpha * prct * Mathf.PI / 180f +
                    offSet * 2f * Mathf.PI) +
                cX,
                0f,
                R * Mathf.Cos(
                    alpha * prct * Mathf.PI / 180f +
                    offSet * 2f * Mathf.PI) +
                cZ
            );
        }

        static object[] SprockTick(float R, float H, float alpha, float aDelt, float sprockTh, float baseAl,
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
                pointList.Add(new Vector3(R * Mathf.Sin(tempA), 0f, R * Mathf.Cos(tempA)));
                pointList.Add(new Vector3((R - sprockTh) * Mathf.Sin(tempA), 0f, (R - sprockTh) * Mathf.Cos(tempA)));
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

            object[] retArr = {pointList, indList, counter};
            return retArr;
        }

        public void DrawSprocket()
        {
            const float earthR = .15f;

            // create point cloud for earth sprocket mesh;
            List<Vector3> pointList = new List<Vector3>();
            List<int> indList = new List<int>();
            object[] retArr;
            int pointCounter = 0;
            int counter = 0;
            float alpha;
            const float sprockTh = .003f;
            const float baseAl = .02f;
            const float pointAl = .01f;
            const float smallH = .007f;
            const float bigH = .015f;
            const float hR = earthR * .4f;
            const int segments = 8;
            const int subSegments = 4;
            const float stepD = 360f / (segments * subSegments) * Mathf.PI / 180f;

            for (int tt = 0; tt < segments; tt++)
            {
                alpha = -counter * stepD;
                // create hour tick;

                retArr = SprockTick(hR, bigH, alpha, -(counter + 1) * stepD, sprockTh, baseAl, pointAl, pointCounter,
                    0f);
                pointList.AddRange((List<Vector3>) retArr[0]);
                indList.AddRange((List<int>) retArr[1]);
                pointCounter = (int) retArr[2];
                counter++;
                for (int dd = 0; dd < subSegments - 1; dd++)
                {
                    alpha = -counter * stepD;
                    // create 15min tick;
                    retArr = SprockTick(hR, smallH, alpha, -(counter + 1) * stepD, sprockTh, baseAl, pointAl,
                        pointCounter, 0f);
                    pointList.AddRange((List<Vector3>) retArr[0]);
                    indList.AddRange((List<int>) retArr[1]);
                    pointCounter = (int) retArr[2];
                    counter++;
                }
            }

            indList.RemoveRange(indList.Count - 6, 6);

            Draw3DPoly(pointList.ToArray(), MirrorIndices(indList.ToArray(), 0));
            name = "Sprocket";
        }
        
    }

    public static class NewCylinder
    {
        public static Circle cylinder;

        public static Circle paren;

        public static Circle methodRing;
        public static Circle rootRing;
        public static Circle rootDot;

        public static Circle referenceDepthIndicator;

        public static void Init(PolygonFactory polygonFactory, Material mainMat)
        {
            Color shapeColor = Color.white;
            
            cylinder = PolygonFactory.NewCirclePoly(mainMat, false);
            cylinder.sidePer1M = 8;
            cylinder.DrawRing(1, .85f, 1, .3f);
            cylinder.name = "cylinder";
            cylinder.SetColor(shapeColor);
            cylinder.transform.SetParent(polygonFactory.transform, false);

            paren = PolygonFactory.NewCirclePoly(mainMat, false);
            paren.DrawRing(.015f, .0135f, .3f, 0);
            paren.name = "paren";
            paren.transform.SetParent(polygonFactory.transform, false);

            rootDot = PolygonFactory.NewCirclePoly(mainMat, false);
            rootDot.name = "RootDot";
            rootDot.DrawCirc(.035f * .5f, 1, 0);
            rootDot.SetColor(shapeColor);
            rootDot.transform.SetParent(polygonFactory.transform, false);
            

        }
    }
}