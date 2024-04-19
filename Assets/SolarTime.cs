using System;
using System.Collections;
using UnityEngine;

namespace Assets
{
    public class SolarTime : MonoBehaviour
    {
        // state vars
        bool minuteFound = false;
        bool secondFound = false;
        public bool isEarthTracking = false;
        public bool isMoonTracking = false;
        bool showNow = true;
        DateTime lastDate = DateTime.Now;

        Coroutine minuteUpdate;
        Coroutine secondUpdate;

        public void NowTime()
        {
            SolarClock.Instance.SetOrbit(DateTime.UtcNow, DateTime.Now);
            showNow = true;
            SolarClock.Instance.nowButton.gameObject.SetActive(false);
            lastDate = DateTime.Now;
        }

        DateTime UTCDateThatMatchesAngle(float targetAngle, DateTime currentDate)
        {
            const float minDistance = 5;

            DateTime pastDate = currentDate.AddHours(isEarthTracking ? -12 : -1);
            DateTime futureDate = currentDate.AddHours(isEarthTracking ? 12 : 1);

            float pastAngleDiff = AngularDifference(pastDate, targetAngle);
            float futureAngleDiff = AngularDifference(futureDate, targetAngle);

            int safety = 0;
            while (pastAngleDiff > minDistance || futureAngleDiff > minDistance)
            {
                currentDate = futureAngleDiff < pastAngleDiff
                    ? futureDate
                    : pastDate;

                pastDate = currentDate.AddHours(isEarthTracking ? -12 : -1);
                futureDate = currentDate.AddHours(isEarthTracking ? 12 : 1);

                pastAngleDiff = AngularDifference(pastDate, targetAngle);
                futureAngleDiff = AngularDifference(futureDate, targetAngle);

                safety++;
                if (safety > 1000 || pastAngleDiff < minDistance || futureAngleDiff < minDistance) break;
            }

            SolarClock.Instance.digiClock.SetTime(currentDate);

            lastDate = currentDate;
            return currentDate;
        }

        float AngularDifference(DateTime test, float targetAngle)
        {
            float diff = isEarthTracking
                ? targetAngle - Orbits.GetEarthOrbitAngle(test)
                : targetAngle - Orbits.GetNonEarthOrbitAngle(test, Moon.lunarSidereal, 60) +
                  Orbits.GetEarthOrbitAngle(test) - 180;
            float abs = Mathf.Abs(diff);
            if (abs > 360)
            {
                // not entirely sure why, but the moon dial can return values greater than 360
                abs -= 360;
            }

            return Mathf.Min(abs, 360 - abs);
        }

        // UPDATE Functions
        void Update()
        {
            if (Input.GetMouseButtonUp(0))
            {
                isEarthTracking = false;
                isMoonTracking = false;
            }

            if (Input.GetMouseButton(0) && (isEarthTracking || isMoonTracking))
            {
                showNow = false;
                SolarClock.Instance.nowButton.gameObject.SetActive(true);
                Vector2 mouseAroundCenter = new(
                    Input.mousePosition.x - Screen.width * .5f,
                    Input.mousePosition.y - Screen.height * .5f);
                float angularPosition = -Mathf.Atan2(mouseAroundCenter.y, mouseAroundCenter.x) + Mathf.PI * .5f;
                if (angularPosition < -Mathf.PI)
                {
                    angularPosition += 2 * Mathf.PI;
                }

                if (angularPosition > Mathf.PI)
                {
                    angularPosition -= 2 * Mathf.PI;
                }

                DateTime utcDateThatMatchesAngle = UTCDateThatMatchesAngle(angularPosition * 180 / Mathf.PI, lastDate);
                DateTime localTimeThatMatchesAngle = utcDateThatMatchesAngle.AddHours(Earth.GetTimeZone());
                SolarClock.Instance.SetOrbit(utcDateThatMatchesAngle, localTimeThatMatchesAngle);
            }

            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                NowTime();
            }

            if (!minuteFound)
            {
                //  Debug.Log("Searching...");
                if (DateTime.Now.Second < 1)
                {
                    //    Debug.Log("FOUND");
                    minuteFound = true;
                    minuteUpdate = StartCoroutine(MinuteUpdate());
                }
            }

            if (secondFound) return;
            //  Debug.Log("Searching...");

            if (DateTime.Now.Millisecond >= 10) return;
            //    Debug.Log("FOUND");
            secondFound = true;
            secondUpdate = StartCoroutine(SecondUpdate());
        }

        IEnumerator MinuteUpdate()
        {
            if (showNow)
            {
                //set orbit exactly every minute
                SolarClock.Instance.SetOrbit(DateTime.UtcNow, DateTime.Now);
            }

            yield return new WaitForSeconds(60);
            minuteUpdate = StartCoroutine(MinuteUpdate());
        }

        IEnumerator SecondUpdate()
        {
            SolarClock.Instance.digiClock.SetTime(DateTime.Now);

            yield return new WaitForSeconds(1);
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
                SolarClock.Instance.SetOrbit(DateTime.UtcNow, DateTime.Now);
            }
        }
    }
}