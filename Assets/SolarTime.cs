using System;
using System.Collections;
using Assets;
using UnityEngine;

namespace DefaultNamespace
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
        }

        DateTime UTCDateThatMatchesAngle(float targetAngle, DateTime test)
        {
            float startingAngDifference = AngularDifference(test, targetAngle);

            const float minDistance = 5;
            const float wraparoundDistance = 350;
            
            if (startingAngDifference > minDistance && startingAngDifference < wraparoundDistance ||
                startingAngDifference <= -wraparoundDistance)
            {
                // either the target is positively ahead or has just crossed going right to left on the bottom
                while (AngularDifference(test, targetAngle) > minDistance ||
                       AngularDifference(test, targetAngle) <= -wraparoundDistance)
                {
                    // reverse time for earth or moon
                    test = test.AddHours(isEarthTracking ? -12 : -1);
                }
            }
            else if (startingAngDifference < -minDistance && startingAngDifference > -wraparoundDistance ||
                     startingAngDifference >= wraparoundDistance)
            {
                // either the target is negatively behind or has just crossed going left to right on the bottom
                while (AngularDifference(test, targetAngle) < -minDistance ||
                       AngularDifference(test, targetAngle) >= wraparoundDistance)
                {
                    // advance time for earth or moon
                    test = test.AddHours(isEarthTracking ? 12 : 1);
                }
            }

            SolarClock.Instance.digiClock.SetTime(test);

            lastDate = test;
            return test;
        }

        float AngularDifference(DateTime test, float targetAngle)
        {
            return isEarthTracking
                ? targetAngle - Orbits.GetEarthOrbitAngle(test)
                : targetAngle - Orbits.GetNonEarthOrbitAngle(test, Moon.lunarSidereal, 60) + Orbits.GetEarthOrbitAngle(test) - 180;
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
                Vector2 mouseAroundCenter = new Vector2(
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