using System;
using UnityEngine;
using System.Collections.Generic;
using Assets.GraphicsUtil.Shapes;
using Assets.UI;
using Assets.UI.Text;
using TMPro;

namespace Assets
{
    public class Earth : MonoBehaviour, ISelectable
    {
        // calibration vars
        const float earthSidereal = 23.9344696f;
        const float earthSynodic = 24;
        float localR;

        // persistent object
        public GameObject earthSys;
        public GameObject earthSph;
        public GameObject localWheelCont;
        public Moon moonDial;
        public TextBox day11, day12, day21, day22;
        public GameObject intDatelineSplit;
        Circle strip;
        Polygon earthSprock;
        public SphereCollider earthSphereCollider;
        SphereCollider handSphereCollider;
        
        // state vars
        int currentINDLday, currentDay;
        
        // INIT Functions
        void Awake()
        {
            currentINDLday = GetDatelineDay(DateTime.UtcNow);
        }
        
        public void NewEarthSystem(DateTime passDate)
        {
            //...(0) Earth Line
            GameObject earthLineCont = new GameObject("EarthLineCont");
            earthLineCont.transform.SetParent(transform, false);

            handSphereCollider = gameObject.AddComponent<SphereCollider>();
            handSphereCollider.radius = SolarClock.SYSTEM_DIAMETER / 5f;
            handSphereCollider.center = new Vector3(0, 0, SolarClock.SYSTEM_DIAMETER);

            float earthH = SolarClock.SYSTEM_DIAMETER * .99f;
            Polygon earthLine = PolygonFactory.DrawTri(
                earthH, 
                SolarClock.SYSTEM_DIAMETER * .05f,
                new Color(1, 1, 1, .3f));
            earthLine.name = "EarthLine";
            earthLine.transform.SetParent(earthLineCont.transform, false);
            
            // ...(1) Earth System
            earthSys = new GameObject("EarthSys");
            earthSys.transform.SetParent(transform, false);
            earthSys.transform.Translate(Vector3.forward * SolarClock.SYSTEM_DIAMETER * .5f);
            
            // .........(1) Earth Sprocket;
            GameObject earthSprockCont = new GameObject("EarthSprockCont");
            earthSprockCont.transform.SetParent(earthSys.transform, false);

            // create point cloud for earth sprocket mesh
            List<Vector3> pointList = new List<Vector3>();
            List<int> indList = new List<int>();
            Circle.SprocketTick retArr;
            int pointCounter = 0;
            int counter = 0;
            float alpha;
            float sprockTh = 1f;
            float baseAl = .007f;
            float pointAl = .003f;
            float smallH = 3f;
            float bigH = 7f;
            float hR = SolarClock.SYSTEM_DIAMETER * .4f;
            float stepD = 360f / 96 * Mathf.PI / 180f;
            for (int tt = 0; tt < 24; tt++)
            {
                alpha = -counter * stepD;
                // create hour tick

                retArr = Circle.SprockTick(hR, bigH, alpha, -(counter + 1) * stepD, sprockTh, baseAl, pointAl,
                    pointCounter, 0);
                pointList.AddRange(retArr.pointList);
                indList.AddRange(retArr.indexList);
                pointCounter = retArr.pointCounter;
                counter++;
                for (int dd = 0; dd < 3; dd++)
                {
                    alpha = -counter * stepD;
                    // create 15min tick
                    retArr = Circle.SprockTick(hR, smallH, alpha, -(counter + 1) * stepD, sprockTh, baseAl, pointAl,
                        pointCounter, 0);
                    pointList.AddRange(retArr.pointList);
                    indList.AddRange(retArr.indexList);
                    pointCounter = retArr.pointCounter;
                    counter++;
                }
            }

            indList.RemoveRange(indList.Count - 6, 6);

            earthSprock = PolygonFactory.NewPoly(SolarClock.Instance.mainMat, false);
            earthSprock.Draw3DPoly(pointList.ToArray(), indList.ToArray());
            earthSprock.SetColor(Color.white);
            earthSprock.name = "EarthSprock";
            earthSprock.transform.SetParent(earthSprockCont.transform, false);

            //...........................(1) hLabelWheel
            // create 24 hour marks
            GameObject hLabelWheel = new GameObject("HourLabelWheel");
            hLabelWheel.transform.SetParent(earthSprockCont.transform, false);
            TextBox hLabel;
            for (int ht = 0; ht < 24; ht++)
            {
                hLabel = TextBox.Create(ht.ToString(), TextBox.FontType.MainFont, 40, TextAlignmentOptions.Center);
                hLabel.transform.SetParent(hLabelWheel.transform, false);
                hLabel.transform.Rotate(Vector3.forward, ht * (360f / 24f));
                hLabel.transform.Translate(Vector3.up * (SolarClock.SYSTEM_DIAMETER * .4f - 10));
                if ((ht >= 0 && ht < 6) || ht > 18)
                {
                    hLabel.transform.Rotate(Vector3.forward, 180);
                }
            }
            hLabelWheel.transform.Rotate(Vector3.right * 90);

            //...........................(3) dayDil
            localR = SolarClock.SYSTEM_DIAMETER * .28f;

            //Collections.ObjectModel.ReadOnlyCollection<TimeZoneInfo> zones = TimeZoneInfo.GetSystemTimeZones()
            //TimeZoneInfo dstZone = zones[0]

            // DateTime dsTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, dstZone)
            int datelineDay = GetDatelineDay(DateTime.UtcNow);
            int yesterday = datelineDay - 1;
            if (yesterday < 0)
                yesterday = 6;

            const float angOffset = 4.2f;
            day11 = TextBox.Create(Calendar.daysofweekAbr[yesterday], TextBox.FontType.MainFont, 30, TextAlignmentOptions.Center);
            day11.transform.SetParent(earthSprockCont.transform, false);
            day11.transform.Rotate(Vector3.up * (180f + angOffset));
            day11.transform.Translate(Vector3.forward * -(localR - 2.25f));
            day11.transform.Rotate(Vector3.right * 90);

            day21 = TextBox.Create(Calendar.daysofweekAbr[datelineDay], TextBox.FontType.MainFont, 30, TextAlignmentOptions.Center);
            day21.transform.SetParent(earthSprockCont.transform, false);
            day21.transform.Rotate(Vector3.up * (180f - angOffset));
            day21.transform.Translate(Vector3.forward * -(localR - 2.25f));
            day21.transform.Rotate(Vector3.right * 90);

            intDatelineSplit = new GameObject("IntDatelineSplit");
            intDatelineSplit.transform.SetParent(earthSprockCont.transform, false);

            day12 = TextBox.Create(Calendar.daysofweekAbr[yesterday], TextBox.FontType.MainFont, 30, TextAlignmentOptions.Center);
            day12.transform.SetParent(intDatelineSplit.transform, false);
            day12.transform.Rotate(Vector3.up * (180 - angOffset));
            day12.transform.Translate(Vector3.forward * -(localR - 2.25f));
            day12.transform.Rotate(Vector3.right * 90);

            day22 = TextBox.Create(Calendar.daysofweekAbr[datelineDay], TextBox.FontType.MainFont, 30, TextAlignmentOptions.Center);
            day22.transform.SetParent(intDatelineSplit.transform, false);
            day22.transform.Rotate(Vector3.up * (180 + angOffset));
            day22.transform.Translate(Vector3.forward * -(localR - 2.25f));
            day22.transform.Rotate(Vector3.right * 90);

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
                    retArr = Circle.SprockTick(localR, -bigH, alpha, -(tt + 1) * stepD, -sprockTh, baseAl * 2, pointAl,
                        pointCounter, 0f);
                else
                    retArr = Circle.SprockTick(localR, -smallH, alpha, -(tt + 1) * stepD, -sprockTh, baseAl, pointAl,
                        pointCounter, 0f);

                pointList2.AddRange(retArr.pointList);
                indList2.AddRange(retArr.indexList);
                pointCounter = retArr.pointCounter;
            }

