using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

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
            if (WasReleased())
            {
                isEarthTracking = false;
                isMoonTracking = false;
            }

            if (IsPressed()
                && (isEarthTracking || isMoonTracking))
            {
                showNow = false;
                SolarClock.Instance.nowButton.gameObject.SetActive(true);
                Vector2 mouseAroundCenter;
                if (Application.platform == RuntimePlatform.Android)
                {
                    mouseAroundCenter = new Vector2(
                        Touchscreen.current.primaryTouch.position.x.ReadValue() - Screen.width * .5f,
                        Touchscreen.current.primaryTouch.position.y.ReadValue() - Screen.height * .5f);
                }
                else
                {
                    mouseAroundCenter = new Vector2(
                        Mouse.current.position.x.ReadValue() - Screen.width * .5f,
                        Mouse.current.position.y.ReadValue() - Screen.height * .5f);
                }

                float angularPosition = -Mathf.Atan2(mouseAroundCenter.y, mouseAroundCenter.x) + Mathf.PI * .5f;
                if (angularPosition < -Mathf.PI)
                {
                    angularPosition += 2 * Mathf.PI;
                }

                if (angularPosition > Mathf.PI)
                {
                    angularPosition -= 2 * Mathf.PI;
                }

                float angleDeg = angularPosition * 180 / Mathf.PI;
                if (!SolarClock.Instance.IsNorth)
                {
                    angleDeg *= -1;
                    angleDeg += 180;
                }

                DateTime utcDateThatMatchesAngle = UTCDateThatMatchesAngle(angleDeg, lastDate);
                DateTime localTimeThatMatchesAngle = utcDateThatMatchesAngle.AddHours(Earth.GetTimeZone());
                SolarClock.Instance.SetOrbit(utcDateThatMatchesAngle, localTimeThatMatchesAngle);
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

        static bool IsPressed()
        {
            return Application.platform == RuntimePlatform.Android 
                ? Touchscreen.current.primaryTouch.press.isPressed 
                : Mouse.current.leftButton.isPressed;
        }
        
        static bool WasReleased()
        {
            return Application.platform == RuntimePlatform.Android 
                ? Touchscreen.current.primaryTouch.press.wasReleasedThisFrame 
                : Mouse.current.leftButton.wasReleasedThisFrame;
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