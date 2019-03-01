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

        Coroutine minuteUpdate;
        Coroutine secondUpdate;
        public bool isTracking = false;

        bool showNow = true;

        DateTime lastDate = DateTime.Now;

        public void NowTime()
        {
            SolarClock.Instance.SetOrbit(DateTime.UtcNow, System.DateTime.Now);
            showNow = true;
        }

        DateTime UTCDateThatMatchesAngle(float targetAngle, DateTime test)
        {
            float startingAngDifference = AngularDifference(test, targetAngle);

            if (startingAngDifference > 5 && startingAngDifference < 270 ||
                startingAngDifference <= -270)
            {
                // either the target is positively ahead on the RHS or has just crossed to the LHS from the bottom
                while (AngularDifference(test, targetAngle) > 5 ||
                       AngularDifference(test, targetAngle) <= -270)
                {
                    // reverse time
                    test = test.AddHours(-12);
                }
            }
            else if (startingAngDifference < -5 && startingAngDifference > -270 ||
                     startingAngDifference >= 270)
            {
                // either the target is negatively behind on the RHS
                while (AngularDifference(test, targetAngle) < -5 ||
                       AngularDifference(test, targetAngle) >= 270)
                {
                    test = test.AddHours(12);
                }
            }

            SolarClock.Instance.digiClock.SetTime(test);

            lastDate = test;
            return test;
        }

        static float AngularDifference(DateTime test, float targetAngle)
        {
            return targetAngle - Orbits.GetEarthOrbitAngle(test);
        }

        // UPDATE Functions
        void Update()
        {
            if (Input.GetMouseButtonUp(0))
            {
                isTracking = false;
            }

            if (Input.GetMouseButton(0) && isTracking)
            {
                showNow = false;
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
                if (System.DateTime.Now.Second < 1)
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
                SolarClock.Instance.SetOrbit(System.DateTime.UtcNow, System.DateTime.Now);
            }

            yield return new WaitForSeconds(60);
            minuteUpdate = StartCoroutine(MinuteUpdate());
        }

        IEnumerator SecondUpdate()
        {
            SolarClock.Instance.digiClock.SetTime(System.DateTime.Now);

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
                SolarClock.Instance.SetOrbit(DateTime.UtcNow, System.DateTime.Now);
            }
        }
    }
}