            indList2.RemoveRange(0, 24);
            indList2.RemoveRange(indList2.Count - 6, 6);

            localWheelCont = new GameObject("LocalWheelCont");

            Polygon hSprock = PolygonFactory.NewPoly(SolarClock.Instance.mainMat, false);
            hSprock.Draw3DPoly(pointList2.ToArray(), indList2.ToArray());
            hSprock.SetColor(Color.white);
            
            hSprock.name = "HourSprocket";
            hSprock.transform.SetParent(localWheelCont.transform, false);

            Polygon whiteTri = PolygonFactory.DrawTri(bigH, 7f, Color.white);
            whiteTri.name = "WhiteTriangle";
            whiteTri.transform.Translate(Vector3.forward * (localR));
            whiteTri.transform.Rotate(Vector3.forward, 180f);
            //whiteTri.transform.Translate(Vector3.up * .2f)
            whiteTri.transform.SetParent(localWheelCont.transform, false);

            Polygon redTri = PolygonFactory.DrawTri(bigH * .7f, 3f, Color.red);
            redTri.name = "RedTriangle";
            redTri.transform.Translate(Vector3.forward * (localR));
            redTri.transform.Rotate(Vector3.forward, 180f);
            redTri.transform.Translate(Vector3.up * .2f);
            redTri.transform.SetParent(localWheelCont.transform, false);

            localWheelCont.transform.SetParent(earthSys.transform, false);

            // ........(3) Earth Sphere
            earthSph = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            earthSphereCollider = earthSph.GetComponent<SphereCollider>();
            earthSphereCollider.enabled = false;
            earthSph.GetComponent<Renderer>().material =
                GameObject.FindGameObjectWithTag("SolarClock").GetComponent<SolarClock>().EarthMM;
            earthSph.name = "Earth";
            earthSph.transform.localScale = Vector3.one * SolarClock.SYSTEM_DIAMETER * .5f;
            earthSph.transform.SetParent(earthSys.transform, false);

