using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Assets.GraphicsUtil.Shapes;
using Assets.UI;
using Assets.UI.Elements;
using Assets.UI.Raycasting;
using Assets.UI.Text;
using DefaultNamespace;
using TMPro;

namespace Assets
{
    [RequireComponent(typeof(SolarTime))]
    public class SolarClock : MonoBehaviour, ISelectable
    {
        public enum ViewState
        {
            Galactic,
            HelioCentric,
            GeoCentric,
        }

        public static SolarClock Instance;
        [HideInInspector] public SolarTime solarTime;

        // calibration vars
        public const float YEAR = 365.256363004f;
        const float sysDia = 150;
        float earthScale = .001f;
        const float earthLineL = 400;
        const float SiderealDayInSeconds = 86164.0905f;

        // font, shader material vars
        public TMP_FontAsset mainFont;
        public Shader mainShader;
        public Material EarthMM, MoonMat;
        [HideInInspector] public Material mainMat;
        public TextBox TextBoxPrefab;
        
        // persistent objects
        [HideInInspector] public Earth earth;
        Orbits orbits;
        public PolygonFactory polygonFactory;
        public DigitalClock digiClock;
        public Calendar calendar;
        
        GameObject sunSprockCont, sunLine, mLabelWheel;
        SphereCollider sphereCollider;
        Raycast raycast;
        Polygon sunSprock;
        TextBox summerText, springText;
        [HideInInspector] public CalendarMenu calendarMenu;
        Button nowButton;
        readonly GameObject[] seasonLabels = new GameObject[4];
        readonly TextBox[] yearQueue = new TextBox[3];

        // camera vars
        public ViewState viewState = ViewState.HelioCentric;
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
        public bool calCreated = false;
        int currentYear, dst;
        DateTime jan1ofthisYear;
        bool labelUp = false;

        // INIT Functions
        void Awake()
        {
            Instance = this;
            solarTime = GetComponent<SolarTime>();

            QualitySettings.antiAliasing = 4;
            orthoSize = solOrthoSize;
            sphereCollider = gameObject.AddComponent<SphereCollider>();
            sphereCollider.radius = 100;

            mainMat = new Material(mainShader);

            NewCylinder.Init(polygonFactory, mainMat);
            NewCube.InitCube(polygonFactory, mainMat);

            raycast = gameObject.AddComponent<Raycast>();

            nowButton = Button.Create("0", TextBox.FontType.MainFont, 120, TextAlignmentOptions.Center);
            nowButton.Pad = 20;
            nowButton.transform.SetParent(Camera.main.transform, false);
            nowButton.transform.localPosition = new Vector3(
                0,
                -270,
                100);
            nowButton.SelectionAction = solarTime.NowTime;

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

            DateTime date1 = DateTime.UtcNow;
            DateTime date2 = DateTime.Now;

            //  Debug.Log(date1)
            //  Debug.Log(date2)

            // store time values to check for days/year/timezone switch
            jan1ofthisYear = new System.DateTime();
            jan1ofthisYear = jan1ofthisYear.AddYears(date2.Year - 1);
            currentYear = date2.Year;
       
            dst = 0;
            if (date2.IsDaylightSavingTime())
            {
                dst = 1;
            }

            EarthMM.SetTextureOffset("_DetailAlbedoMap",
                new Vector2((12 - Earth.GetTimeZone() - dst + .5f) / 24f, 0));

            // create new SolarClock and set celestial positions
            gameObject.name = "SolarClock";
            NewSolarClock(sysDia, date2);

            SetOrbit(date1, date2);
            earth.moonDial.MoveMoonSprockCont(date2, false, false);
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

            calendarMenu = Camera.main.GetComponent<CalendarMenu>();
            CreateOrToggleCalendar();
        }

        void NewSolarClock(float clockR, System.DateTime passDate)
        {
            // (0) SUNDIAL
            GameObject sunDial = NewSunDial(clockR, passDate);
            sunDial.name = "SunDial";
            sunDial.transform.SetParent(transform, false);

            // (1) EARTHDIAL
            GameObject earthGO = new GameObject("EarthDial");
            earth = earthGO.AddComponent<Earth>();
            earth.NewEarthSystem(clockR, passDate);
            earthGO.transform.SetParent(transform, false);

            // (2) SUN and Planets
            //.........(0) Planets
            GameObject orbitsGO = new GameObject("PlanetOrbits");
            orbits = orbitsGO.AddComponent<Orbits>();
            orbits.NewOrbits(clockR * .5f);
            orbitsGO.transform.SetParent(transform, false);
        }

