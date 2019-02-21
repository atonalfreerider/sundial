using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

namespace Assets
{
    public class SolarClock : MonoBehaviour
    {
        enum ViewState
        {
            Galactic,
            HelioCentric,
            GeoCentric,
        }

        // calibration vars
        const float YEAR = 365.256363004f;
        const float sysDia = 150;
        float earthScale = .001f;
        const float earthLineL = 400;
        const float SiderealDayInSeconds = 86164.0905f;

        // text vars
        public Font mainFont;
        public Shader mainShader;
        public Material EarthMM, MoonMat;

        // persistent objects
        Earth earth;
        Orbits orbits;
        GameObject sunSprockCont, sunSprock, mLabelWheel, sunLine;
        Text summerText, springText;
        Calendar calendar;

        public DigitalClock digiClock;
        readonly GameObject[] seasonLabels = new GameObject[4];
        readonly Text[] yearQueue = new Text[3];

        // time initialization
        System.DateTime travelDateUTC = System.DateTime.UtcNow;
        System.DateTime travelDateLocal = System.DateTime.Now;

        // camera vars
        ViewState viewState = ViewState.HelioCentric;
        const float solCamY = 270;
        const float earthCamY = 135;
        Vector3 targetPos = new Vector3(0, solCamY, 0);
        Quaternion targetRot = Quaternion.AngleAxis(90, Vector3.right);
        Vector3 orbitScale = new Vector3(1, .01f, 1);
        float orthoSize;
#if UNITY_EDITOR
        const float solOrthoSize = sysDia;
        const float earthOrthoSize = sysDia * .5f + 2;
#else
        // Viewer for Galaxy S8
        const float solOrthoSize = 320;
        const float earthOrthoSize = 125;
#endif
        Coroutine zooming;

        // light vars
        public Light ptLight, dirLight;
        float ptInt = .5f;

        // state vars
        bool minuteFound = false;
        bool secondFound = false;
        bool calCreated = false;
        //bool newDayFound = false
        int currentDay, currentMonth, currentYear, currentTimeZone, currentINDL, dst;
        int spinInc = 0;
        bool showNow = true;
        System.DateTime jan1ofthisYear;

        bool forward = true;
        bool labelUp = false;

        Coroutine minuteUpdate;
        Coroutine secondUpdate;

        // INIT Functions
        void Awake()
        {
            QualitySettings.antiAliasing = 4;
            Shapes.Init(mainShader);
            Items.Init(mainFont);
            orthoSize = solOrthoSize;

            /*
            // time testing
            System.DateTime test1 = new System.DateTime()
            test1 = test1.AddYears(2014)
            System.DateTime test2 = new System.DateTime()
            test2 = test2.AddYears(2014)
            test2 = test2.AddHours(-11)
            test1 = test1.AddDays(200)
            test1 = test1.AddHours(5)
   
            //Switch between test and real time
            System.DateTime date1 = test1
            System.DateTime date2 = test2
            */

            System.DateTime date1 = System.DateTime.UtcNow;
            System.DateTime date2 = System.DateTime.Now;

            //  Debug.Log(date1)
            //  Debug.Log(date2)

            // store time values to check for days/year/timezone switch
            jan1ofthisYear = new System.DateTime();
            jan1ofthisYear = jan1ofthisYear.AddYears(date2.Year - 1);
            currentDay = date2.Day;
            currentMonth = date2.Month;
            currentYear = date2.Year;
            currentTimeZone = Earth.GetTimeZone();
            currentINDL = Earth.GetDatelineDay(date1);
            dst = 0;
            if (date2.IsDaylightSavingTime())
            {
                dst = 1;
            }

            EarthMM.SetTextureOffset("_DetailAlbedoMap",
                new Vector2(((float) (12 - currentTimeZone - dst) + .5f) / 24f, 0));

            // create new SolarClock and set celestial positions
            gameObject.name = "SolarClock";
            NewSolarClock(sysDia, date2);

            SetOrbit(date1, date2);
            earth.moonDial.MoveMoonSprockCont(date2, false, false, forward);
            // move 11 days past winter EQUINOX + local hour difference
            sunSprockCont.transform.localRotation = Quaternion.AngleAxis(SunSprockOffset(), Vector3.up);
        }

