using UnityEngine;
using System.Collections;

public class Orbits : MonoBehaviour {
    // container for 4 inner planets;
    public GameObject[] planets = new GameObject[4];
    public GameObject[] paths = new GameObject[4];

    // calibration vars;
    float earthR = 5f;
    float DoverR = 47.33f;
    float velComp = .1f;
    public float flatScale = .01f;
    string[] planetLabels = new string[4] { "MercuryOrbit", "VenusOrbit", "EarthOrbit", "MarsOrbit" };
    Color[] planetColors = new Color[4] { new Color(.4f, .5f, .6f), new Color(.97f, .97f, .85f), new Color(.8f, .92f, .97f), new Color(.9f, .2f, .3f) };
    Color[] planetHandColors = new Color[4] { new Color(.4f, .5f, .6f, .3f), new Color(.97f, .97f, .85f, .3f), new Color(.3f, .3f, 1f, 1f), new Color(.9f, .2f, .3f, .3f) };

    public void NewOrbits(float earthOR) {
        float[] planetORs = new float[4] { earthOR * .387f, earthOR * .723f, earthOR, earthOR * 1.523f };
        float[] planetRs = new float[4] { earthR * .238f, earthR * .95f, earthR, earthR * .532f };
        GameObject pl;
        int count = 0;
        GameObject planet;
        GameObject path;
        Color minorColor = new Color(1f, 1f, 1f, .5f);
        float minorThick = .3f;
        foreach (float plR in planetORs) {
            pl = new GameObject();
            pl.name = planetLabels[count];
            planet = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            planet.transform.localScale = new Vector3(planetRs[count], planetRs[count], planetRs[count]);
            planet.GetComponent<Renderer>().material.color = planetColors[count];

            if (count == 0 || count == 1 || count == 3) {
                path = Shapes.DrawRing(plR, plR - minorThick, 1f / 3f, minorColor, planetORs[count] * DoverR * velComp, true);
                Shapes.DrawTri(plR, 2f, planetHandColors[count]).transform.parent = pl.transform;
            }
            else {
                path = Shapes.DrawRing(plR, plR - .5f, 5f / 6f, new Color(1f, 1f, 1f, 1f), earthOR * DoverR * velComp, true);
                Shapes.DrawTri(plR, 5f, planetHandColors[count]).transform.parent = pl.transform;
            }
            path.transform.parent = pl.transform;
            path.transform.localScale = new Vector3(1f, flatScale, 1f);
            paths[count] = path;
            planet.transform.Translate(Vector3.forward * plR);
            planet.transform.parent = pl.transform;
            planets[count] = pl;
            pl.transform.parent = this.transform;
            count++;
        }
    }

    public static float getOrbitPos(System.DateTime passDate, float passPeriod, float passOffset) {
        //Debug.Log(-((System.Convert.ToSingle(passDate.Ticks - System.DateTime.MinValue.AddYears(System.DateTime.Now.Year - 1).Ticks)) / 10000f / 1000f / 60f / 60f / 24f / passPeriod) * 360f);
        return -((System.Convert.ToSingle(passDate.Ticks - System.DateTime.MinValue.AddYears(System.DateTime.Now.Year - 1).Ticks)) / 10000f / 1000f / 60f / 60f / 24f / passPeriod) * 360f + passOffset;
    }

}
