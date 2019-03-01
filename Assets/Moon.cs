using System;
using UnityEngine;
using System.Collections.Generic;
using Assets.GraphicsUtil.Shapes;
using Assets.UI;
using Assets.UI.Text;
using TMPro;

namespace Assets
{
    public class Moon : MonoBehaviour, ISelectable
    {
        // calibration vars
        public System.DateTime offsetDate;
        const float lunarSynodic = 29.531f;
        public const float lunarSidereal = 27.321582f;
        float moonR;

        // persistent objects
        public GameObject moonSys;
        public GameObject moon;
        public GameObject moonSprockCont;
        Polygon moonSprock;
        public GameObject moonLabels;
        TextBox month1L;
        GameObject monthSplitCont;
        TextBox month1L2;
        TextBox month2L;
        TextBox[] dayList;

        // INIT Functions
        public void NewMoon(float passMoonR, System.DateTime passDate)
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
            moon.GetComponent<SphereCollider>().radius = 2;
            moon.name = "Moon";
            moon.GetComponent<Renderer>().material =
                GameObject.FindGameObjectWithTag("SolarClock").GetComponent<SolarClock>().MoonMat;
            const float moonRad = 75 * .273f * .3f;
            moon.transform.localScale = new Vector3(moonRad, moonRad, moonRad);
            moon.transform.Translate(Vector3.forward * moonR);
            moon.transform.parent = moonSys.transform;

            moonSys.transform.SetParent(transform, false);
        }

        GameObject NewMoonSprockCont(float passMoonR, System.DateTime passDate)
        {
            moonR = passMoonR;
            GameObject newMoonSprockCont = new GameObject("MoonSprockCont");
            //.........................................(0) circ

            //...........................(0) hWheel
            // point cloud for moon sprocket
            List<Vector3> pointList = new List<Vector3>();
            List<int> indList = new List<int>();
            Circle.SprocketTick retArr;
            int pointCounter = 0;
            float alpha;
            float sprockTh = .7f;
            float baseAl = .007f;
            float pointAl = .001f;
            float smallH = 3;
            float step = 360 / lunarSynodic * Mathf.PI / 180f;

            for (int tt = 0; tt < 29; tt++)
            {
                alpha = -tt * step;
                retArr = Circle.SprockTick(moonR, smallH, alpha, -(tt + 1) * step, sprockTh, baseAl, pointAl,
                    pointCounter, 0);
                pointList.AddRange(retArr.pointList);
                indList.AddRange(retArr.indexList);
                pointCounter = retArr.pointCounter;
            }

            indList.RemoveRange(indList.Count - 42, 42);

            moonSprock = PolygonFactory.NewPoly(SolarClock.Instance.mainMat, false);
            moonSprock.Draw3DPoly(pointList.ToArray(), indList.ToArray());
            moonSprock.SetColor(Color.white);
            moonSprock.name = "MoonSprock";
            moonSprock.transform.parent = newMoonSprockCont.transform;

            Polygon monthTick = PolygonFactory.DrawTri(7, 1, Color.white);
            monthTick.transform.parent = moonSprock.transform;
            monthTick.transform.Translate(Vector3.forward * moonR);

            moonLabels = NewDayMonthLabels(passDate);
            moonLabels.transform.SetParent(newMoonSprockCont.transform, false);
            moonLabels.transform.Rotate(Vector3.right * 90);

            return newMoonSprockCont;
        }

        public void MoveMoonSprockCont(System.DateTime passDate, bool dayChange, bool monthChange)
        {
            moonSprockCont.transform.rotation = Quaternion.AngleAxis(
                moonSys.transform.rotation.eulerAngles.y + passDate.Hour * 360 / (24 * lunarSynodic),
                Vector3.up);

            if (dayChange)
            {
                // increment or decrement each day by 1 
                int day;
                foreach (TextBox dL in dayList)
                {
                    if (true)
                    {
                        // time is moving forward
                        day = dL.SpecialInt + 1;
                        if (passDate.Month > 1)
                        {
                            // if not January
                            if (monthChange && day > System.DateTime.DaysInMonth(passDate.Year, passDate.Month - 1) ||
                                !monthChange && day > System.DateTime.DaysInMonth(passDate.Year, passDate.Month))
                            {
                                day = 1; // month has changed over and current day in queue has exceeded next months num days || month has not changed and current day in queue is greater than current month's num days -> reset to 1;
                            }
                        }
                        else
                        {
                            // January
                            if (monthChange && day > System.DateTime.DaysInMonth(passDate.Year - 1, 12) ||
                                !monthChange && day > System.DateTime.DaysInMonth(passDate.Year, passDate.Month))
                            {
                                day = 1;
                            }
                        }
                    }
                    else
                    {
                        // time is moving backward
                        day = System.Convert.ToInt32(dL.Text) - 1;
                        if (day < 1)
                        {
                            day = System.DateTime.DaysInMonth(passDate.Year, passDate.Month);
                        }
                    }

                    dL.Text = day.ToString();
                    dL.SpecialInt = day;
                }
            }

            int daysUntilNextMonth = System.DateTime.DaysInMonth(passDate.Year, passDate.Month) - passDate.Day + 1;
            if (monthChange)
            {
                if (passDate.Month < 12)
                {
                    daysUntilNextMonth = System.DateTime.DaysInMonth(passDate.Year, passDate.Month + 1) - passDate.Day;
                }
                else
                {
                    daysUntilNextMonth = System.DateTime.DaysInMonth(passDate.Year + 1, 1) - passDate.Day;
                }

                // update text
                month1L.Text = Calendar.monthofYrAbr[passDate.Month - 1];
                month1L2.Text = Calendar.monthofYrAbr[passDate.Month - 1];
                month2L.Text = Calendar.monthofYrAbr[Calendar.ConvertMonth(passDate.Month)];
            }

            bool setOn = false;
            // detect if 1st is not in 1st pos
            for (int ii = 0; ii < 29; ii++)
            {
                if (dayList[ii].SpecialInt != 1 || ii <= 0) continue;
                setOn = true;
                break;
            }

            monthSplitCont.SetActive(setOn);

            monthSplitCont.transform.rotation = Quaternion.AngleAxis(
                moonSprock.transform.rotation.eulerAngles.y - daysUntilNextMonth * 360 / lunarSynodic,
                Vector3.up);
        }