        void Start()
        {
            // move camera to Solar View - initialize lights
            Zoomer(1);
            dirLight.transform.LookAt(earth.earthSys.transform);
            earth.earthSys.gameObject.SetActive(false);
            dirLight.enabled = false;
            ptLight.intensity = ptInt;
        }

        void NewSolarClock(float clockR, System.DateTime passDate)
        {
            // (0) SUNDIAL;
            GameObject sunDial = NewSunDial(clockR, passDate);
            sunDial.name = "SunDial";
            sunDial.transform.SetParent(transform, false);

            // (1) EARTHDIAL;
            GameObject earthGO = new GameObject("EarthDial");
            earth = earthGO.AddComponent<Earth>();
            earth.NewEarthSystem(clockR, passDate);
            earthGO.transform.SetParent(transform, false);

            // (2) SUN and Planets;
            //.........(0) Planets;
            GameObject orbitsGO = new GameObject("PlanetOrbits");
            orbits = orbitsGO.AddComponent<Orbits>();
            orbits.NewOrbits(clockR * .5f);
            orbitsGO.transform.SetParent(transform, false);
        }

        // CREATION Functions
        GameObject NewSunDial(float sundialR, System.DateTime passDate)
        {
            GameObject sunDial = new GameObject();
            //.........(1) Sun;
            GameObject sunStar = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            float sunR = 12f;
            sunStar.transform.localScale = new Vector3(sunR, sunR, sunR);
            sunStar.name = "Sun";
            sunStar.transform.SetParent(sunDial.transform, false);
            sunStar.GetComponent<SphereCollider>().enabled = false;

            // .......(0) SEASONS;
            GameObject seasonCross = new GameObject("SeasonCross");
            Items.AddCanvas(seasonCross);
            Color axisColor = new Color(1, 1, 1, .5f);
            GameObject solsticeLine = Shapes.DrawLine("dotted", new Vector3(0, 0, sundialR),
                new Vector3(0, 0, -sundialR), axisColor, .5f, .5f);
            solsticeLine.name = "SolsticeLine";
            solsticeLine.transform.SetParent(seasonCross.transform, false);
            solsticeLine.transform.Rotate(Vector3.right, -90);

            GameObject equinoxLine = Shapes.DrawLine("dotted", new Vector3(0, 0, sundialR),
                new Vector3(0, 0, -sundialR), axisColor, .5f, .5f);
            equinoxLine.name = "EquinoxLine";
            equinoxLine.transform.SetParent(seasonCross.transform, false);
            equinoxLine.transform.Rotate(Vector3.up, 90);
            equinoxLine.transform.Rotate(Vector3.forward, -90);

            // Season labels;
            string[] seasonList = {"SUMMER", "SPRING", "WINTER", "FALL"};

            int count = 0;
            Text seasonText;
            Color seaTextColor = new Color(1, 1, 1, .5f);
            foreach (string season in seasonList)
            {
                seasonText = Items.NewText(season, seaTextColor, 60, TextAnchor.MiddleCenter, false);
                seasonText.transform.Rotate(Vector3.forward, -count * 90 - 45 - 90);
                seasonText.transform.Translate(Vector3.up * -sundialR * .82f);
                seasonText.transform.SetParent(seasonCross.transform);
                if (count == 0)
                    summerText = seasonText;
                else if (count == 1)
                    springText = seasonText;
                seasonLabels[count] = seasonText.gameObject;
                count++;
            }

            seasonCross.transform.SetParent(sunDial.transform, false);
            sunSprockCont = DrawSunSprockCont(sundialR, passDate);
            sunSprockCont.name = "SunSprockCont";
            sunSprockCont.transform.SetParent(sunDial.transform, false);

            //SUN LINE;
            sunLine = new GameObject("SunLine");

            GameObject yearLine = Items.YearLine((passDate.Year - 1).ToString(), earthLineL);
            yearLine.transform.SetParent(sunLine.transform, false);
            yearLine.transform.localPosition = new Vector3(0, -earthLineL, 0);
            yearQueue[0] = yearLine.transform.GetChild(1).GetComponent<Text>();

            yearLine = Items.YearLine(passDate.Year.ToString(), earthLineL);
            yearLine.transform.SetParent(sunLine.transform, false);
            yearQueue[1] = yearLine.transform.GetChild(1).GetComponent<Text>();

            yearLine = Items.YearLine((passDate.Year + 1).ToString(), earthLineL);
            yearLine.transform.SetParent(sunLine.transform, false);
            yearLine.transform.localPosition = new Vector3(0, earthLineL, 0);
            yearQueue[2] = yearLine.transform.GetChild(1).GetComponent<Text>();

            sunLine.transform.SetParent(sunDial.transform, false);
            sunLine.SetActive(false);

            return sunDial;
        }

