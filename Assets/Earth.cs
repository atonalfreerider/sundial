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
        public float earthR;
        float localR;

        // persistent object
        public GameObject earthSys;
        public GameObject earthSph;
        public GameObject localWheelCont;
        public Moon moonDial;
        public TextBox day11;
        public TextBox day12;
        public GameObject intDatelineSplit;
        public TextBox day21;
        public TextBox day22;
        Circle strip;
        Polygon earthSprock;
        public SphereCollider earthSphereCollider;
        SphereCollider handSphereCollider;
        
        // INIT Functions
        public void NewEarthSystem(float passEarthR, System.DateTime passDate)
        {
            earthR = passEarthR;
            //...(0) Earth Line
            GameObject earthLineCont = new GameObject("EarthLineCont");
            earthLineCont.transform.SetParent(transform, false);

            handSphereCollider = gameObject.AddComponent<SphereCollider>();
            handSphereCollider.radius = 30;
            handSphereCollider.center = new Vector3(0, 0, earthR);

            float earthH = earthR * .99f;
            Polygon earthLine = PolygonFactory.DrawTri(
                earthH, 
                earthR * .05f,
                new Color(1, 1, 1, .3f));
            earthLine.name = "EarthLine";
            earthLine.transform.SetParent(earthLineCont.transform, false);
            
            // ...(1) Earth System
            earthSys = new GameObject("EarthSys");
            earthSys.transform.SetParent(transform, false);
            earthSys.transform.Translate(Vector3.forward * earthR * .5f);
            
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
            float hR = earthR * .4f;
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
                hLabel.transform.Translate(Vector3.up * (earthR * .4f - 10));
                if ((ht >= 0 && ht < 6) || ht > 18)
                {
                    hLabel.transform.Rotate(Vector3.forward, 180);
                }
            }
            hLabelWheel.transform.Rotate(Vector3.right * 90);

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
            moonDial = moonDialGO.AddComponent<Moon>();
            moonDial.NewMoon(earthR * .45f, passDate);
        }

        public void RedrawStrip(System.DateTime passDate)
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
            float woundAngle = -(System.Convert.ToSingle(passDate.Ticks -
                                             System.DateTime.MinValue.AddYears(System.DateTime.Now.Year - 1).Ticks) /
                     10000f / 1000f / 60f / 60f / earthSidereal) * 360 + 75;
            return Orbits.UnwindAngle(woundAngle);
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

        
        public Transform SelectionTarget => transform;
        public void Highlight()
        {
            throw new System.NotImplementedException();
        }

        public void Unhighlight()
        {
            throw new System.NotImplementedException();
        }

        public void RequestSelection()
        {
            if (SolarClock.Instance.viewState == SolarClock.ViewState.GeoCentric)
            {
                SolarClock.Instance.Toggle(SolarClock.ViewState.HelioCentric);
                earthSphereCollider.enabled = false;
            }
            else
            {
                SolarClock.Instance.isTracking = true;
            }
        }

        public void RequestDeselection()
        {
            
        }
    }
}