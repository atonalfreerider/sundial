using System.Collections.Generic;
using System.Linq;
using Assets.GraphicsUtil.Shapes.Lines;
using UnityEngine;

namespace Assets.GraphicsUtil.Shapes
{
    public class PolygonFactory : MonoBehaviour
    {
        // regular polygons
        [HideInInspector]
        public Polygon tri, hex, tetra, icoSphere0, icoSphereCut0, dodecSphere, dodecSphereCut;
  
        // INIT
        public void BuildPolygons(Material mainMat)
        {
            NewCylinder.Init(this, mainMat);
            Color shapeColor = Color.white;
            
            // regular polygons
            tri = NewPoly(mainMat, true);
            tri.DrawRegPoly(1, 3, 0, 1, 0);
            tri.name = "triangle";
            tri.SetColor(shapeColor);
            tri.transform.SetParent(transform, false);

            hex = NewPoly(mainMat, true);
            hex.DrawRegPoly(1, 6, Mathf.PI / 6f, 1, 0);
            hex.name = "hexagon";
            hex.SetColor(shapeColor);
            hex.transform.SetParent(transform, false);

            tetra = NewPoly(mainMat, true);
            const int d = 1;
            tetra.Draw3DPoly(
                new[]
                {
                    new Vector3(d, d, d),
                    new Vector3(d, -d, -d),
                    new Vector3(-d, d, -d),
                    new Vector3(-d, -d, d)
                },
                new[] {0, 1, 2, 0, 3, 1, 0, 2, 3, 1, 3, 2});
            tetra.name = "tetrahedron";
            tetra.SetColor(shapeColor);
            tetra.transform.SetParent(transform, false);

            icoSphereCut0 = NewPoly(mainMat, true);
            IcoSphere.VertsAndFaces ivaf = IcoSphere.NewIcoVertsAndFaces(1, 0);
            IcoSphere.NewSphere(icoSphereCut0, ivaf.verts, ivaf.faces, false);
            icoSphereCut0.name = "icoSphereCut0";
            icoSphereCut0.SetColor(shapeColor);
            icoSphereCut0.transform.SetParent(transform, false);

            icoSphere0 = NewPoly(mainMat, true);
            ivaf = IcoSphere.NewIcoVertsAndFaces(1, 0);
            IcoSphere.NewSphere(icoSphere0, ivaf.verts, ivaf.faces, false);
            icoSphere0.name = "icoSphere0";
            icoSphere0.SetColor(shapeColor);
            icoSphere0.transform.SetParent(transform, false);

            dodecSphere = NewPoly(mainMat, true);
            ivaf = IcoSphere.NewDodecVertsAndFaces(1);
            IcoSphere.NewSphere(dodecSphere, ivaf.verts, ivaf.faces, false);
            dodecSphere.name = "dodec";
            dodecSphere.SetColor(shapeColor);
            dodecSphere.transform.SetParent(transform, false);

            dodecSphereCut = NewPoly(mainMat, true);
            ivaf = IcoSphere.NewDodecVertsAndFaces(1);
            IcoSphere.NewSphere(dodecSphereCut, ivaf.verts, ivaf.faces, false);
            dodecSphereCut.name = "dodecCut";
            dodecSphereCut.SetColor(shapeColor);
            dodecSphereCut.transform.SetParent(transform, false);
        }

        public static Polygon NewPoly(Material passMat, bool wireFrame)
        {
            Polygon newPoly = new GameObject("Polygon").AddComponent<Polygon>();
            AddMesh(newPoly.gameObject, newPoly, passMat, wireFrame);
            newPoly.rend = newPoly.gameObject.GetComponent<Renderer>();

            return newPoly;
        }

        public static Circle NewCirclePoly(Material passMat, bool wireFrame = false)
        {
            Circle newPoly = new GameObject("CirclePolygon").AddComponent<Circle>();
            AddMesh(newPoly.gameObject, newPoly, passMat, wireFrame);
            newPoly.rend = newPoly.gameObject.GetComponent<Renderer>();

            return newPoly;
        }

