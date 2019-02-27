using UnityEngine;
using System.Collections.Generic;
using Assets.GraphicsUtil.Shapes;

namespace Assets
{
    public static class Shapes
    {
        static Material mainMat;

        public static void Init(Shader mainShader)
        {
            mainMat = new Material(mainShader);
        }

        public static GameObject DrawDottedLine(Vector3 pt0, Vector3 pt1, Color passColor, float passLW)
        {
            GameObject line;
            line = new GameObject("Line");
            float length = Vector3.Distance(pt0, pt1);
            int totalDot = Mathf.FloorToInt(length * .15f);
            Polygon dot;
            for (int ii = 0; ii < totalDot; ii++)
            {
                dot = Object.Instantiate(NewCylinder.rootDot);
                dot.transform.SetParent(line.transform, false);
                dot.transform.localScale = Vector3.one * passLW;
                float mag = -ii * length / totalDot;
                if (pt1.x > 0)
                    dot.transform.localPosition = pt0 + new Vector3(mag - .5f, 0, 0);
                else
                    dot.transform.localPosition = pt0 + new Vector3(0, 0, mag - .5f);
            }

            return line;
        }

        public static GameObject DrawTri(float h, float b, Color passColor)
        {
            List<Vector3> skinList = new List<Vector3>
            {
                new Vector3(0, 0, h), 
                new Vector3(b * .5f, 0, 0),
                new Vector3(-b * .5f, 0, 0)
            };

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