        GameObject NewDayMonthLabels(System.DateTime passDate)
        {
            //...........................(1) hLabelWheel
            // create days in month;        
            dayList = new TextBox[29];
            GameObject newDLabelWheel = new GameObject("MoonLabels");
            TextBox dLabel;
            int count = 0;
            int count2 = 0;
            int htr = 0;
            const float dayLabelPad = 2;
            for (int ht = passDate.Day; ht <= System.DateTime.DaysInMonth(passDate.Year, passDate.Month); ht++)
            {
                dLabel = TextBox.Create(ht.ToString(), TextBox.FontType.MainFont, 28, TextAlignmentOptions.Right);
                dLabel.SpecialInt = ht;
                dLabel.transform.SetParent(newDLabelWheel.transform, false);
                dayList[count2] = dLabel;
                dLabel.transform.Rotate(Vector3.forward, (ht - passDate.Day) * (360 / lunarSynodic) + 2);
                dLabel.transform.Translate(Vector3.up * (moonR - dayLabelPad));
                dLabel.transform.Rotate(Vector3.forward, 90);
                count++;
                count2++;
                htr = ht;
                if (count > 28)
                    break;
            }

            int nextD = 1;
            htr++;
            while (count < 29)
            {
                dLabel = TextBox.Create(nextD.ToString(), TextBox.FontType.MainFont, 28, TextAlignmentOptions.Right);
                dLabel.SpecialInt = nextD;
                dLabel.transform.SetParent(newDLabelWheel.transform, false);
                dayList[count2] = dLabel;
                dLabel.transform.Rotate(Vector3.forward, (htr - passDate.Day) * (360 / lunarSynodic) + 2);
                dLabel.transform.Translate(Vector3.up * (moonR - dayLabelPad));
                dLabel.transform.Rotate(Vector3.forward, 90);
                nextD++;
                count++;
                count2++;
                htr++;
            }

            month1L = TextBox.Create(Calendar.monthofYrAbr[passDate.Month - 1], TextBox.FontType.MainFont, 28, TextAlignmentOptions.Left);
            month1L.transform.SetParent(newDLabelWheel.transform, false);
            month1L.transform.Translate(Vector3.up * (moonR + dayLabelPad));
            month1L.transform.Translate(Vector3.left * 2);
            month1L.transform.Rotate(Vector3.forward, 90);
            month1L.transform.Rotate(Vector3.up, -3.5f);

            monthSplitCont = new GameObject("month split contain");
            monthSplitCont.transform.SetParent(newDLabelWheel.transform, false);

            Polygon monthTick = PolygonFactory.DrawTri(7, 1, Color.white);
            monthTick.transform.SetParent(monthSplitCont.transform, false);
            monthTick.transform.Translate(Vector3.forward * moonR);

            month1L2 = TextBox.Create(Calendar.monthofYrAbr[passDate.Month - 1], TextBox.FontType.MainFont , 28, TextAlignmentOptions.Left);
            month1L2.transform.SetParent(monthSplitCont.transform, false);
            month1L2.transform.Rotate(Vector3.right * 90);
            month1L2.transform.Translate(Vector3.up * (moonR + 1));
            month1L2.transform.Translate(Vector3.right * 2);
            month1L2.transform.Rotate(Vector3.forward * 90);
            month1L2.transform.Rotate(Vector3.up, -1);
            
            month2L = TextBox.Create(Calendar.monthofYrAbr[Calendar.ConvertMonth(passDate.Month)], TextBox.FontType.MainFont, 28, TextAlignmentOptions.Left);
            month2L.transform.SetParent(monthSplitCont.transform, false);
            month2L.transform.Rotate(Vector3.right * 90);
            month2L.transform.Translate(Vector3.up * (moonR + 1));
            month2L.transform.Translate(Vector3.left * 2);
            month2L.transform.Rotate(Vector3.forward * 90);
            month2L.transform.Rotate(Vector3.up, 5f);

            return newDLabelWheel;
        }

        public Transform SelectionTarget { get; }
        public void Highlight()
        {
            throw new NotImplementedException();
        }

        public void Unhighlight()
        {
            throw new NotImplementedException();
        }

        public void RequestSelection()
        {
            SolarClock.Instance.solarTime.isMoonTracking = true;
        }

        public void RequestDeselection()
        {
            throw new NotImplementedException();
        }
    }
}