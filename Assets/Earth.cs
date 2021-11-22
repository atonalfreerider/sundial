using System;
using UnityEngine;
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
        Circle earthSprock;
        public SphereCollider earthSphereCollider;
        public SphereCollider handSphereCollider;
        
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
            GameObject earthLineCont = new("EarthLineCont");
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
            GameObject earthSprockCont = new("EarthSprockCont");
            earthSprockCont.transform.SetParent(earthSys.transform, false);

            // create point cloud for earth sprocket mesh
            earthSprock = PolygonFactory.NewCirclePoly(SolarClock.Instance.mainMat);
            earthSprock.DrawSprocket(SolarClock.SYSTEM_DIAMETER * .4f, 24, 3, 1,
                SolarClock.SYSTEM_DIAMETER / 150f,  .007f, .003f,
                SolarClock.SYSTEM_DIAMETER * .02f, SolarClock.SYSTEM_DIAMETER * .0466f);
           
            earthSprock.SetColor(Color.white);
            earthSprock.name = "EarthSprock";
            earthSprock.transform.SetParent(earthSprockCont.transform, false);

            //...........................(1) hLabelWheel
            // create 24 hour marks
            GameObject hLabelWheel = new("HourLabelWheel");
            hLabelWheel.transform.SetParent(earthSprockCont.transform, false);
            TextBox hLabel;
            for (int ht = 0; ht < 24; ht++)
            {
                hLabel = TextBox.Create(ht.ToString(), TextBox.FontType.MainFont, 40, TextAlignmentOptions.Center);
                hLabel.transform.SetParent(hLabelWheel.transform, false);
                hLabel.transform.Rotate(Vector3.forward, ht * (360f / 24f));
                hLabel.transform.Translate(Vector3.up * (SolarClock.SYSTEM_DIAMETER * .4f - SolarClock.SYSTEM_DIAMETER * .066f));
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
            const float padDay = SolarClock.SYSTEM_DIAMETER * .015f;
            day11 = TextBox.Create(Calendar.daysofweekAbr[yesterday], TextBox.FontType.MainFont, 30, TextAlignmentOptions.Center);
            day11.transform.SetParent(earthSprockCont.transform, false);
            day11.transform.Rotate(Vector3.up * (180 + angOffset));
            day11.transform.Translate(Vector3.forward * -(localR - padDay));
            day11.transform.Rotate(Vector3.right * 90);

            day21 = TextBox.Create(Calendar.daysofweekAbr[datelineDay], TextBox.FontType.MainFont, 30, TextAlignmentOptions.Center);
            day21.transform.SetParent(earthSprockCont.transform, false);
            day21.transform.Rotate(Vector3.up * (180 - angOffset));
            day21.transform.Translate(Vector3.forward * -(localR - padDay));
            day21.transform.Rotate(Vector3.right * 90);

            intDatelineSplit = new GameObject("IntDatelineSplit");
            intDatelineSplit.transform.SetParent(earthSprockCont.transform, false);

            day12 = TextBox.Create(Calendar.daysofweekAbr[yesterday], TextBox.FontType.MainFont, 30, TextAlignmentOptions.Center);
            day12.transform.SetParent(intDatelineSplit.transform, false);
            day12.transform.Rotate(Vector3.up * (180 - angOffset));
            day12.transform.Translate(Vector3.forward * -(localR - padDay));
            day12.transform.Rotate(Vector3.right * 90);

            day22 = TextBox.Create(Calendar.daysofweekAbr[datelineDay], TextBox.FontType.MainFont, 30, TextAlignmentOptions.Center);
            day22.transform.SetParent(intDatelineSplit.transform, false);
            day22.transform.Rotate(Vector3.up * (180 + angOffset));
            day22.transform.Translate(Vector3.forward * -(localR - padDay));
            day22.transform.Rotate(Vector3.right * 90);

            //........ (2) local wheel
            localWheelCont = new GameObject("LocalWheelCont");
            const float bigH = SolarClock.SYSTEM_DIAMETER * .1f;
            
            Circle hSprock = PolygonFactory.NewCirclePoly(SolarClock.Instance.mainMat);
            hSprock.DrawSprocket(localR, 1, 23, -1,
                SolarClock.SYSTEM_DIAMETER * .0046f, .04f, .001f,
                SolarClock.SYSTEM_DIAMETER * .02f,  bigH);
            hSprock.SetColor(Color.white);
            hSprock.name = "HourSprocket";
            hSprock.transform.SetParent(localWheelCont.transform, false);

            Polygon redTri = PolygonFactory.DrawTri(bigH * .7f, SolarClock.SYSTEM_DIAMETER * .015f, Color.red);
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
            earthSph.GetComponent<Renderer>().material = SolarClock.Instance.EarthMM;
            earthSph.name = "Earth";
            earthSph.transform.localScale = Vector3.one * SolarClock.SYSTEM_DIAMETER * .5f;
            earthSph.transform.SetParent(earthSys.transform, false);

            ////........(6) Moon;    
            GameObject moonDialGO = new("MoonDial");
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
            float prct = (360 - getLocalClockAlpha(passDate)) / 360f;
            if (strip != null)
            {
                Destroy(strip.gameObject);
            }

            strip = PolygonFactory.NewCirclePoly(SolarClock.Instance.mainMat);
            strip.DrawRing(
                localR - SolarClock.SYSTEM_DIAMETER / 150f,
                localR - SolarClock.SYSTEM_DIAMETER * .0233f,
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