        public static Line NewLinePoly(Material passMat, bool wireFrame)
        {
            Line newPoly = new GameObject("LinePolygon").AddComponent<Line>();
            AddMesh(newPoly.gameObject, newPoly, passMat, wireFrame);
            newPoly.rend = newPoly.gameObject.GetComponent<Renderer>();

            return newPoly;
        }
        
        public static Rectangle NewRectPoly(Material passMat, bool wireFrame)
        {
            Rectangle newPoly = new GameObject("RectPolygon").AddComponent<Rectangle>();
            AddMesh(newPoly.gameObject, newPoly, passMat, wireFrame);
            newPoly.rend = newPoly.gameObject.GetComponent<Renderer>();

            return newPoly;
        }
        
        public static Polygon DrawTri(float h, float b, Color passColor)
        {
            Polygon newTri = new GameObject("TrianglePolygon").AddComponent<Polygon>();
            AddMesh(newTri.gameObject, newTri, SolarClock.Instance.mainMat, false);
            newTri.rend = newTri.GetComponent<Renderer>();
            Vector3[] skinList = {
                new Vector3(0, 0, h), 
                new Vector3(b * .5f, 0, 0),
                new Vector3(-b * .5f, 0, 0)
            };

            int[] indList = {0, 1, 2};
            
            newTri.Draw3DPoly(skinList, indList);
            newTri.SetColor(passColor);
            return newTri;
        }
        
        public static GameObject DrawDottedLine(Vector3 pt0, Vector3 pt1, Color passColor, float passLW)
        {
            GameObject line;
            line = new GameObject("Line");
            float length = Vector3.Distance(pt0, pt1);
            int totalDot = Mathf.FloorToInt(length / (passLW / 3f));
            Polygon dot;
            for (int ii = 0; ii < totalDot; ii++)
            {
                dot = Instantiate(NewCylinder.rootDot);
                dot.transform.SetParent(line.transform, false);
                dot.transform.localScale = Vector3.one * passLW;
                float mag = -ii * length / totalDot;
                if (pt1.x > 0)
                {
                    dot.transform.localPosition = pt0 + new Vector3(mag - .5f, 0, 0);
                }
                else
                {
                    dot.transform.localPosition = pt0 + new Vector3(0, 0, mag - .5f);
                }
            }

            return line;
        }
        
        public static void AddMesh(GameObject polyGO, Polygon basePoly, Material passMat, bool wireFrame)
        {
            // add mesh;
            MeshFilter filter = polyGO.AddComponent(typeof(MeshFilter)) as MeshFilter;
            filter.sharedMesh = new Mesh();
            MeshRenderer meshRend = polyGO.AddComponent(typeof(MeshRenderer)) as MeshRenderer;
            meshRend.sharedMaterial = passMat;
            meshRend.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            meshRend.receiveShadows = false;
            meshRend.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
            basePoly.meshFilter = filter;
            basePoly.wireFrame = wireFrame;
        }
    }

    public static class IcoSphere
    {
        public struct TriangleIndices
        {
            public readonly int v1;
            public readonly int v2;
            public readonly int v3;

            public TriangleIndices(int _v1, int _v2, int _v3)
            {
                v1 = _v1;
                v2 = _v2;
                v3 = _v3;
            }
        }

        public struct VertsAndFaces
        {
            public readonly Vector3[] verts;
            public readonly TriangleIndices[] faces;

            public VertsAndFaces(Vector3[] _verts, TriangleIndices[] _faces)
            {
                verts = _verts;
                faces = _faces;
            }
        }

        public static void NewSphere(Polygon poly, Vector3[] verts, TriangleIndices[] faces, bool doubleSide)
        {
            Mesh sharedMesh = poly.meshFilter.sharedMesh;
            sharedMesh.Clear();

            sharedMesh.vertices = verts;

            List<int> triList = new List<int>();
            for (int i = 0; i < faces.Length; i++)
            {
                triList.Add(faces[i].v1);
                triList.Add(faces[i].v2);
                triList.Add(faces[i].v3);

                if (doubleSide)
                {
                    triList.Add(faces[i].v1);
                    triList.Add(faces[i].v3);
                    triList.Add(faces[i].v2);
                }
            }
            sharedMesh.triangles = triList.ToArray();

            Vector3[] normals = new Vector3[verts.Length];
            for (int i = 0; i < normals.Length; i++)
                normals[i] = verts[i].normalized;

            sharedMesh.normals = normals;

            sharedMesh.RecalculateBounds();

            poly.meshFilter.sharedMesh = sharedMesh;
        }