        #region CREATION Functions
        
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
            Color axisColor = new Color(1, 1, 1, .5f);
            GameObject solsticeLine = PolygonFactory.DrawDottedLine(
                new Vector3(0, 0, sundialR),
                new Vector3(0, 0, -sundialR),
                axisColor,
                20);
            solsticeLine.name = "SolsticeLine";
            solsticeLine.transform.SetParent(seasonCross.transform, false);

            GameObject equinoxLine = PolygonFactory.DrawDottedLine(
                new Vector3(0, 0, sundialR),
                new Vector3(0, 0, -sundialR),
                axisColor,
                20);
            equinoxLine.name = "EquinoxLine";
            equinoxLine.transform.SetParent(seasonCross.transform, false);
            equinoxLine.transform.Rotate(Vector3.up, 90);

            // Season labels;
            string[] seasonList = {"SUMMER", "SPRING", "WINTER", "FALL"};

            int count = 0;
            foreach (string season in seasonList)
            {
                TextBox seasonText = TextBox.Create(season, TextBox.FontType.SecFont, 60, TextAlignmentOptions.Center);
                seasonText.transform.SetParent(seasonCross.transform, false);
                seasonText.transform.Rotate(Vector3.up, count * 90 - 45);
                seasonText.transform.Translate(Vector3.forward * sundialR * .82f);
                seasonText.transform.Rotate(Vector3.right * 90);
                seasonText.transform.Rotate(Vector3.forward * 180);
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
            sunLine.transform.SetParent(sunDial.transform, false);
            sunLine.transform.Rotate(Vector3.up * -90);

            GameObject yearLine = GalacticLine.YearLine((passDate.Year - 1).ToString(), earthLineL);
            yearLine.transform.SetParent(sunLine.transform, false);
            yearLine.transform.localPosition = new Vector3(0, -earthLineL, 0);
            yearQueue[0] = yearLine.transform.GetChild(1).GetComponent<TextBox>();

            yearLine = GalacticLine.YearLine(passDate.Year.ToString(), earthLineL);
            yearLine.transform.SetParent(sunLine.transform, false);
            yearQueue[1] = yearLine.transform.GetChild(1).GetComponent<TextBox>();

            yearLine = GalacticLine.YearLine((passDate.Year + 1).ToString(), earthLineL);
            yearLine.transform.SetParent(sunLine.transform, false);
            yearLine.transform.localPosition = new Vector3(0, earthLineL, 0);
            yearQueue[2] = yearLine.transform.GetChild(1).GetComponent<TextBox>();

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

            TextBox monthText;
            for (int mt = 1; mt <= 12; mt++)
            {
                monthText = TextBox.Create(monthArray[mt - 1], TextBox.FontType.MainFont, 60,
                    TextAlignmentOptions.Center);
                monthText.transform.Rotate(Vector3.forward, 30 * mt);
                monthText.transform.Translate(Vector3.up * (-sundialR + 6));

                monthText.transform.SetParent(mLabelWheel.transform);
            }

            FlipMonthLabels(true);
            mLabelWheel.transform.Rotate(Vector3.right * 90);
            mLabelWheel.transform.Rotate(Vector3.forward, -360 * 15 / YEAR - 180);

            mLabelWheel.transform.SetParent(newSunSprockCont.transform);

            return newSunSprockCont;
        }

        Polygon DrawSunSprock(float sundialR, System.DateTime passDate)
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
            Circle.SprocketTick retArr;
            int counter = 0;
            int pointCounter = 0;
            float alpha;
            const float sprockTh = 1;
            const float baseAl = .002f;
            const float pointAl = .001f;
            const float bigH = 10;
            const float medH = 5;
            const float smallH = 3;
            int dayCounter = firstSunday;
            const float step = 360f / YEAR * Mathf.PI / 180f;