        GameObject DrawSunSprockCont(float sundialR, System.DateTime passDate)
        {
            GameObject newSunSprockCont = new GameObject("SunSprocketContainer");

            sunSprock = DrawSunSprock(sundialR, passDate);
            sunSprock.name = "SunSprock";
            sunSprock.transform.SetParent(newSunSprockCont.transform, false);

            // ........(3) MONTH LABEL;
            string[] monthArray = Calendar.monthofYrAbr;
            mLabelWheel = new GameObject("MonthLabelWheel");

            Items.AddCanvas(mLabelWheel);

            Text monthText;
            for (int mt = 1; mt <= 12; mt++)
            {
                monthText = Items.NewText(monthArray[mt - 1], Color.white, 60, TextAnchor.MiddleCenter, false);
                monthText.transform.Rotate(Vector3.forward, 30 * mt);
                monthText.transform.Translate(Vector3.up * (-sundialR + 6));

                monthText.transform.SetParent(mLabelWheel.transform);
            }

            FlipMonthLabels(true);
            mLabelWheel.transform.Rotate(Vector3.forward, -360f * 15f / YEAR - 180);

            mLabelWheel.transform.SetParent(newSunSprockCont.transform);

            return newSunSprockCont;
        }

        GameObject DrawSunSprock(float sundialR, System.DateTime passDate)
        {
            // ........(2) DAYS;
            int[] daysinMonth = {31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31};
            if (System.DateTime.IsLeapYear(passDate.Year))
            {
                daysinMonth[1]++;
            }

            // determine start point of first Sunday;       
            int firstSunday = 8 - Calendar.ConvertDaytoInt(jan1ofthisYear.DayOfWeek.ToString());
            // create point cloud for sprocket mesh;
            List<Vector3> pointList = new List<Vector3>();
            List<int> indList = new List<int>();
            object[] retArr = new object[3];
            int counter = 0;
            int pointCounter = 0;
            float alpha;
            float sprockTh = 1;
            float baseAl = .002f;
            float pointAl = .001f;
            float bigH = 10;
            float medH = 5;
            float smallH = 3;
            int dayCounter = firstSunday;
            float step = 360f / YEAR * Mathf.PI / 180f;

            foreach (int diM in daysinMonth)
            {
                alpha = -counter * step;
                if (dayCounter == 7)
                {
                    dayCounter = 0;
                }

                // add first-of-month tick;
                retArr = Shapes.SprockTick(
                    sundialR,
                    bigH,
                    alpha,
                    -(counter + 1) * step,
                    sprockTh,
                    baseAl,
                    pointAl,
                    pointCounter,
                    0);
                pointList.AddRange((List<Vector3>) retArr[0]);
                indList.AddRange((List<int>) retArr[1]);
                pointCounter = (int) retArr[2];
                counter++;
                dayCounter++;
                for (int dd = 0; dd < diM - 1; dd++)
                {
                    alpha = -counter * step;
                    if (dayCounter == 7)
                    {
                        // add Sunday tick;
                        retArr = Shapes.SprockTick(
                            sundialR,
                            medH,
                            alpha,
                            -(counter + 1) * step,
                            sprockTh,
                            baseAl,
                            pointAl,
                            pointCounter,
                            0);
                        dayCounter = 0;
                    }
                    else
                    {
                        // add day tick;
                        retArr = Shapes.SprockTick(
                            sundialR,
                            smallH,
                            alpha,
                            -(counter + 1) * step,
                            sprockTh,
                            baseAl,
                            pointAl,
                            pointCounter,
                            0);
                    }

                    pointList.AddRange((List<Vector3>) retArr[0]);
                    indList.AddRange((List<int>) retArr[1]);
                    pointCounter = (int) retArr[2];
                    counter++;
                    dayCounter++;
                }
            }

            indList.RemoveRange(indList.Count - 6, 6);
            return Shapes.CreatePoly(pointList, indList, Color.white);
        }