            ////........(6) Moon;    
            GameObject moonDialGO = new GameObject("MoonDial");
            moonDialGO.transform.SetParent(earthSys.transform, false);
            moonDial = moonDialGO.AddComponent<Moon>();
            moonDial.NewMoon(SolarClock.SYSTEM_DIAMETER * .45f, passDate);
        }

        public void SetEarthSystemOrbit(DateTime newDateUTC, DateTime newDateLocal)
        {
            // Earth System Orbit
            transform.rotation = Quaternion.AngleAxis(
                Orbits.GetEarthOrbitAngle(newDateUTC),
                Vector3.up);

            // reset Earth Sphere
            earthSph.transform.rotation = Quaternion.identity;
            // the polar axis of the earth is tilted
            earthSph.transform.Rotate(Vector3.right, -23.4f);
            // the Earth is rotated by the hours into the day
            earthSph.transform.Rotate(Vector3.up, getDiurnalPos(newDateUTC));

            // Clock Sprocket - reverse rotated from the diurnal position of the Earth
            localWheelCont.transform.localRotation =
                Quaternion.Euler(new Vector3(
                    180,
                    -getLocalClockAlpha(newDateLocal) + 180,
                    0));
            
            intDatelineSplit.transform.rotation =
                Quaternion.AngleAxis(
                    earthSph.transform.rotation.eulerAngles.y - 90,
                    Vector3.up);

            int datelineDay = GetDatelineDay(newDateUTC);
            if (datelineDay != currentINDLday)
            {
                int yesterday = datelineDay - 1;
                if (yesterday < 0)
                    yesterday = 6;

                day11.Text = Calendar.daysofweekAbr[yesterday];
                day12.Text = Calendar.daysofweekAbr[yesterday];

                day21.Text = Calendar.daysofweekAbr[datelineDay];
                day22.Text = Calendar.daysofweekAbr[datelineDay];

                currentINDLday = datelineDay;
            }
            
            if (currentDay != newDateLocal.Day)
            {
                currentDay = newDateLocal.Day;

                // update dayCal
                if (SolarClock.Instance.calCreated)
                {
                    SolarClock.Instance.calendar.DrawDayCalendar(newDateLocal, earthSys.transform);
                }
            }

            RedrawStrip(newDateUTC.AddHours(12));

            moonDial.SetMoonOrbit(newDateUTC, newDateLocal);
        }
        
        void RedrawStrip(DateTime passDate)
        {
            // for some reason, 3 of these are getting created
            float prct = (360 - getLocalClockAlpha(passDate)) / 360f;
            if (strip != null)
            {
                Destroy(strip.gameObject);
            }

            strip = PolygonFactory.NewCirclePoly(SolarClock.Instance.mainMat);
            strip.DrawRing(
                localR - 1,
                localR - 3.5f,
                prct,
                0,
                0,
                false);
            strip.SetColor(new Color(1, 1, 1, .3f));
            strip.transform.SetParent(earthSprock.transform, false);
        }

        // TIME Functions
        public static int GetTimeZone()
        {
            DateTime loc = DateTime.Now;
            TimeZone tz = TimeZone.CurrentTimeZone;
            TimeSpan ts = tz.GetUtcOffset(loc);

            return ts.Hours;
        }

        static int GetDatelineDay(DateTime passUTC)
        {
            DateTime dateline = passUTC.AddHours(12);
            return Convert.ToInt32(dateline.DayOfWeek);
        }

        static float getDiurnalPos(DateTime passDate)
        {
            //Debug.Log(-((Convert.ToSingle(passDate.Ticks - DateTime.MinValue.AddYears(DateTime.Now.Year - 1).Ticks)) / 10000f / 1000f / 60f / 60f / earthSidereal) * 360f );
            float woundAngle = -(Convert.ToSingle(passDate.Ticks -
                                             DateTime.MinValue.AddYears(DateTime.Now.Year - 1).Ticks) /
                     10000f / 1000f / 60f / 60f / earthSidereal) * 360 + 75;
            return Orbits.UnwindAngle(woundAngle);
        }

        static float getLocalClockAlpha(DateTime passDate)
        {
            return (passDate.Hour * 60 * 60 + passDate.Minute * 60 + passDate.Second) /
                   (earthSynodic * 60 * 60) * 360;
        }

        static float getSynodicPos(DateTime passDate)
        {
            float time = (float) DateTime.Now.Subtract(DateTime.MinValue.AddYears(1969))
                .TotalMilliseconds;
            return -(time / 1000f / 60f / 60f / earthSynodic -
                     Mathf.Floor(time / 1000f / 60f / 60f / earthSynodic)) * 360 - 180 - 26;
        }
        
        #region ISelectable
        
        public Transform SelectionTarget => transform;

        public void RequestSelection()
        {
            if (SolarClock.Instance.viewState == SolarClock.ViewState.GeoCentric)
            {
                SolarClock.Instance.Toggle(SolarClock.ViewState.HelioCentric);
                earthSphereCollider.enabled = false;
            }
            else
            {
                SolarClock.Instance.solarTime.isEarthTracking = true;
            }
        }

        #endregion
    }
}