using UnityEngine;
using System.Collections.Generic;

namespace Assets
{
    public static class Shapes
    {
        static Material mainMat;

        public static void Init(Shader mainShade)
        {
            mainMat = new Material(mainShade);
        }

        public static object[] SprockTick(float R, float H, float alpha, float aDelt, float sprockTh, float baseAl,
            float pointAl, int counter, float spiralH)
        {
            List<Vector3> pointList = new List<Vector3>();
            List<int> indList = new List<int>();

            pointList.Add(new Vector3((R) * Mathf.Sin(alpha + baseAl), spiralH, (R) * Mathf.Cos(alpha + baseAl)));
            pointList.Add(new Vector3((R - sprockTh) * Mathf.Sin(alpha + baseAl), spiralH,
                (R - sprockTh) * Mathf.Cos(alpha + baseAl)));

            pointList.Add(new Vector3((R - H) * Mathf.Sin(alpha + pointAl), spiralH,
                (R - H) * Mathf.Cos(alpha + pointAl)));
            pointList.Add(new Vector3((R - H) * Mathf.Sin(alpha - pointAl), spiralH,
                (R - H) * Mathf.Cos(alpha - pointAl)));

            pointList.Add(new Vector3((R) * Mathf.Sin(alpha - baseAl), spiralH, (R) * Mathf.Cos(alpha - baseAl)));
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

            object[] retArr = new object[3] {pointList, indList, counter};
            return retArr;
        }

        public static GameObject DrawLine(string type, Vector3 pt0, Vector3 pt1, Color passColor, float passLW,
            float passLD)
        {
            GameObject line;
            float D = Vector3.Distance(pt0, pt1);
            if (type == "flat")
            {
                line = GameObject.CreatePrimitive(PrimitiveType.Quad);
                line.transform.localPosition = Vector3.Lerp(pt0, pt1, .5f);
                line.transform.LookAt(pt0);
                line.transform.localScale = new Vector3(D, passLW, 1f);
                line.transform.Rotate(Vector3.up, 90f);
                line.transform.Rotate(Vector3.right, 90f);

                Material Outline = mainMat;
                line.transform.GetComponent<Renderer>().material = Outline;
                line.transform.GetComponent<Renderer>().material.color = passColor;
            }
            else if (type == "dotted")
            {
                line = new GameObject();
                float length = Vector3.Distance(pt0, pt1);
                int totalDot = Mathf.FloorToInt(length * .15f);
                GameObject dot;
                for (int ii = 0; ii < totalDot; ii++)
                {
                    dot = DrawCirc(passLW, 1f, passColor);
                    dot.transform.parent = line.transform;
                    float mag = -ii * length / totalDot;
                    if (pt1.x > 0f)
                        dot.transform.localPosition = pt0 + new Vector3(mag - .5f, 0f, 0f);
                    else
                        dot.transform.localPosition = pt0 + new Vector3(0f, 0f, mag - .5f);
                }
            }
            else
                line = new GameObject();

            line.name = "Line";
            return line;
        }

        static Vector3 VectArc(float R, float alpha, float prct, float offSet, float cX, float cY, float cZ)
        {
            return new Vector3(R * Mathf.Sin(alpha * prct * Mathf.PI / 180f + offSet * Mathf.PI / 180f) + cX, cY,
                R * Mathf.Cos(alpha * prct * Mathf.PI / 180f + offSet * Mathf.PI / 180f) + cZ);
        }

        static GameObject DrawCirc(float R, float prct, Color passColor)
        {
            List<Vector3> skinList = new List<Vector3>();
            List<int> indList = new List<int>();

            skinList.Add(Vector3.zero);
            int side = (int) (18 * prct);
            int count = 1;
            for (int ii = side - 1; ii >= 0; ii--)
            {
                skinList.Add(VectArc(R, ii * 360f / side, prct, 0f, 0f, 0f, 0f));
                indList.Add(0);
                indList.Add(count + 1);
                indList.Add(count);
                count++;
            }

            indList[indList.Count - 2] = 1;

            return CreatePoly(skinList, indList, passColor);
        }

        public static GameObject DrawRing(float R1, float R2, float prct, Color passColor, float spiralH, bool dim)
        {
            List<Vector3> skinList = new List<Vector3>();
            List<int> indList = new List<int>();
            int side = (int) (36 * prct * R1 / 10f);
            int count = 0;
            float spirBit = 0;

            float dimR = 0f;

            for (int ii = side - 1; ii >= 0; ii--)
            {
                spirBit = -(ii) * spiralH / side;
                if (dim)
                {
                    dimR = Mathf.Lerp(0f, R1 - R2, (float) ii / side);
                }

                skinList.Add(VectArc(R1 - dimR, ii * 360f / side, prct, 0f, 0f, spirBit, 0f));
                skinList.Add(VectArc(R2, ii * 360f / side, prct, 0f, 0f, spirBit, 0f));
                indList.Add(count);
                indList.Add(count + 1);
                indList.Add(count + 2);

                indList.Add(count + 1);
                indList.Add(count + 3);
                indList.Add(count + 2);

                count += 2;
            }

            if (prct == 1f)
            {
                indList[indList.Count - 1] = 0;
                indList[indList.Count - 2] = 1;

                indList[indList.Count - 4] = 0;
            }
            else if (indList.Count > 6)
                indList.RemoveRange(indList.Count - 6, 6);

            GameObject chord = CreatePoly(skinList, indList, passColor);
            return chord;
        }

        public static GameObject DrawTri(float h, float b, Color passColor)
        {
            List<Vector3> skinList = new List<Vector3>();
            skinList.Add(new Vector3(0f, 0f, h));
            skinList.Add(new Vector3(b * .5f, 0f, 0f));
            skinList.Add(new Vector3(-b * .5f, 0f, 0f));

            List<int> indList = new List<int>() {0, 1, 2};

            GameObject tri = CreatePoly(skinList, indList, passColor);
            return tri;
        }

        public static GameObject CreatePoly(List<Vector3> vertex, List<int> indList, Color passColor)
        {
            GameObject newGO = new GameObject();

            if (vertex.Count < 3)
                return newGO;

            Vector3[] vertices = new Vector3[vertex.Count];
            int count = 0;
            foreach (Vector3 vec in vertex)
            {
                vertices[count] = new Vector3(vec.x, vec.y, vec.z);
                count++;
            }

            int[] indices = new int[indList.Count];
            count = 0;
            foreach (int ind in indList)
            {
                indices[count] = ind;
                count++;
            }

            // Use the triangulator to get indices for creating triangles
            // Triangulator tr = new Triangulator(vertices2D);
            //  int[] indices = tr.Triangulate();

            // Create the Vector3 vertices


            // Create the mesh
            Mesh msh = new Mesh();
            msh.vertices = vertices;
            msh.triangles = indices;
            msh.RecalculateNormals();
            msh.RecalculateBounds();

            // Set up game object with mesh;
            MeshFilter filter = newGO.AddComponent(typeof(MeshFilter)) as MeshFilter;
            filter.mesh = msh;
            newGO.AddComponent(typeof(MeshRenderer));
            newGO.transform.GetComponent<Renderer>().material = mainMat;
            newGO.transform.GetComponent<Renderer>().material.color = passColor;
            // add collider;
            // MeshCollider col = (MeshCollider)newGO.AddComponent(typeof(MeshCollider));
            // col.sharedMesh = msh;

            return newGO;
        }
    }
}