        // TOGGLE Functions
        void Toggle()
        {
            orbitScale = new Vector3(1, orbits.flatScale, 1);
            sunSprockCont.SetActive(true);
            earth.gameObject.SetActive(true);
            foreach (GameObject seaLab in seasonLabels)
                seaLab.SetActive(true);

            sunLine.SetActive(false);

            if (viewState == ViewState.HelioCentric)
            {
                // zoom to Earth;
                viewState = ViewState.GeoCentric;
                GetEarthCam();
                orbits.gameObject.SetActive(false);
                earth.earthSys.gameObject.SetActive(true);
                earthScale = 1;
                ptInt = 1;
                dirLight.enabled = true;
                orthoSize = earthOrthoSize;
                FlipMonthLabels(false);
                if (calCreated)
                    calendar.dayCal.SetActive(calendar.vis);
            }
            else
            {
                // zoom to Solar;
                viewState = ViewState.HelioCentric;
                //420f;
                // 270;
                targetPos = new Vector3(0, solCamY, 0);
                targetRot = Quaternion.Euler(new Vector3(90, 0, 0));
                orbits.gameObject.SetActive(true);
                earthScale = .001f;
                orthoSize = solOrthoSize;

                ptInt = .5f;
                FlipMonthLabels(true);
            }

            if (zooming != null)
                StopCoroutine(zooming);

            zooming = StartCoroutine(Zoom(50));
        }

        void FlipMonthLabels(bool passLabelUp)
        {
            if (passLabelUp != labelUp)
            {
                labelUp = passLabelUp;
                int count = 0;

                for (int ii = 3; ii < 9; ii++)
                {
                    mLabelWheel.transform.GetChild(ii).Rotate(Vector3.forward * 180);
                    count++;
                }

                summerText.transform.Rotate(Vector3.forward * 180);
                springText.transform.Rotate(Vector3.forward * 180);
            }
        }

        IEnumerator Zoom(int passCD)
        {
            for (int ii = passCD; ii > 0; ii--)
            {
                Zoomer(1f / ii);
                yield return null;
            }

            zooming = null;
            Zoomer(1);
        }

        void Zoomer(float lerp)
        {
            Camera.main.transform.position = Vector3.Lerp(
                Camera.main.transform.position,
                targetPos,
                lerp);
            Camera.main.transform.rotation = Quaternion.Lerp(
                Camera.main.transform.rotation,
                targetRot,
                lerp);
            Camera.main.orthographicSize = Mathf.Lerp(
                Camera.main.orthographicSize,
                orthoSize,
                lerp);
            earth.earthSys.transform.localScale = Vector3.Lerp(
                earth.earthSys.transform.localScale,
                new Vector3(earthScale, earthScale, earthScale),
                lerp);

            ptLight.intensity = Mathf.Lerp(ptLight.intensity, ptInt, lerp);

            if (viewState == ViewState.HelioCentric && lerp >= 1 - float.Epsilon)
            {
                earth.earthSys.gameObject.SetActive(false);
                dirLight.enabled = false;
            }

            foreach (GameObject path in orbits.paths)
            {
                path.transform.localScale = Vector3.Lerp(
                    path.transform.localScale,
                    orbitScale,
                    lerp);
            }
        }

