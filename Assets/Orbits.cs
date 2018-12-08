using UnityEngine;

namespace Assets
{
    public class Orbits : MonoBehaviour
    {
        // container for 4 inner planets
        public GameObject[] planets = new GameObject[4];
        public GameObject[] paths = new GameObject[4];

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
            GameObject path;
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

                if (count != 2)
                {
                    path = Shapes.DrawRing(
                        planetData.orbitMultiplier * earthOR,
                        planetData.orbitMultiplier * earthOR - minorThick, 
                        .33f, 
                        minorColor,
                        planetData.orbitMultiplier * earthOR * DoverR * velComp,
                        true);
                    Shapes.DrawTri(
                        planetData.orbitMultiplier * earthOR,
                        2,
                        planetData.planetHandColor).transform.SetParent(planetOrbit.transform, false);
                }
                else
                {
                    // earth
                    path = Shapes.DrawRing(
                        earthOR,
                        earthOR - .5f,
                        .833f,
                        new Color(1, 1, 1, 1),
                        earthOR * DoverR * velComp,
                        true);
                    Shapes.DrawTri(
                        planetData.radius,
                        5,
                        planetData.planetHandColor).transform.SetParent(planetOrbit.transform, false);
                }

                path.transform.SetParent(planetOrbit.transform, false);
                path.transform.localScale = new Vector3(1, flatScale, 1);
                paths[count] = path;
                count++;
            }
        }

        public static float GetOrbitPos(
            System.DateTime passDate, 
            float passPeriodInDays, 
            float passOffset = 0)
        {
            long ticksIntoCurrentYear = passDate.Ticks -
                                        System.DateTime.MinValue.AddYears(
                                            System.DateTime.Now.Year - 1).Ticks;

            float daysIntoCurrentYear = System.Convert.ToSingle(ticksIntoCurrentYear) /
                                        10000000 /
                                        SolarClock.SiderealDayInSeconds;

            // convert to degrees
            return -(daysIntoCurrentYear / passPeriodInDays) * 360 + passOffset;
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