        public static VertsAndFaces NewIcoVertsAndFaces(float radius, int recursionLevel)
        {
            List<Vector3> vertList = new List<Vector3>();
            Dictionary<long, int> middlePointIndexCache = new Dictionary<long, int>();

            // create 12 vertices of a icosahedron
            float t = (1 + Mathf.Sqrt(5f)) / 2f;

            vertList.Add(new Vector3(-1, t, 0).normalized * radius); // 0
            vertList.Add(new Vector3(1, t, 0).normalized * radius); // 1

            vertList.Add(new Vector3(0, 1, t).normalized * radius); // 5
            vertList.Add(new Vector3(0, 1, -t).normalized * radius); // 7

            vertList.Add(new Vector3(-t, 0, 1).normalized * radius); // 11
            vertList.Add(new Vector3(-t, 0, -1).normalized * radius); //10

            vertList.Add(new Vector3(t, 0, -1).normalized * radius); // 8
            vertList.Add(new Vector3(t, 0, 1).normalized * radius); // 9

            vertList.Add(new Vector3(0, -1, -t).normalized * radius); // 6
            vertList.Add(new Vector3(0, -1, t).normalized * radius); // 4


            vertList.Add(new Vector3(-1, -t, 0).normalized * radius); // 2
            vertList.Add(new Vector3(1, -t, 0).normalized * radius); // 3

            // create 20 triangles of the icosahedron
            List<TriangleIndices> faces = new List<TriangleIndices>
            {
                // 5 faces around point 0
                new TriangleIndices(0, 2, 1),
                new TriangleIndices(0, 1, 3),
                new TriangleIndices(0, 4, 2),
                new TriangleIndices(0, 3, 5),
                new TriangleIndices(0, 5, 4),
                
                new TriangleIndices(1, 6, 3),
                new TriangleIndices(1, 2, 7),
                new TriangleIndices(1, 7, 6),
                
                new TriangleIndices(3, 6, 8),
                new TriangleIndices(3, 8, 5),
                
                new TriangleIndices(2, 4, 9),
                new TriangleIndices(2, 9, 7),
                
                new TriangleIndices(4, 5, 10),
                new TriangleIndices(4, 10, 9),
                new TriangleIndices(5, 8, 10),
            
                // 5 faces around point 11
                new TriangleIndices(6, 7, 11),
                new TriangleIndices(7, 9, 11),
                new TriangleIndices(9, 10, 11),
                new TriangleIndices(8, 11, 10),
                new TriangleIndices(8, 6, 11)
            };

            // refine triangles
            for (int i = 0; i < recursionLevel; i++)
            {
                List<TriangleIndices> faces2 = new List<TriangleIndices>();
                foreach (TriangleIndices tri in faces)
                {
                    // replace triangle by 4 triangles
                    int a = getMiddlePoint(tri.v1, tri.v2, ref vertList, ref middlePointIndexCache, radius);
                    int b = getMiddlePoint(tri.v2, tri.v3, ref vertList, ref middlePointIndexCache, radius);
                    int c = getMiddlePoint(tri.v3, tri.v1, ref vertList, ref middlePointIndexCache, radius);

                    faces2.Add(new TriangleIndices(tri.v1, a, c));
                    faces2.Add(new TriangleIndices(tri.v2, b, a));
                    faces2.Add(new TriangleIndices(tri.v3, c, b));
                    faces2.Add(new TriangleIndices(a, b, c));
                }
                faces = faces2;
            }

            return new VertsAndFaces(vertList.ToArray(), faces.ToArray());
        }