        // TIME Functions
        void SetOrbit(System.DateTime newDateUTC, System.DateTime newDateLocal)
        {
            // Earth System Orbit
            // January 1st is 10 days past the solstice which is 180 deg from where the top of the circle is 
            const float yearStartOffset = -180 - (10 / YEAR) * 360;
            earth.transform.rotation = Quaternion.AngleAxis(
                Orbits.GetEarthOrbitAngle(newDateUTC, YEAR, yearStartOffset),
                Vector3.up);

            // reset Earth Sphere
            earth.earthSph.transform.rotation = Quaternion.identity;
            // the polar axis of the earth is tilted
            earth.earthSph.transform.Rotate(Vector3.right, -23.4f);
            // the Earth is rotated by the hours into the day
            earth.earthSph.transform.Rotate(Vector3.up, Earth.getDiurnalPos(newDateUTC));

            // Clock Sprocket - reverse rotated from the diurnal position of the Earth
            earth.localWheelCont.transform.localRotation =
                Quaternion.Euler(new Vector3(
                    180,
                    -Earth.getLocalClockAlpha(newDateLocal) + 180,
                    0));

            // Moon Orbit
            earth.moonDial.moonSys.transform.rotation = Quaternion.AngleAxis(
                Orbits.GetNonEarthOrbitAngle(newDateUTC, Moon.lunarSidereal, 60),
                Vector3.up);

            if (currentDay != newDateLocal.Day)
            {
                // day has switched over -> move month tri
                if (currentMonth != newDateLocal.Month)
                {
                    earth.moonDial.MoveMoonSprockCont(newDateLocal, true, true, forward);
                    currentMonth = newDateLocal.Month;
                }
                else
                    earth.moonDial.MoveMoonSprockCont(newDateLocal, true, false, forward);

                currentDay = newDateLocal.Day;

                // update dayCal
                if (calCreated)
                    calendar.DrawDayCalendar(newDateLocal, earth.earthSys.transform);
            }

            if (currentYear != newDateLocal.Year)
            {
                // year has changed over
                sunSprockCont.transform.localRotation = Quaternion.AngleAxis(SunSprockOffset(), Vector3.up);
                currentYear = newDateLocal.Year;

                yearQueue[0].text = (currentYear - 1).ToString();
                yearQueue[1].text = (currentYear).ToString();
                yearQueue[2].text = (currentYear + 1).ToString();

                jan1ofthisYear = new System.DateTime();
                jan1ofthisYear = jan1ofthisYear.AddYears(newDateLocal.Year - 1);

                Destroy(sunSprock);
                sunSprock = DrawSunSprock(sysDia, newDateLocal);
                sunSprock.name = "SunSprock";
                sunSprock.transform.SetParent(sunSprockCont.transform, false);
                sunSprock.transform.localRotation = Quaternion.identity;

                // update yearcal
                if (calCreated)
                    calendar.DrawYearCalendar(newDateLocal, transform);
            }

            earth.intDatelineSplit.transform.rotation =
                Quaternion.AngleAxis(
                    earth.earthSph.transform.rotation.eulerAngles.y - 90,
                    Vector3.up);
            string temp1 = earth.day21.text;
            earth.day21.text = "";
            earth.day21.text = temp1;
            string temp2 = earth.day22.text;
            earth.day22.text = "";
            earth.day22.text = temp2;

            int datelineDay = Earth.GetDatelineDay(newDateUTC);
            if (datelineDay != currentINDL)
            {
                int yesterday = datelineDay - 1;
                if (yesterday < 0)
                    yesterday = 6;

                earth.day11.text = Calendar.daysofweekAbr[yesterday];
                earth.day12.text = Calendar.daysofweekAbr[yesterday];

                earth.day21.text = Calendar.daysofweekAbr[datelineDay];
                earth.day22.text = Calendar.daysofweekAbr[datelineDay];

                currentINDL = datelineDay;
            }

            earth.RedrawStrip(newDateUTC.AddHours(12));

            // Mercury Orbit: 88 days
            orbits.planets[0].transform.localRotation = Quaternion.AngleAxis(
                Orbits.GetNonEarthOrbitAngle(newDateUTC, 88, 120),
                Vector3.up);

            // Venus Orbit: 224.698 days
            orbits.planets[1].transform.localRotation = Quaternion.AngleAxis(
                Orbits.GetNonEarthOrbitAngle(newDateUTC, 224.698f, 120),
                Vector3.up);

            // little Earth Orbit:365.256363004 days
            orbits.planets[2].transform.localRotation = Quaternion.AngleAxis(
                earth.transform.localRotation.eulerAngles.y,
                Vector3.up);

            // Mars Orbit:  686.971
            orbits.planets[3].transform.localRotation = Quaternion.AngleAxis(
                Orbits.GetNonEarthOrbitAngle(newDateUTC, 686.971f, -130),
                Vector3.up);

            if (viewState == ViewState.GeoCentric)
            {
                // move main camera and light to keep up with Earth
                GetEarthCam();
                Zoomer(1);
                dirLight.transform.LookAt(earth.earthSys.transform);
            }

            // move the sun line horizontally as a percentage of how far the solar system is through the year
            sunLine.transform.position = new Vector3(
                0,
                earthLineL * (.5f - (newDateLocal.Ticks - jan1ofthisYear.Ticks) /
                              10000000f /
                              SiderealDayInSeconds /
                              YEAR),
                0);
        }

