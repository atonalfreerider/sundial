using Assets;
using Assets.GraphicsUtil.Shapes;
using Assets.GraphicsUtil.Shapes.Lines;
using UnityEngine;

namespace Assets.GraphicsUtil.Shapes
{
    public class PolygonFactory : MonoBehaviour
    {
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
                new(0, 0, h), 
                new(b * .5f, 0, 0),
                new(-b * .5f, 0, 0)
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
}