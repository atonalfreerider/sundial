using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

namespace Assets
{
    public class Earth : MonoBehaviour
    {
        // calibration vars
        const float earthSidereal = 23.9344696f;
        const float earthSynodic = 24;
        public float earthR;
        float localR;

        // persistent object
        public GameObject earthSys;
        public GameObject earthSph;
        public GameObject localWheelCont;
        public Moon moonDial;
        public Text day11;
        public Text day12;
        public GameObject intDatelineSplit;
        public Text day21;
        public Text day22;
        GameObject strip;
        GameObject earthSprock;

        // INIT Functions
        public void NewEarthSystem(float passEarthR, System.DateTime passDate)
        {
            earthR = passEarthR;
            //...(0) Earth Line
            GameObject earthLineCont = new GameObject("EarthLine");
            float earthH = earthR * .99f;
            GameObject earthLine = Shapes.DrawTri(
                earthH, 
                earthR * .05f,
                new Color(1, 1, 1, .3f));
            earthLine.transform.SetParent(earthLineCont.transform, false);
            //GameObject earthLine2 = Shapes.DrawTri(earthH * .7f, earthR * .02f, Color.white)
            //earthLine2.transform.SetParent(earthLineCont.transform
            earthLineCont.transform.SetParent(transform, false);

            // ...(1) Earth System
            earthSys = new GameObject("EarthSys");
            // .........(1) Earth Sprocket;
            GameObject earthSprockCont = new GameObject("EarthSprockCont");
            Items.AddCanvas(earthSprockCont);

            // create point cloud for earth sprocket mesh
            List<Vector3> pointList = new List<Vector3>();
            List<int> indList = new List<int>();
            object[] retArr = new object[3];
            int pointCounter = 0;
            int counter = 0;
            float alpha;
            float sprockTh = 1f;
            float baseAl = .007f;
            float pointAl = .003f;
            float smallH = 3f;
            float bigH = 7f;
            float hR = earthR * .4f;
            float stepD = 360f / 96 * Mathf.PI / 180f;
            for (int tt = 0; tt < 24; tt++)
            {
                alpha = -counter * stepD;
                // create hour tick

                retArr = Shapes.SprockTick(hR, bigH, alpha, -(counter + 1) * stepD, sprockTh, baseAl, pointAl,
                    pointCounter, 0);
                pointList.AddRange((List<Vector3>) retArr[0]);
                indList.AddRange((List<int>) retArr[1]);
                pointCounter = (int) retArr[2];
                counter++;
                for (int dd = 0; dd < 3; dd++)
                {
                    alpha = -counter * stepD;
                    // create 15min tick
                    retArr = Shapes.SprockTick(hR, smallH, alpha, -(counter + 1) * stepD, sprockTh, baseAl, pointAl,
                        pointCounter, 0);
                    pointList.AddRange((List<Vector3>) retArr[0]);
                    indList.AddRange((List<int>) retArr[1]);
                    pointCounter = (int) retArr[2];
                    counter++;
                }
            }

            indList.RemoveRange(indList.Count - 6, 6);

            earthSprock = Shapes.CreatePoly(pointList, indList, Color.white);
            earthSprock.name = "EarthSprock";
            earthSprock.transform.SetParent(earthSprockCont.transform);

            //...........................(1) hLabelWheel
            // create 24 hour marks
            GameObject hLabelWheel = new GameObject();
            Items.AddCanvas(hLabelWheel);
            hLabelWheel.name = "HourLabelWheel";
            Text hLabel;
            for (int ht = 0; ht < 24; ht++)
            {
                hLabel = Items.NewText(ht.ToString(), Color.white, 40, TextAnchor.MiddleCenter, false);
                hLabel.transform.Rotate(Vector3.forward, ht * (360f / 24f));
                hLabel.transform.Translate(Vector3.up * (earthR * .4f - 10f));
                if ((ht >= 0 && ht < 6) || ht > 18)
                    hLabel.transform.Rotate(Vector3.forward, 180f);

                hLabel.transform.SetParent(hLabelWheel.transform);
            }

            hLabelWheel.transform.SetParent(earthSprockCont.transform);

            //...........................(3) dayDil
            localR = earthR * .28f;

            //System.Collections.ObjectModel.ReadOnlyCollection<System.TimeZoneInfo> zones = System.TimeZoneInfo.GetSystemTimeZones()
            //System.TimeZoneInfo dstZone = zones[0]

            // System.DateTime dsTime = System.TimeZoneInfo.ConvertTimeFromUtc(System.DateTime.UtcNow, dstZone)
            int datelineDay = GetDatelineDay(System.DateTime.UtcNow);
            int yesterday = datelineDay - 1;
            if (yesterday < 0)
                yesterday = 6;

            const float angOffset = 4.2f;
            day11 = Items.NewText(Calendar.daysofweekAbr[yesterday], Color.white, 30, TextAnchor.MiddleCenter, false);
            day11.transform.SetParent(earthSprockCont.transform);
            day11.transform.Rotate(Vector3.forward * (180f - angOffset));
            day11.transform.Translate(Vector3.up * -(localR - 2.25f));

            day21 = Items.NewText(Calendar.daysofweekAbr[datelineDay], Color.white, 30, TextAnchor.MiddleCenter, false);
            day21.transform.SetParent(earthSprockCont.transform);
            day21.transform.Rotate(Vector3.forward * (180f + angOffset));
            day21.transform.Translate(Vector3.up * -(localR - 2.25f));

            intDatelineSplit = new GameObject("IntDatelineSplit");
            Items.AddCanvas(intDatelineSplit);
            intDatelineSplit.transform.rotation = Quaternion.identity;

            day12 = Items.NewText(Calendar.daysofweekAbr[yesterday], Color.white, 30, TextAnchor.MiddleCenter, false);
            day12.transform.SetParent(intDatelineSplit.transform);
            day12.transform.Rotate(Vector3.forward * (180f + angOffset));
            day12.transform.Translate(Vector3.up * -(localR - 2.25f));

            day22 = Items.NewText(Calendar.daysofweekAbr[datelineDay], Color.white, 30, TextAnchor.MiddleCenter, false);
            day22.transform.SetParent(intDatelineSplit.transform);
            day22.transform.Rotate(Vector3.forward * (180f - angOffset));
            day22.transform.Translate(Vector3.up * -(localR - 2.25f));

            intDatelineSplit.transform.SetParent(earthSprockCont.transform);

            strip = new GameObject();
            strip.transform.SetParent(earthSprock.transform, false);

            earthSprockCont.transform.SetParent(earthSys.transform);

            //........ (2) local wheel
            List<Vector3> pointList2 = new List<Vector3>();
            List<int> indList2 = new List<int>();
            pointCounter = 0;
            sprockTh = .7f;
            baseAl = .04f;
            pointAl = .001f;
            smallH = 3f;
            bigH = 15f;
            stepD = 360f / 24f * Mathf.PI / 180f;
            for (int tt = 0; tt < 24; tt++)
            {
                alpha = -tt * stepD;
                if (tt == 0) // local tick
                    retArr = Shapes.SprockTick(localR, -bigH, alpha, -(tt + 1) * stepD, -sprockTh, baseAl * 2, pointAl,
                        pointCounter, 0f);
                else
                    retArr = Shapes.SprockTick(localR, -smallH, alpha, -(tt + 1) * stepD, -sprockTh, baseAl, pointAl,
                        pointCounter, 0f);

                pointList2.AddRange((List<Vector3>) retArr[0]);
                indList2.AddRange((List<int>) retArr[1]);
                pointCounter = (int) retArr[2];
            }

            indList2.RemoveRange(0, 24);
            indList2.RemoveRange(indList2.Count - 6, 6);

            localWheelCont = new GameObject("LocalWheelCont");

            GameObject hSprock = Shapes.CreatePoly(pointList2, indList2, Color.white);
            hSprock.name = "HourSprocket";
            hSprock.transform.SetParent(localWheelCont.transform, false);

            GameObject whiteTri = Shapes.DrawTri(bigH, 7f, Color.white);
            whiteTri.name = "WhiteTriangle";
            whiteTri.transform.Translate(Vector3.forward * (localR));
            whiteTri.transform.Rotate(Vector3.forward, 180f);
            //whiteTri.transform.Translate(Vector3.up * .2f)
            whiteTri.transform.SetParent(localWheelCont.transform, false);

            GameObject redTri = Shapes.DrawTri(bigH * .7f, 3f, Color.red);
            redTri.name = "RedTriangle";
            redTri.transform.Translate(Vector3.forward * (localR));
            redTri.transform.Rotate(Vector3.forward, 180f);
            redTri.transform.Translate(Vector3.up * .2f);
            redTri.transform.SetParent(localWheelCont.transform, false);

            localWheelCont.transform.SetParent(earthSys.transform, false);

            // ........(3) Earth Sphere
            earthSph = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            earthSph.GetComponent<Renderer>().material =
                GameObject.FindGameObjectWithTag("SolarClock").GetComponent<SolarClock>().EarthMM;
            earthSph.name = "Earth";
            earthSph.transform.localScale = Vector3.one * 75;
            earthSph.transform.SetParent(earthSys.transform, false);

            //.........(4) Day Calendar
            //earth.addChild(Calendar.NewDayCalendar(earthR*.4))

            //.........(5) shade
            //var shade = new GameObject()
            //shade.addChild(Shapes.NewTrapazoid(earthR*.822,earthR*.418,20,.5,0x000000))
            //shade.addChild(Shapes.NewChord(earthR*.822,earthR*.185, .5, 0x000000))
            //shade.getChildAt(1).y = earthR * .418
            //shade.getChildAt(1).x =  -  shade.getChildAt(1).width / 2
            //shade.filters = [blur]
            //shade.alpha = .7
            //earth.addChild(shade)

            ////........(6) Moon;    
            GameObject moonDialGO = new GameObject("MoonDial");
            moonDialGO.transform.SetParent(earthSys.transform, false);
            moonDial = (Moon) moonDialGO.AddComponent<Moon>();
            moonDial.NewMoon(earthR * .45f, passDate);

            earthSys.transform.Translate(Vector3.forward * earthR * .5f);
            earthSys.transform.SetParent(transform, false);
        }

