using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Assets.GraphicsUtil.Shapes;
using Assets.UI;
using Assets.UI.Text;
using TMPro;

namespace Assets
{
    public class Moon : MonoBehaviour, ISelectable
    {
        // calibration vars
        public DateTime offsetDate;
        const float lunarSynodic = 29.531f;
        public const float lunarSidereal = 27.321582f;
        float moonR;

        // persistent objects
        public GameObject moonSys;
        public GameObject moon;
        public GameObject moonSprockCont;
        Circle moonSprock;
        public GameObject moonLabels;
        TextBox month1L, month1L2, month2L;
        GameObject monthSplitCont;
        TextBox[] dayList;

        // INIT Functions
        public void NewMoon(float passMoonR, DateTime passDate)
        {
            moonR = passMoonR;
            //...............(5) MOON
            //................... (0) moon dial
            moonSprockCont = NewMoonSprockCont(moonR, passDate);
            moonSprockCont.transform.SetParent(transform, false);

            //....................(1) Moon Sys
            moonSys = new GameObject("MoonSys");

            //...........(0) Moon Hand
            GameObject moonHandCont = new GameObject("MoonHandCont");
            float moonH = moonR * .95f;
            Polygon moonHand = PolygonFactory.DrawTri(moonH, moonR * .1f, Color.white);
            moonHand.transform.parent = moonHandCont.transform;

            Polygon moonHand2 = PolygonFactory.DrawTri(moonH * .85f, moonH * .05f, new Color(.5f, .5f, .5f));
            moonHand2.transform.Translate(Vector3.up * .2f);
            moonHand2.transform.parent = moonHandCont.transform;
            moonHandCont.transform.parent = moonSys.transform;

            //.........................(0) Moon
            moon = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            SphereCollider moonCollider = moon.GetComponent<SphereCollider>();
            moonCollider.radius = SolarClock.SYSTEM_DIAMETER / 75f;
            
            // TODO if we want an eclipse this can get set back to Default
            moon.layer = LayerMask.NameToLayer("TransparentFX");
            
            moon.name = "Moon";
            moon.GetComponent<Renderer>().material = SolarClock.Instance.MoonMat;
            const float moonRad = SolarClock.SYSTEM_DIAMETER * .04095f;
            moon.transform.localScale = Vector3.one * moonRad;
            moon.transform.Translate(Vector3.forward * moonR);
            moon.transform.parent = moonSys.transform;

            moonSys.transform.SetParent(transform, false);
        }

        GameObject NewMoonSprockCont(float passMoonR, DateTime passDate)
        {
            moonR = passMoonR;
            GameObject newMoonSprockCont = new GameObject("MoonSprockCont");
            //.........................................(0) circ

            //...........................(0) hWheel
            // point cloud for moon sprocket
            moonSprock = PolygonFactory.NewCirclePoly(SolarClock.Instance.mainMat);
            moonSprock.DrawSprocket(moonR, 29, 0, 1, 
                SolarClock.SYSTEM_DIAMETER * .00466f, .007f, .001f,
                0, SolarClock.SYSTEM_DIAMETER * .02f, 
                lunarSynodic, 42);
            moonSprock.SetColor(Color.white);
            moonSprock.name = "MoonSprock";
            moonSprock.transform.parent = newMoonSprockCont.transform;

            Polygon monthTick = PolygonFactory.DrawTri(7, 1, Color.white);
            monthTick.transform.parent = moonSprock.transform;
            monthTick.transform.Translate(Vector3.forward * moonR);

            moonLabels = NewDayMonthLabels();
            moonLabels.transform.SetParent(newMoonSprockCont.transform, false);
            moonLabels.transform.Rotate(Vector3.right * 90);

            return newMoonSprockCont;
        }

