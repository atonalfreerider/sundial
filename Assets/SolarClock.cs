using System;
using UnityEngine;
using System.Collections;
using System.Linq;
using Assets.GraphicsUtil.Shapes;
using Assets.GraphicsUtil.Shapes.Lines;
using Assets.UI;
using Assets.UI.Elements;
using Assets.UI.Raycasting;
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
        public const float SYSTEM_DIAMETER = 150;
        float earthScale = .001f;
        const float earthLineL = 400;
        const float SiderealDayInSeconds = 86164.0905f;

        // font, shader material vars
        public Material EarthMM, MoonMat;

        // persistent objects
        [HideInInspector] public Earth earth;
        Orbits orbits;
        public PolygonFactory polygonFactory;
        public DigitalClock digiClock;
        public Calendar calendar;

        GameObject sunSprockCont, sunLine, mLabelWheel;
        SphereCollider sphereCollider;
        Raycast raycast;
        Circle sunSprock;
        TextBox summerText, springText;
        [HideInInspector] public CalendarMenu calendarMenu;
        public Button nowButton;
        readonly GameObject[] seasonLabels = new GameObject[4];
        readonly TextBox[] yearQueue = new TextBox[3];

        // camera vars
        public ViewState viewState = ViewState.HelioCentric;
        const float FIXED_CAM_Y = 200;
        Vector3 targetPos = new(0, FIXED_CAM_Y, 0);
        Quaternion targetRot = Quaternion.AngleAxis(90, Vector3.right);
        Vector3 orbitScale = new(1, .01f, 1);
        float orthoSize;
        Coroutine zooming;
        const float ZOOM_DURATION = 1;

        static float solOrthoSize => SYSTEM_DIAMETER * (Screen.width > Screen.height
            ? 1
            : (float)Screen.height / Screen.width);

        static float earthOrthoSize => solOrthoSize * (Screen.width > Screen.height
            ? .55f
            : .45f); // get a little closer on phones

        // light vars
        public Light ptLight, dirLight;
        float ptInt = .5f;

        // state vars
        public bool calCreated = false;
        int currentYear, dst;
        bool labelUp = false;

        // INIT Functions

        void Awake()
        {
            Instance = this;
            polygonFactory.Init();
            solarTime = GetComponent<SolarTime>();

            QualitySettings.antiAliasing = 4;
            orthoSize = solOrthoSize;
            sphereCollider = gameObject.AddComponent<SphereCollider>();
            sphereCollider.radius = SYSTEM_DIAMETER * .7f;

            Circle.NewCylinder.Init(polygonFactory);
            NewCube.InitCube(polygonFactory);
            StaticLink.InitStaticLink(polygonFactory);

            raycast = gameObject.AddComponent<Raycast>();

            nowButton = Button.Create("Reset Current Time", 200, TextAlignmentOptions.Center);
            nowButton.Pad = 20;
            nowButton.transform.SetParent(Camera.main.transform, false);
            if (Application.platform == RuntimePlatform.Android)
            {
                nowButton.transform.localPosition = new Vector3(
                    0,
                    -280,
                    100);
            }
            else
            {
                nowButton.transform.localPosition = new Vector3(
                    -200,
                    -140,
                    100);
            }

            nowButton.SelectionAction = solarTime.NowTime;
            nowButton.gameObject.SetActive(false);

            DateTime utcNow = DateTime.UtcNow;
            DateTime now = DateTime.Now;

            // store time values to check for days/year/timezone switch
            currentYear = now.Year;

            dst = 0;
            if (now.IsDaylightSavingTime())
            {
                dst = 1;
            }

            EarthMM.SetTextureOffset("_DetailAlbedoMap",
                new Vector2((12 - Earth.GetTimeZone() + dst + .5f) / 24f, 0));

            // create new SolarClock and set celestial positions
            gameObject.name = "SolarClock";
            NewSolarClock(now);

            SetOrbit(utcNow, now);
            // move 10 days past winter SOLSTICE + local hour difference
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
            CreateCalendar();
        }

        void NewSolarClock(DateTime passDate)
        {
            // (0) SUNDIAL
            GameObject sunDial = NewSunDial(passDate);
            sunDial.name = "SunDial";
            sunDial.transform.SetParent(transform, false);

            // (1) EARTHDIAL
            GameObject earthGO = new("EarthDial");
            earth = earthGO.AddComponent<Earth>();
            earth.NewEarthSystem(passDate);
            earthGO.transform.SetParent(transform, false);

            // (2) SUN and Planets
            //.........(0) Planets
            GameObject orbitsGO = new("PlanetOrbits");
            orbits = orbitsGO.AddComponent<Orbits>();
            orbits.NewOrbits();
            orbitsGO.transform.SetParent(transform, false);
        }

        #region CREATION Functions

        GameObject NewSunDial(DateTime passDate)
        {
            GameObject sunDial = new();
            //.........(1) Sun;
            GameObject sunStar = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            const float sunR = SYSTEM_DIAMETER * .08f;
            sunStar.transform.localScale = Vector3.one * sunR;
            sunStar.name = "Sun";
            sunStar.transform.SetParent(sunDial.transform, false);
            sunStar.GetComponent<SphereCollider>().enabled = false;

            // .......(0) SEASONS;
            GameObject seasonCross = new("SeasonCross");
            Color axisColor = new(1, 1, 1, .5f);
            GameObject solsticeLine = PolygonFactory.DrawDottedLine(
                new Vector3(0, 0, SYSTEM_DIAMETER),
                new Vector3(0, 0, -SYSTEM_DIAMETER),
                axisColor,
                SYSTEM_DIAMETER * .1333f);
            solsticeLine.name = "SolsticeLine";
            solsticeLine.transform.SetParent(seasonCross.transform, false);

            GameObject equinoxLine = PolygonFactory.DrawDottedLine(
                new Vector3(0, 0, SYSTEM_DIAMETER),
                new Vector3(0, 0, -SYSTEM_DIAMETER),
                axisColor,
                SYSTEM_DIAMETER * .1333f);
            equinoxLine.name = "EquinoxLine";
            equinoxLine.transform.SetParent(seasonCross.transform, false);
            equinoxLine.transform.Rotate(Vector3.up, 90);

            // Season labels;
            string[] seasonList = { "SUMMER", "SPRING", "WINTER", "FALL" };

            int count = 0;
            foreach (string season in seasonList)
            {
                TextBox seasonText = TextBox.Create(season, TextAlignmentOptions.Center);
                seasonText.Size = 60;
                seasonText.transform.SetParent(seasonCross.transform, false);
                seasonText.Color = new Color(1, 1, 1, .5f);
                seasonText.transform.Rotate(Vector3.up, count * 90 - 45);
                seasonText.transform.Translate(Vector3.forward * SYSTEM_DIAMETER * .82f);
                seasonText.transform.Rotate(Vector3.right * 90);
                seasonText.transform.Rotate(Vector3.forward * 180);
                switch (count)
                {
                    case 0:
                        summerText = seasonText;
                        break;
                    case 1:
                        springText = seasonText;
                        break;
                }

                seasonLabels[count] = seasonText.gameObject;
                count++;
            }

            seasonCross.transform.SetParent(sunDial.transform, false);
            sunSprockCont = DrawSunSprockCont(passDate);
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

        GameObject DrawSunSprockCont(DateTime passDate)
        {
            GameObject newSunSprockCont = new("SunSprocketContainer");

            sunSprock = DrawSunSprock(passDate);
            sunSprock.transform.SetParent(newSunSprockCont.transform, false);

            // ........(3) MONTH LABEL;
            string[] monthArray = Calendar.monthofYrAbr;
            mLabelWheel = new GameObject("MonthLabelWheel");

            TextBox monthText;
            for (int mt = 1; mt <= 12; mt++)
            {
                monthText = TextBox.Create(monthArray[mt - 1], TextAlignmentOptions.Center);
                monthText.Size = 60;
                monthText.transform.Rotate(Vector3.forward, 30 * mt);
                monthText.transform.Translate(Vector3.up * (-.96f * SYSTEM_DIAMETER));

                monthText.transform.SetParent(mLabelWheel.transform);
            }

            FlipMonthLabels(true);
            mLabelWheel.transform.Rotate(Vector3.right * 90);
            mLabelWheel.transform.Rotate(Vector3.forward, -360 * 15 / YEAR - 180);

            mLabelWheel.transform.SetParent(newSunSprockCont.transform);

            return newSunSprockCont;
        }

        static Circle DrawSunSprock(DateTime passDate)
        {
            Circle newSunSprocket = PolygonFactory.NewCirclePoly(PolygonFactory.Instance.mainMat);
            newSunSprocket.name = "SunSprocket";

            // ........(2) DAYS;
            newSunSprocket.DrawSunSprocket(SYSTEM_DIAMETER, passDate,
                SYSTEM_DIAMETER / 150f, .002f, .001f,
                SYSTEM_DIAMETER * .02f, SYSTEM_DIAMETER * .0333f, SYSTEM_DIAMETER * .0666f,
                YEAR);

            newSunSprocket.SetColor(Color.white);
            return newSunSprocket;
        }

        void CreateCalendar()
        {
            if (!calCreated)
            {
                calendar = new Calendar(YEAR, SYSTEM_DIAMETER, SYSTEM_DIAMETER * .4f);
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
                earth.handSphereCollider.enabled = false;

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
                sphereCollider.enabled = true;
                earth.handSphereCollider.enabled = true;

                targetPos = new Vector3(0, FIXED_CAM_Y, 0);
                targetRot = Quaternion.Euler(new Vector3(90, 0, 0));
                orbits.gameObject.SetActive(true);
                earthScale = .001f;
                orthoSize = solOrthoSize;

                ptInt = .5f;
                FlipMonthLabels(true);
            }

            if (zooming != null)
                StopCoroutine(zooming);

            zooming = StartCoroutine(Zoom(ZOOM_DURATION));
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

        IEnumerator Zoom(float animationDuration)
        {
            float animationProgress = 0;
            while (animationProgress < animationDuration)
            {
                Zoomer(Time.deltaTime / (animationDuration - animationProgress));
                yield return null;
                animationProgress += Time.deltaTime;
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
                Vector3.one * earthScale,
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

                Destroy(sunSprock.gameObject);
                sunSprock = DrawSunSprock(newDateLocal);
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

            TimeSpan yearProgress = new(newDateLocal.Ticks - new DateTime(newDateLocal.Year, 1, 1).Ticks);
            sunLine.transform.position = new Vector3(
                0,
                earthLineL * (.5f - yearProgress.Days / YEAR),
                0);
        }

        void GetEarthCam()
        {
            targetPos = new Vector3(
                earth.earthSys.transform.position.x,
                FIXED_CAM_Y,
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

        void OnApplicationFocus(bool hasFocus)
        {
            if (hasFocus)
            {
                SetOrbit(DateTime.UtcNow, DateTime.Now);
            }
        }

        #endregion

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
                zooming = StartCoroutine(Zoom(ZOOM_DURATION));
            }

            if (Input.GetKeyDown(KeyCode.T))
            {
                digiClock.gameObject.SetActive(!digiClock.gameObject.activeInHierarchy);
            }

            if (Input.GetKeyDown(KeyCode.C))
            {
                calendarMenu.calendars.menuButtons.First().Value.RequestSelection();
            }
        }

        #region ISelectable

        public Transform SelectionTarget => transform;

        public void RequestSelection()
        {
            Toggle(ViewState.GeoCentric);
        }

        #endregion
    }
}