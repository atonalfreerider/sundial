using System;
using Assets.GraphicsUtil.Shapes;
using UnityEngine;

namespace Assets
{
    public class Orbits : MonoBehaviour
    {
        // container for 4 inner planets
        public GameObject[] planets = new GameObject[4];
        public Circle[] paths = new Circle[4];

        // calibration vars
        const float earthR = 5f;
        const float DoverR = 47.33f;
        const float velComp = .1f;
        public float flatScale = .01f;
        readonly PlanetData[] planetDatas =
        {
            new PlanetData(
                "Mercury",
                new Color(.4f, .5f, .6f),
                new Color(.4f, .5f, .6f, .3f),
                .387f,
                .238f * earthR
            ),
            new PlanetData(
                "Venus",
                new Color(.97f, .97f, .85f),
                new Color(.97f, .97f, .85f, .3f),
                .723f,
                .95f * earthR
            ),
            new PlanetData(
                "Earth",
                new Color(.8f, .92f, .97f),
                new Color(.3f, .3f, 1f, 1f),
                1,
                earthR
            ),
            new PlanetData(
                "Mars",
                new Color(.9f, .2f, .3f),
                new Color(.9f, .2f, .3f, .3f),
                1.523f,
                .532f * earthR
            )
        };

        public void NewOrbits(float earthOR)
        {
            GameObject planetOrbit;
            int count = 0;
            GameObject planet;
            Circle path;
            Polygon planetTri;
            Color minorColor = new Color(1, 1, 1, .5f);
            float minorThick = .3f;
            foreach (PlanetData planetData in planetDatas)
            {
                planetOrbit = new GameObject(planetDatas[count].planetName);
                planetOrbit.transform.SetParent(transform, false);
                planets[count] = planetOrbit;
                
                planet = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                planet.GetComponent<Renderer>().material.color = planetData.planetColor;
                planet.transform.SetParent(planetOrbit.transform, false);
                planet.transform.Translate(Vector3.forward * planetData.orbitMultiplier * earthOR);
                planet.transform.localScale = Vector3.one * planetData.radius;

                path = PolygonFactory.NewCirclePoly(SolarClock.Instance.mainMat);
                if (count != 2)
                {
                    path.DrawRing(
                        planetData.orbitMultiplier * earthOR,
                        planetData.orbitMultiplier * earthOR - minorThick,
                        .33f,
                        0,
                        planetData.orbitMultiplier * earthOR * DoverR * velComp,
                        true);
                    path.SetColor(minorColor);
                    planetTri = PolygonFactory.DrawTri(
                        planetData.orbitMultiplier * earthOR,
                        2,
                        planetData.planetHandColor);
                }
                else
                {
                    // earth
                    path.DrawRing(
                        earthOR,
                        earthOR - .5f,
                        .833f,
                        0,
                        earthOR * DoverR * velComp,
                        true);
                    path.SetColor(new Color(1, 1, 1, 1));
                    planetTri = PolygonFactory.DrawTri(
                        earthOR,
                        5,
                        planetData.planetHandColor);
                }

                path.name = "planet path";
                path.transform.SetParent(planetOrbit.transform, false);
                path.transform.localScale = new Vector3(1, flatScale, 1);
                paths[count] = path;
                
                planetTri.transform.SetParent(planetOrbit.transform, false);
                planetTri.name = "planet triangle";
                
                count++;
            }
        }

        public static float GetEarthOrbitAngle(DateTime passDate)
        {
            // Earth's orbit is always fixed to the amount of days into the year divided by a year
            long ticksIntoCurrentYear = passDate.Ticks -
                                        DateTime.MinValue.AddYears(
                                            DateTime.Now.Year - 1).Ticks;
            
            TimeSpan timeSpan = new TimeSpan(ticksIntoCurrentYear);

            // convert to degrees
            float woundAngle = -((float) timeSpan.TotalDays / SolarClock.YEAR) * 360 + SolarClock.SunSprockOffset();
            return UnwindAngle(woundAngle);
        }

        public static float GetNonEarthOrbitAngle(
            DateTime passDate, 
            float passPeriodInDays, 
            float passOffset = 0)
        {
            // anything that is not the earth follows an orbit defined by a period
            // since the planets and the moon don't have a naturally occuring ordinal position, define an arbitrary
            // one and offset the orbits by an arbitrary amount
            DateTime Date_2019_01_01 = new DateTime(2019,1,1);
            long ticksFrom_2019_01_01 = passDate.Ticks - Date_2019_01_01.Ticks;
            
            TimeSpan timeSpan = new TimeSpan(ticksFrom_2019_01_01);
            float periodInSeconds = passPeriodInDays * 24 * 60 * 60;
            // convert to degrees
            float woundAngle = -((float) timeSpan.TotalSeconds / periodInSeconds) * 360 + passOffset;
            return UnwindAngle(woundAngle);
        }
        
        public static float UnwindAngle(float angle)
        {
            while (angle > 180)
            {
                angle -= 360;
            }

            while (angle < -180)
            {
                angle += 360;
            }
            return angle;
        }

        struct PlanetData
        {
            public readonly string planetName;
            public readonly Color planetColor;
            public readonly Color planetHandColor;
            public readonly float orbitMultiplier;
            public readonly float radius;

            public PlanetData(
                string planetName, 
                Color planetColor,
                Color planetHandColor,
                float orbitMultiplier,
                float radius)
            {
                this.planetName = planetName;
                this.planetColor = planetColor;
                this.planetHandColor = planetHandColor;
                this.orbitMultiplier = orbitMultiplier;
                this.radius = radius;
            }
        }
    }
}