            foreach (int diM in daysinMonth)
            {
                alpha = -counter * step;
                if (dayCounter == 7)
                {
                    dayCounter = 0;
                }

                // add first-of-month tick;
                retArr = Circle.SprockTick(
                    sundialR,
                    bigH,
                    alpha,
                    -(counter + 1) * step,
                    sprockTh,
                    baseAl,
                    pointAl,
                    pointCounter,
                    0);
                pointList.AddRange(retArr.pointList);
                indList.AddRange(retArr.indexList);
                pointCounter = retArr.pointCounter;
                counter++;
                dayCounter++;
                for (int dd = 0; dd < diM - 1; dd++)
                {
                    alpha = -counter * step;
                    if (dayCounter == 7)
                    {
                        // add Sunday tick;
                        retArr = Circle.SprockTick(
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
                        retArr = Circle.SprockTick(
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

                    pointList.AddRange(retArr.pointList);
                    indList.AddRange(retArr.indexList);
                    pointCounter = retArr.pointCounter;
                    counter++;
                    dayCounter++;
                }
            }

            indList.RemoveRange(indList.Count - 6, 6);

            Polygon newSunSprocket = PolygonFactory.NewPoly(SolarClock.Instance.mainMat, false);
            newSunSprocket.Draw3DPoly(pointList.ToArray(), indList.ToArray());
            newSunSprocket.SetColor(Color.white);
            return newSunSprocket;
        }

        void CreateOrToggleCalendar()
        {
            if (!calCreated)
            {
                calendar = new Calendar();
                calendar.Init(YEAR, sysDia, sysDia * .4f);

                calCreated = true;
            }
        }

        #endregion
        
        #region TOGGLE Functions
        
        public void Toggle(ViewState newState)
        {
            viewState = newState;
            orbitScale = new Vector3(1, orbits.flatScale, 1);
            sunSprockCont.SetActive(true);
            earth.gameObject.SetActive(true);
            foreach (GameObject seaLab in seasonLabels)
                seaLab.SetActive(true);

            sunLine.SetActive(false);

            if (viewState == ViewState.GeoCentric)
            {
                // zoom to Earth
                sphereCollider.enabled = false;
                earth.earthSphereCollider.enabled = true;
                GetEarthCam();
                orbits.gameObject.SetActive(false);
                earth.earthSys.gameObject.SetActive(true);
                earthScale = 1;
                ptInt = 1;
                dirLight.enabled = true;
                orthoSize = earthOrthoSize;
                FlipMonthLabels(false);
            }
            else
            {
                // zoom to Solar
                //420f;
                // 270;
                sphereCollider.enabled = true;
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

            foreach (Circle path in orbits.paths)
            {
                path.transform.localScale = Vector3.Lerp(
                    path.transform.localScale,
                    orbitScale,
                    lerp);
            }
        }

        #endregion
        
        #region TIME Functions
        
        public void SetOrbit(DateTime newDateUTC, DateTime newDateLocal)
        {
            earth.SetEarthSystemOrbit(newDateUTC, newDateLocal);
            
            orbits.SetLittlePlanetsOrbit(newDateUTC);

            if (currentYear != newDateLocal.Year)
            {
                // year has changed over
                sunSprockCont.transform.localRotation = Quaternion.AngleAxis(SunSprockOffset(), Vector3.up);
                currentYear = newDateLocal.Year;

                yearQueue[0].Text = (currentYear - 1).ToString();
                yearQueue[1].Text = (currentYear).ToString();
                yearQueue[2].Text = (currentYear + 1).ToString();

                jan1ofthisYear = new DateTime();
                jan1ofthisYear = jan1ofthisYear.AddYears(newDateLocal.Year - 1);

                Destroy(sunSprock.gameObject);
                sunSprock = DrawSunSprock(sysDia, newDateLocal);
                sunSprock.name = "SunSprock";
                sunSprock.transform.SetParent(sunSprockCont.transform, false);

                // update yearcal
                if (calCreated)
                {
                    calendar.DrawYearCalendar(newDateLocal, transform);
                }
            }

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

        public static float SunSprockOffset()
        {
            // January 1st is 10 days past the solstice which is 180 deg from where the top of the circle is 
            // also nudge by the amount of hours that this timezone is ahead or behind INDL
            return -10 * 360 / YEAR - 180 + Earth.GetTimeZone() * 360 / (YEAR * 24);
        }

        void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                RaycastTarget target = raycast.TargetAfterCasting();
                target?.AsSelectable.RequestSelection();
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

            if (Input.GetKeyDown(KeyCode.C))
            {
                CreateOrToggleCalendar();
            }
        }

        void OnApplicationFocus(bool hasFocus)
        {
            if (hasFocus)
            {
                SetOrbit(DateTime.UtcNow, DateTime.Now);
            }
        }

        #endregion
        
        #region ISelectable
        
        public Transform SelectionTarget => transform;

        public void RequestSelection()
        {
            Toggle(ViewState.GeoCentric);
        }
        
        #endregion
    }
}