        void GetEarthCam()
        {
            targetPos = new Vector3(
                earth.earthSys.transform.position.x,
                earthCamY,
                earth.earthSys.transform.position.z);
            targetRot = Quaternion.Euler(new Vector3(
                90,
                earth.transform.rotation.eulerAngles.y - 180,
                0));
        }

        static float SunSprockOffset()
        {
            return -10 * 360 / YEAR - 180 + Earth.GetTimeZone() * 360 / (YEAR * 24);
        }

        // UPDATE Functions
        IEnumerator MinuteUpdate()
        {
            if (showNow)
            {
                //set orbit exactly every minute
                SetOrbit(System.DateTime.UtcNow, System.DateTime.Now);
            }

            yield return new WaitForSeconds(60);
            minuteUpdate = StartCoroutine(MinuteUpdate());
        }

        IEnumerator SecondUpdate()
        {
            digiClock.SetTime(System.DateTime.Now);

            yield return new WaitForSeconds(1);
            secondUpdate = StartCoroutine(SecondUpdate());
        }

        void FixedUpdate()
        {
            // default tick is .02 sec;       
            if (!showNow)
            {
                SetOrbit(travelDateUTC, travelDateLocal);
                travelDateUTC = travelDateUTC.AddMinutes(spinInc);
                travelDateLocal = travelDateLocal.AddMinutes(spinInc);
                digiClock.SetTime(travelDateLocal);
            }
        }

        void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                Toggle();
            }

            if (Input.GetKeyDown(KeyCode.I))
            {
                viewState = ViewState.Galactic;
                sunSprockCont.SetActive(false);
                earth.gameObject.SetActive(false);
                targetPos = new Vector3(375, 330, 150);
                targetRot = Quaternion.Euler(new Vector3(40, 250, 257));
                orbits.gameObject.SetActive(true);
                foreach (GameObject seaLab in seasonLabels)
                    seaLab.SetActive(false);

                sunLine.SetActive(true);
                earthScale = .001f;
                orbitScale = Vector3.one;

                ptInt = .35f;

                if (zooming != null)
                    StopCoroutine(zooming);

                orthoSize = 150;
                zooming = StartCoroutine(Zoom(50));
            }

            if (Input.GetKeyDown(KeyCode.T))
            {
                digiClock.gameObject.SetActive(!digiClock.gameObject.activeInHierarchy);
            }


            // switch between realtime and speed time;
            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                showNow = false;
                spinInc += 3;
                if (spinInc > 0)
                    forward = true;
            }

            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                showNow = false;
                spinInc -= 3;
                if (spinInc < 0)
                    forward = false;
            }

            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                spinInc = 0;
                SetOrbit(System.DateTime.UtcNow, System.DateTime.Now);
                showNow = true;
                spinInc = 0;
                forward = true;
            }

            if (Input.GetKeyDown(KeyCode.C))
            {
                if (!calCreated)
                {
                    calendar = new Calendar();
                    calendar.Init(YEAR, sysDia, sysDia * .4f, SunSprockOffset() + 360 / YEAR);
                    System.DateTime date2 = System.DateTime.Now;
                    calendar.DrawYearCalendar(date2, transform);

                    calendar.DrawDayCalendar(date2, earth.earthSys.transform);
                    calCreated = true;
                }

                calendar.Toggle();
            }

            if (!minuteFound)
            {
                //  Debug.Log("Searching...");
                if (System.DateTime.Now.Second < 1)
                {
                    //    Debug.Log("FOUND");
                    minuteFound = true;
                    minuteUpdate = StartCoroutine(MinuteUpdate());
                }
            }

            if (secondFound) return;
            //  Debug.Log("Searching...");
            
            if (System.DateTime.Now.Millisecond >= 10) return;
            //    Debug.Log("FOUND");
            secondFound = true;
            secondUpdate = StartCoroutine(SecondUpdate());
        }

        void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                minuteFound = false;
                secondFound = false;
                if (minuteUpdate != null)
                {
                    StopCoroutine(minuteUpdate);
                    minuteUpdate = null;
                }

                if (secondUpdate != null)
                {
                    StopCoroutine(secondUpdate);
                    secondUpdate = null;
                }
            }
            else
            {
                SetOrbit(System.DateTime.UtcNow, System.DateTime.Now);
            }
        }

        void OnApplicationFocus(bool hasFocus)
        {
            if (hasFocus)
            {
                SetOrbit(System.DateTime.UtcNow, System.DateTime.Now);
            }
        }
    }
}