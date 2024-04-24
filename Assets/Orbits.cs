using System;
using Assets.GraphicsUtil.Shapes;
using UnityEngine;

namespace Assets
{
    /// <summary>
    /// Creates inner planets and sets their positions. The angular position is calculated based on the current date.
    /// </summary>
    public class Orbits : MonoBehaviour
    {
        enum Planets
        {
            Mercury = 0,
            Venus = 1,
            Earth = 2,
            Mars = 3
        }

        // container for 4 inner planets
        readonly GameObject[] planetContainers = new GameObject[Enum.GetValues(typeof(Planets)).Length];
        public Circle[] paths = new Circle[Enum.GetValues(typeof(Planets)).Length];

        // values relative to Earth
        readonly float[] planetRadii =
        {
            .238f,
            .95f,
            1,
            .532f
        };

        readonly float[] planetOrbits =
        {
            .387f,
            .723f,
            1,
            1.523f
        };
        
        readonly float[] orbitalPeriods =
        {
            87.969f,
            224.698f,
            365.256363004f,
            686.971f
        };
        
        // these are arbitrary values that start from the zero day in 1970
        readonly float[] orbitalOffsets =
        {
            60,
            120,
            0,
            -130
        };

        readonly Color[] planetColors =
        {
            new(.4f, .5f, .6f),
            new(.97f, .97f, .85f),
            new(.8f, .92f, .97f),
            new(.9f, .2f, .3f)
        };

        readonly Color[] planetHandColors =
        {
            new(.4f, .5f, .6f, .3f),
            new(.97f, .97f, .85f, .3f),
            new(.3f, .3f, 1f, 1f),
            new(.9f, .2f, .3f, .3f)
        };

        // calibration vars
        const float EarthR = SolarClock.SYSTEM_DIAMETER * .033f;
        
        // galactic spiral values
        const float DoverR = 47.33f;
        const float VelocityCompensation = .1f;
        public float flatScale = .01f;

        public void NewOrbits()
        {
            const float earthOrbitalR = SolarClock.SYSTEM_DIAMETER * .5f; // arbitrary - put Earth at the halfway point
            foreach (Planets planet in Enum.GetValues(typeof(Planets)))
            {
                GameObject planetContainer = new GameObject(planet.ToString());
                planetContainer.transform.SetParent(transform, false);
                planetContainers[(int)planet] = planetContainer;

                GameObject planetSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                planetSphere.GetComponent<Renderer>().material.color = planetColors[(int)planet];
                planetSphere.transform.SetParent(planetContainer.transform, false);
                planetSphere.transform.Translate(Vector3.forward * planetOrbits[(int)planet] * earthOrbitalR);
                planetSphere.transform.localScale = Vector3.one * planetRadii[(int)planet] * EarthR;

                Circle path = PolygonFactory.NewCirclePoly(PolygonFactory.Instance.mainMat);
                Polygon planetTri;
                if (planet != Planets.Earth)
                {
                    path.DrawRing(
                        planetOrbits[(int)planet] * earthOrbitalR,
                        planetOrbits[(int)planet] * earthOrbitalR - SolarClock.SYSTEM_DIAMETER * .002f,
                        .33f,
                        0,
                        planetOrbits[(int)planet] * earthOrbitalR * DoverR * VelocityCompensation,
                        true);
                    path.SetColor(new Color(1, 1, 1, .5f));
                    planetTri = PolygonFactory.DrawTri(
                        planetOrbits[(int)planet] * earthOrbitalR,
                        SolarClock.SYSTEM_DIAMETER * .0133f,
                        planetHandColors[(int)planet]);
                }
                else
                {
                    // earth
                    path.DrawRing(
                        earthOrbitalR,
                        earthOrbitalR - SolarClock.SYSTEM_DIAMETER * .00333f,
                        .833f,
                        0,
                        earthOrbitalR * DoverR * VelocityCompensation,
                        true);
                    path.SetColor(new Color(1, 1, 1, 1));
                    planetTri = PolygonFactory.DrawTri(
                        earthOrbitalR,
                        SolarClock.SYSTEM_DIAMETER * .0333f,
                        planetHandColors[(int)planet]);
                }

                path.name = "planet path";
                path.transform.SetParent(planetContainer.transform, false);
                path.transform.localScale = new Vector3(1, flatScale, 1);
                paths[(int)planet] = path;

                planetTri.transform.SetParent(planetContainer.transform, false);
                planetTri.name = "planet triangle";
            }
        }

        public void SetLittlePlanetsOrbit(DateTime newDateUTC)
        {
            int count = 0;
            foreach (GameObject planetGo in planetContainers)
            {
                planetGo.transform.localRotation = Quaternion.AngleAxis(
                    count != (int)Planets.Earth 
                        ? GetNonEarthOrbitAngle(newDateUTC, orbitalPeriods[count], orbitalOffsets[count]) 
                        : GetEarthOrbitAngle(newDateUTC),
                    Vector3.up);

                count++;
            }
        }

        public static float GetEarthOrbitAngle(DateTime passDate)
        {
            // Earth's orbit is always fixed to the amount of days into the year divided by a year
            long ticksIntoCurrentYear = passDate.Ticks -
                                        DateTime.MinValue.AddYears(
                                            DateTime.Now.Year - 1).Ticks;

            TimeSpan timeSpan = new(ticksIntoCurrentYear);

            // convert to degrees
            float woundAngle = -((float)timeSpan.TotalDays / SolarClock.YEAR) * 360 + SolarClock.SunSprockOffset();
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
            DateTime Date_2019_01_01 = new(2019, 1, 1);
            long ticksFrom_2019_01_01 = passDate.Ticks - Date_2019_01_01.Ticks;

            TimeSpan timeSpan = new(ticksFrom_2019_01_01);
            float periodInSeconds = passPeriodInDays * 24 * 60 * 60;
            // convert to degrees
            float woundAngle = -((float)timeSpan.TotalSeconds / periodInSeconds) * 360 + passOffset;
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
    }
}