        static int getMiddlePoint(int p1, int p2, ref List<Vector3> vertices, ref Dictionary<long, int> cache,
            float radius)
        {
            // return index of point in the middle of p1 and p2

            // first check if we have it already
            bool firstIsSmaller = p1 < p2;
            long smallerIndex = firstIsSmaller ? p1 : p2;
            long greaterIndex = firstIsSmaller ? p2 : p1;
            long key = (smallerIndex << 32) + greaterIndex;

            if (cache.TryGetValue(key, out int ret))
            {
                return ret;
            }

            // not in cache, calculate it
            Vector3 point1 = vertices[p1];
            Vector3 point2 = vertices[p2];
            Vector3 middle = new Vector3
            (
                (point1.x + point2.x) / 2f,
                (point1.y + point2.y) / 2f,
                (point1.z + point2.z) / 2f
            );

            // add vertex makes sure point is on unit sphere
            int i = vertices.Count;
            vertices.Add(middle.normalized * radius);

            // store it, return index
            cache.Add(key, i);

            return i;
        }

        public static VertsAndFaces NewDodecVertsAndFaces(float r)
        {
            // Calculate constants that will be used to generate vertices
            float phi = (Mathf.Sqrt(5f) - 1) / 2f; // The golden ratio

            float a = 1f / Mathf.Sqrt(3f);
            float b = a / phi;
            float c = a * phi;

            // Generate each vertex
            List<Vector3> vertices = new List<Vector3>();

            foreach (int i in new[] {-1, 1})
            {
                foreach (int j in new[] {-1, 1})
                {
                    vertices.Add(new Vector3(0, i * c * r, j * b * r));
                    vertices.Add(new Vector3(i * c * r, j * b * r, 0));
                    vertices.Add(new Vector3(i * b * r, 0, j * c * r));

                    vertices.AddRange(new[] {-1, 1}
                        .Select(k => new Vector3(i * a * r, j * a * r, k * a * r)));
                }
            }

            List<TriangleIndices> faces = new List<TriangleIndices>
            {
                // 0, 1, 3, 11, 13
                new TriangleIndices(0, 1, 3),
                new TriangleIndices(0, 11, 1),
                new TriangleIndices(0, 13, 11),
                // 0, 2, 3, 8, 10
                new TriangleIndices(10, 2, 8),
                new TriangleIndices(10, 3, 2),
                new TriangleIndices(10, 0, 3),
                // 0, 10, 12, 13, 18
                new TriangleIndices(10, 13, 0),
                new TriangleIndices(10, 12, 13),
                new TriangleIndices(10, 18, 12),
                // 1, 2, 3, 4, 7
                new TriangleIndices(4, 3, 1),
                new TriangleIndices(7, 3, 4),
                new TriangleIndices(2, 3, 7),
                // 1, 4, 5, 11, 14
                new TriangleIndices(1, 11, 4),
                new TriangleIndices(11, 14, 4),
                new TriangleIndices(14, 5, 4),
                // 2, 6, 7, 8, 9
                new TriangleIndices(9, 2, 7),
                new TriangleIndices(9, 8, 2),
                new TriangleIndices(9, 6, 8),
                // 4, 5, 7, 9, 15
                new TriangleIndices(9, 7, 4),
                new TriangleIndices(9, 4, 5),
                new TriangleIndices(9, 5, 15),
                // 5, 14, 15, 17, 19
                new TriangleIndices(19, 14, 17),
                new TriangleIndices(19, 5, 14),
                new TriangleIndices(19, 15, 5),
                // 6, 8, 10, 16, 18
                new TriangleIndices(6, 10, 8),
                new TriangleIndices(6, 16, 10),
                new TriangleIndices(16, 18, 10),
                // 6, 9, 15, 16, 19
                new TriangleIndices(16, 6, 9),
                new TriangleIndices(16, 9, 19),
                new TriangleIndices(19, 9, 15),
                // 11, 12, 13, 14, 17
                new TriangleIndices(11, 13, 14),
                new TriangleIndices(13, 12, 14),
                new TriangleIndices(12, 17, 14),
                // 12, 16, 17, 18, 19
                new TriangleIndices(12, 19, 17),
                new TriangleIndices(12, 18, 19),
                new TriangleIndices(18, 16, 19)
            };

            VertsAndFaces dhvf = new VertsAndFaces(vertices.ToArray(), faces.ToArray());

            return dhvf;
        }
    }
}