        void MoveMoonSprockCont(DateTime passDate)
        {
            moonSprockCont.transform.rotation = Quaternion.AngleAxis(
                moonSys.transform.rotation.eulerAngles.y +
                // retreat moon dial by number of hours into current day
                passDate.Hour * 360 / (24 * lunarSynodic),
                Vector3.up);

            int thisDay = passDate.Day;
            int thisMonth = passDate.Month;
            int thisYear = passDate.Year;

            // set each day 
            foreach (TextBox dL in dayList)
            {
                dL.Text = thisDay.ToString();
                thisDay++;

                if (dayList.Last() == dL) break;
                // only test for changes if we're still mid-list

                if (thisDay > DateTime.DaysInMonth(thisYear, thisMonth))
                {
                    // month has changed and current day in queue is greater than current month's num days -> reset to 1;
                    thisDay = 1;
                    thisMonth++;
                    if (thisMonth > 12)
                    {
                        thisMonth = 1;
                        thisYear++;
                    }
                }
            }

            // update text
            month1L.Text = Calendar.monthofYrAbr[passDate.Month - 1];
            month1L2.Text = Calendar.monthofYrAbr[passDate.Month - 1];
            month2L.Text = Calendar.monthofYrAbr[Calendar.ConvertMonth(passDate.Month)];

            monthSplitCont.SetActive(thisMonth != passDate.Month);

            int daysUntilNextMonth = DateTime.DaysInMonth(passDate.Year, passDate.Month) - passDate.Day + 1;
            monthSplitCont.transform.rotation = Quaternion.AngleAxis(
                moonSprock.transform.rotation.eulerAngles.y - daysUntilNextMonth * 360 / lunarSynodic,
                Vector3.up);
        }

        GameObject NewDayMonthLabels()
        {
            //...........................(1) hLabelWheel
            // create days in month;        
            dayList = new TextBox[29];
            GameObject newDLabelWheel = new GameObject("MoonLabels");
            const float dayLabelPad = SolarClock.SYSTEM_DIAMETER * .0133f;;
            for (int ht = 0; ht < dayList.Length; ht++)
            {
                TextBox dLabel = TextBox.Create("", TextBox.FontType.MainFont, 28, TextAlignmentOptions.Right);
                dLabel.transform.SetParent(newDLabelWheel.transform, false);
                dayList[ht] = dLabel;
                dLabel.transform.Rotate(Vector3.forward, (ht) * (360 / lunarSynodic) + 2);
                dLabel.transform.Translate(Vector3.up * (moonR - dayLabelPad));
                dLabel.transform.Rotate(Vector3.forward, 90);
            }

            month1L = TextBox.Create("", TextBox.FontType.MainFont, 28,
                TextAlignmentOptions.Left);
            month1L.transform.SetParent(newDLabelWheel.transform, false);
            month1L.transform.Translate(Vector3.up * (moonR + dayLabelPad));
            month1L.transform.Translate(Vector3.left * SolarClock.SYSTEM_DIAMETER * .0133f);
            month1L.transform.Rotate(Vector3.forward, 90);
            month1L.transform.Rotate(Vector3.up, -3.5f);

            monthSplitCont = new GameObject("month split contain");
            monthSplitCont.transform.SetParent(newDLabelWheel.transform, false);

            Polygon monthTick = PolygonFactory.DrawTri(7, SolarClock.SYSTEM_DIAMETER / 150f, Color.white);
            monthTick.transform.SetParent(monthSplitCont.transform, false);
            monthTick.transform.Translate(Vector3.forward * moonR);

            month1L2 = TextBox.Create("", TextBox.FontType.MainFont, 28,
                TextAlignmentOptions.Left);
            month1L2.transform.SetParent(monthSplitCont.transform, false);
            month1L2.transform.Rotate(Vector3.right * 90);
            month1L2.transform.Translate(Vector3.up * (moonR + 1));
            month1L2.transform.Translate(Vector3.right * SolarClock.SYSTEM_DIAMETER * .0133f);
            month1L2.transform.Rotate(Vector3.forward * 90);
            month1L2.transform.Rotate(Vector3.up, -1);

            month2L = TextBox.Create("",
                TextBox.FontType.MainFont, 28, TextAlignmentOptions.Left);
            month2L.transform.SetParent(monthSplitCont.transform, false);
            month2L.transform.Rotate(Vector3.right * 90);
            month2L.transform.Translate(Vector3.up * (moonR + 1));
            month2L.transform.Translate(Vector3.left * SolarClock.SYSTEM_DIAMETER * .0133f);
            month2L.transform.Rotate(Vector3.forward * 90);
            month2L.transform.Rotate(Vector3.up, 5);

            return newDLabelWheel;
        }

        public void SetMoonOrbit(DateTime newDateUTC, DateTime newDateLocal)
        {
            // Moon Orbit
            moonSys.transform.rotation = Quaternion.AngleAxis(
                Orbits.GetNonEarthOrbitAngle(newDateUTC, lunarSidereal, 60),
                Vector3.up);

            MoveMoonSprockCont(newDateLocal);
        }

        #region ISelectable

        public Transform SelectionTarget { get; }

        public void RequestSelection()
        {
            SolarClock.Instance.solarTime.isMoonTracking = true;
        }

        #endregion
    }
}