using UnityEngine;

namespace Assets.GraphicsUtil.Shapes
{
    public class Rectangle : Polygon
    {
        // the current width and height of the rectangle;
        public float curW;
        public float curH;
        
        public void DrawRect(float w, float h, float d)
        {
            curH = h;
            curW = w;

            Vector3[] skinList = new Vector3[4];

            skinList[0] = new Vector3(-w * .5f, 0f, h * .5f);
            skinList[1] = new Vector3(w * .5f, 0f, h * .5f);
            skinList[2] = new Vector3(w * .5f, 0f, -h * .5f);
            skinList[3] = new Vector3(-w * .5f, 0f, -h * .5f);

            int[] indList = {0, 1, 2, 0, 2, 3};

            if (d < float.Epsilon)
            {
                // a rectangle with 0 depth - only draw two sides;
                Draw3DPoly(skinList, MirrorIndices(indList, 0));
            }
            else
            {
                Extrude(skinList, indList, d, false, true, 0f);
            }
        }
    }

    public static class NewCube
    {
        public static Polygon cube;
        
        // rectangles
        public static Rectangle transRectPoly, textureRectPoly;

        public static void InitCube(PolygonFactory polygonFactory, Material mainMat)
        {
            // Rectangle
            transRectPoly = PolygonFactory.NewRectPoly(mainMat, false);
            transRectPoly.DrawRect(1, 1, 0);
            transRectPoly.name = "transRectPoly";
            transRectPoly.transform.SetParent(polygonFactory.transform, false);
        }
    }
}