        public void RedrawStrip(System.DateTime passDate)
        {
            float prct = (360 - getLocalClockAlpha(passDate)) / 360f;
            Destroy(strip);
            strip = Shapes.DrawRing(
                localR - 1,
                localR - 3.5f,
                prct,
                new Color(1, 1, 1, .3f),
                0,
                false);
            strip.transform.SetParent(earthSprock.transform, false);
            strip.transform.localPosition = Vector3.zero;
            strip.transform.localRotation = Quaternion.identity;
            strip.transform.localScale = Vector3.one;
        }

        // TIME Functions;
        public static int GetTimeZone()
        {
            System.DateTime loc = System.DateTime.Now;
            System.TimeZone tz = System.TimeZone.CurrentTimeZone;
            System.TimeSpan ts = tz.GetUtcOffset(loc);

            return ts.Hours;
        }

        public static int GetDatelineDay(System.DateTime passUTC)
        {
            System.DateTime dateline = passUTC.AddHours(12);
            return System.Convert.ToInt32(dateline.DayOfWeek);
        }

        public static float getDiurnalPos(System.DateTime passDate)
        {
            //Debug.Log(-((System.Convert.ToSingle(passDate.Ticks - System.DateTime.MinValue.AddYears(System.DateTime.Now.Year - 1).Ticks)) / 10000f / 1000f / 60f / 60f / earthSidereal) * 360f );
            return -(System.Convert.ToSingle(passDate.Ticks -
                                             System.DateTime.MinValue.AddYears(System.DateTime.Now.Year - 1).Ticks) /
                     10000f / 1000f / 60f / 60f / earthSidereal) * 360 + 75;
        }

        public static float getLocalClockAlpha(System.DateTime passDate)
        {
            return (passDate.Hour * 60 * 60 + passDate.Minute * 60 + passDate.Second) /
                   (earthSynodic * 60 * 60) * 360;
        }

        public float getSynodicPos(System.DateTime passDate)
        {
            float time = (float) System.DateTime.Now.Subtract(System.DateTime.MinValue.AddYears(1969))
                .TotalMilliseconds;
            return -(time / 1000f / 60f / 60f / earthSynodic -
                     Mathf.Floor(time / 1000f / 60f / 60f / earthSynodic)) * 360 - 180 - 26;
        }
    }
}