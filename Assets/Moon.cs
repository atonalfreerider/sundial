using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

namespace Assets
{
    public class Moon : MonoBehaviour
    {
        // calibration vars;
        public System.DateTime offsetDate;
        const float lunarSynodic = 29.531f;
        public float lunarSidereal = 27.321582f;
        float moonR;

        // persistent objects;
        public GameObject moonSys;
        public GameObject moon;
        public GameObject moonSprockCont;
        GameObject moonSprock;
        public GameObject moonLabels;
        Text month1L;
        GameObject monthSplitCont;
        Text month1L2;
        Text month2L;
        Text[] dayList;

        // INIT Functions;
        public void NewMoon(float passMoonR, System.DateTime passDate)
        {
            moonR = passMoonR;
            //...............(5) MOON;
            //................... (0) moon dial;
            moonSprockCont = NewMoonSprockCont(moonR, passDate);
            moonSprockCont.transform.SetParent(transform, false);
            moonSprockCont.name = "MoonSprockCont";

            //....................(1) Moon Sys;
            moonSys = new GameObject();
            moonSys.name = "MoonSys";

            //...........(0) Moon Hand;
            GameObject moonHandCont = new GameObject();
            moonHandCont.name = "MoonHandCont";
            float moonH = moonR * .95f;
            GameObject moonHand = Shapes.DrawTri(moonH, moonR * .1f, Color.white);
            moonHand.transform.parent = moonHandCont.transform;

            GameObject moonHand2 = Shapes.DrawTri(moonH * .85f, moonH * .05f, new Color(.5f, .5f, .5f));
            moonHand2.transform.Translate(Vector3.up * .2f);
            moonHand2.transform.parent = moonHandCont.transform;
            moonHandCont.transform.parent = moonSys.transform;

            //.........................(0) Moon;
            moon = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            //  moon = GameObject.FindGameObjectWithTag("SolarClock").GetComponent<SolarClock>().LowPolySphere;
            moon.name = "Moon";
            moon.GetComponent<Renderer>().material =
                GameObject.FindGameObjectWithTag("SolarClock").GetComponent<SolarClock>().MoonMat;
            float moonRad = 75f * .273f * .3f;
            moon.transform.localScale = new Vector3(moonRad, moonRad, moonRad);
            moon.transform.Translate(Vector3.forward * moonR);
            moon.transform.parent = moonSys.transform;

            moonSys.transform.SetParent(transform, false);
        }

        GameObject NewMoonSprockCont(float passMoonR, System.DateTime passDate)
        {
            moonR = passMoonR;
            GameObject newMoonSprockCont = new GameObject();
            //.........................................(0) circ;

            //...........................(0) hWheel;
            // point cloud for moon sprocket;
            List<Vector3> pointList = new List<Vector3>();
            List<int> indList = new List<int>();
            object[] retArr = new object[3];
            int pointCounter = 0;
            float alpha;
            float sprockTh = .7f;
            float baseAl = .007f;
            float pointAl = .001f;
            float smallH = 3f;
            float step = 360f / lunarSynodic * Mathf.PI / 180f;

            for (int tt = 0; tt < 29; tt++)
            {
                alpha = -tt * step;
                retArr = Shapes.SprockTick(moonR, smallH, alpha, -(tt + 1) * step, sprockTh, baseAl, pointAl,
                    pointCounter, 0f);
                pointList.AddRange((List<Vector3>) retArr[0]);
                indList.AddRange((List<int>) retArr[1]);
                pointCounter = (int) retArr[2];
            }

            indList.RemoveRange(indList.Count - 42, 42);

            moonSprock = Shapes.CreatePoly(pointList, indList, Color.white);
            moonSprock.name = "MoonSprock";
            moonSprock.transform.parent = newMoonSprockCont.transform;

            GameObject monthTick = Shapes.DrawTri(7f, 1f, Color.white);
            monthTick.transform.parent = moonSprock.transform;
            monthTick.transform.Translate(Vector3.forward * moonR);

            moonLabels = NewDayMonthLabels(passDate);
            moonLabels.name = "MoonLabels";
            moonLabels.transform.SetParent(newMoonSprockCont.transform);

            return newMoonSprockCont;
        }

        public void MoveMoonSprockCont(System.DateTime passDate, bool dayChange, bool monthChange, bool forward)
        {
            moonSprockCont.transform.rotation = Quaternion.Euler(0f,
                moonSys.transform.rotation.eulerAngles.y + passDate.Hour * 360f / (24f * lunarSynodic), 0f);

            if (dayChange)
            {
                // increment or decrement each day by 1;       
                int day;
                foreach (Text dL in dayList)
                {
                    if (forward)
                    {
                        // time is moving forward;
                        day = System.Convert.ToInt32(dL.text) + 1;
                        if (passDate.Month > 1)
                        {
                            // if not January;
                            if ((monthChange && day > System.DateTime.DaysInMonth(passDate.Year, passDate.Month - 1)) ||
                                (!monthChange && day > System.DateTime.DaysInMonth(passDate.Year, passDate.Month)))
                                day = 1; // month has changed over and current day in queue has exceeded next months num days || month has not changed and current day in queue is greater than current month's num days -> reset to 1; 
                        }
                        else
                        {
                            // January;
                            if ((monthChange && day > System.DateTime.DaysInMonth(passDate.Year - 1, 12)) ||
                                (!monthChange && day > System.DateTime.DaysInMonth(passDate.Year, passDate.Month)))
                                day = 1;
                        }
                    }
                    else
                    {
                        // time is moving backward;
                        day = System.Convert.ToInt32(dL.text) - 1;
                        if (day < 1)
                            day = System.DateTime.DaysInMonth(passDate.Year, passDate.Month);
                    }

                    dL.text = day.ToString();
                }
            }

            int daysUntilNextMonth = System.DateTime.DaysInMonth(passDate.Year, passDate.Month) - passDate.Day + 1;
            if (monthChange)
            {
                if (passDate.Month < 12)
                    daysUntilNextMonth = System.DateTime.DaysInMonth(passDate.Year, passDate.Month + 1) - passDate.Day;
                else
                    daysUntilNextMonth = System.DateTime.DaysInMonth(passDate.Year + 1, 1) - passDate.Day;

                // update text;
                month1L.text = Calendar.monthofYrAbr[passDate.Month - 1];
                month1L2.text = Calendar.monthofYrAbr[passDate.Month - 1];
                month2L.text = Calendar.monthofYrAbr[Calendar.ConvertMonth(passDate.Month)];
            }

            bool setOn = false;
            // detect if 1st is not in 1st pos;
            for (int ii = 0; ii < 29; ii++)
            {
                if (System.Convert.ToInt32(dayList[ii].text) == 1 && ii > 0)
                {
                    setOn = true;
                    break;
                }
            }

            monthSplitCont.SetActive(setOn);

            monthSplitCont.transform.rotation = Quaternion.Euler(0f,
                moonSprock.transform.rotation.eulerAngles.y - daysUntilNextMonth * 360f / lunarSynodic, 0f);
        }

        GameObject NewDayMonthLabels(System.DateTime passDate)
        {
            //...........................(1) hLabelWheel;
            // create days in month;        
            dayList = new Text[29];
            GameObject newDLabelWheel = new GameObject();
            Items.AddCanvas(newDLabelWheel);
            Text dLabel;
            int count = 0;
            int count2 = 0;
            int htr = 0;
            for (int ht = passDate.Day; ht <= System.DateTime.DaysInMonth(passDate.Year, passDate.Month); ht++)
            {
                dLabel = Items.NewText(ht.ToString(), Color.white, 28, TextAnchor.MiddleRight, false);
                dayList[count2] = dLabel;
                dLabel.transform.Rotate(Vector3.forward, (ht - passDate.Day) * (360f / lunarSynodic) + 2f);
                dLabel.transform.Translate(Vector3.up * (moonR - 6.5f));
                dLabel.transform.Rotate(Vector3.forward, 90f);
                dLabel.transform.SetParent(newDLabelWheel.transform);
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
                dLabel = Items.NewText(nextD.ToString(), Color.white, 28, TextAnchor.MiddleRight, false);
                dayList[count2] = dLabel;
                dLabel.transform.Rotate(Vector3.forward, (htr - passDate.Day) * (360f / lunarSynodic) + 2f);
                dLabel.transform.Translate(Vector3.up * (moonR - 6.5f));
                dLabel.transform.Rotate(Vector3.forward, 90f);
                dLabel.transform.SetParent(newDLabelWheel.transform);
                nextD++;
                count++;
                count2++;
                htr++;
            }

            month1L = Items.NewText("", Color.white, 28, TextAnchor.MiddleLeft, false);
            month1L.transform.Translate(Vector3.up * (moonR + 6f));
            month1L.transform.Translate(Vector3.right * -2f);
            month1L.transform.Rotate(Vector3.forward, 90f);
            month1L.transform.Rotate(Vector3.up, -3.5f);
            month1L.transform.SetParent(newDLabelWheel.transform);

            monthSplitCont = new GameObject();

            GameObject monthTick = Shapes.DrawTri(7f, 1f, Color.white);
            monthTick.transform.parent = monthSplitCont.transform;
            monthTick.transform.Translate(Vector3.forward * moonR);

            month1L2 = Items.NewText("", Color.white, 28, TextAnchor.MiddleLeft, false);
            month1L2.transform.Translate(Vector3.up * (moonR + 6f));
            month1L2.transform.Translate(Vector3.right * 2f);
            month1L2.transform.Rotate(Vector3.forward, 90f);
            month1L2.transform.Rotate(Vector3.up, 1f);
            month1L2.transform.SetParent(monthSplitCont.transform);

            month2L = Items.NewText("", Color.white, 28, TextAnchor.MiddleLeft, false);
            month2L.transform.Translate(Vector3.up * (moonR + 6f));
            month2L.transform.Translate(Vector3.right * -2f);
            month2L.transform.Rotate(Vector3.forward, 90f);
            month2L.transform.Rotate(Vector3.up, -3.5f);
            month2L.transform.SetParent(monthSplitCont.transform);

            monthSplitCont.transform.parent = newDLabelWheel.transform;

            return newDLabelWheel;
        }
    }
}