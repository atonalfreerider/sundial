using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Assets.GraphicsUtil.Shapes;
using Assets.UI.Text;
using TMPro;
using Object = UnityEngine.Object;

namespace Assets
{
    public struct MyEvent
    {
        public readonly string title;
        public System.DateTime start;
        public System.DateTime end;
        public Color color;

        public MyEvent(
            string passTitle,
            System.DateTime passStart,
            System.DateTime passEnd,
            Color passColor)
        {
            title = passTitle;
            start = passStart;
            end = passEnd;
            color = passColor;
        }
    }

    public class Calendar
    {
        float YEAR;
        float calR;
        float earthR;

        public GameObject dayCal;
        GameObject yearCal;
        public bool vis = false;

        List<MyEvent> yearQueue;
        List<MyEvent> dayQueue;

        public static string[] daysofweek =
        {
            "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday"
        };

        public static readonly string[] daysofweekAbr =
        {
            "SUN", "MON", "TUE", "WED", "THU", "FRI", "SAT"
        };

        public static readonly string[] monthofYrAbr =
        {
            "JAN", "FEB", "MAR", "APR", "MAY", "JUN", "JUL", "AUG", "SEP", "OCT", "NOV", "DEC"
        };

        float sunSprockOffset;

        // INIT Functions


        static Dictionary<string, MyEvent[]> RetrieveCalendarEvents()
        {
            Dictionary<string, MyEvent[]> calendarsAndEvents = new Dictionary<string, MyEvent[]>();
            DateTime Jan1Of1970 = new DateTime(1970, 1, 1);

            using (AndroidJavaClass javaClass =
                new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                using (AndroidJavaObject activity = javaClass.GetStatic<AndroidJavaObject>("currentActivity"))
                {
                    AndroidJavaObject calObj = activity.Call<AndroidJavaObject>("getAllCalendarNames");
                    byte[] calendarNamesBytes = calObj.GetRawObject().ToInt32() != 0
                        ? AndroidJNIHelper.ConvertFromJNIArray<byte[]>(calObj.GetRawObject())
                        : new byte[0];
                    calObj.Dispose();

                    string rectifyCalNames = System.Text.Encoding.Default.GetString(calendarNamesBytes);
                    string[] calNames = rectifyCalNames.Split('|');
                    int count = 0;
                    foreach (string calName in calNames)
                    {
                        //Debug.Log(calName);
                        string[] pair = calName.Split(':');

                        bool parsed = int.TryParse(pair[0], out int index);
                        if (!parsed) continue;

                        AndroidJavaObject obj = activity.Call<AndroidJavaObject>("getAllEventsStartEnd", index);
                        int[] eventsStartEnd = obj.GetRawObject().ToInt32() != 0
                            ? AndroidJNIHelper.ConvertFromJNIArray<int[]>(obj.GetRawObject())
                            : new int[0];
                        obj.Dispose();

                        AndroidJavaObject obj2 = activity.Call<AndroidJavaObject>("getAllEventsTitles", index);
                        byte[] eventsTitles = obj2.GetRawObject().ToInt32() != 0
                            ? AndroidJNIHelper.ConvertFromJNIArray<byte[]>(obj2.GetRawObject())
                            : new byte[0];
                        obj2.Dispose();

                        string rectify = System.Text.Encoding.Default.GetString(eventsTitles);
                        string[] split = rectify.Split('|');

                        MyEvent[] calendarEvents = new MyEvent[split.Length];
                        for (int i = 0; i < calendarEvents.Length; i++)
                        {
                            string title = split[i];
                            if (i * 2 > eventsStartEnd.Length - 2) break;

                            // note: I added a "d" for double below by accident and it worked - what are the chances?
                            int start = eventsStartEnd[i * 2];
                            DateTime startDate = Jan1Of1970.AddMilliseconds(start * 10000d);
                            int end = eventsStartEnd[i * 2 + 1];
                            DateTime endDate = Jan1Of1970.AddMilliseconds(end * 10000d);

                            //int TimeZone = Earth.GetTimeZone();
                            calendarEvents[i] = new MyEvent(
                                title,
                                startDate,
                                endDate,
                                GetEventColor(title)
                            );
                        }

                        calendarsAndEvents.Add(calName, calendarEvents);

                        count++;
                    }
                }
            }

            return calendarsAndEvents;
        }

        static Color GetEventColor(string title)
        {
            switch (title)
            {
                case "ABQ":
                    return new Color(1f, 1f, 0f, .5f);
                case "LA":
                case "NYC":
                case "NC":
                    return new Color(1f, 0f, 0f, .5f);
                case "London":
                case "LONDON":
                case "Zurich":
                    return new Color(.3f, 0, 1f, .5f);
                case "India":
                case "Singapore":
                case "Taiwan":
                    return new Color(0f, 1f, 0f, .5f);
                default:
                    return new Color(0f, 0f, 1f, .5f);
            }
        }

        public void Init(float passYEAR, float passCR, float passER, float passSunSprockOffset)
        {
            YEAR = passYEAR;
            calR = passCR;
            earthR = passER;
            sunSprockOffset = passSunSprockOffset;

            yearQueue = new List<MyEvent>();
            dayQueue = new List<MyEvent>();

            /*
            System.DateTime now = System.DateTime.Now;
            yearQueue.Add(new MyEvent(
                "TEST",
                CreateDate(now.Year, 1, 1, 0, 0),
                CreateDate(now.Year, 2, 1, 0, 0),
                Color.red));

            dayQueue.Add(
                new MyEvent(
                    "TEST",
                    CreateDate(now.Year, now.Month, now.Day, 1, 0),
                    CreateDate(now.Year, now.Month, now.Day, 5, 0),
                    Color.red));
*/

            if (Application.platform != RuntimePlatform.Android) return;

            Dictionary<string, MyEvent[]> calendarEvents = RetrieveCalendarEvents();
            foreach (KeyValuePair<string, MyEvent[]> kvp in calendarEvents)
            {
                if (!kvp.Key.Contains("johnbvoorhees@gmail.com")) continue;
                foreach (MyEvent calendarEvent in kvp.Value)
                {
                    long ticks = calendarEvent.end.Ticks - calendarEvent.start.Ticks;
                    //Debug.Log($"{calendarEvent.title}:{prettyDate(calendarEvent.start)}-{prettyDate(calendarEvent.end)}");

                    if (ticks >= TimeSpan.TicksPerDay)
                    {
                        yearQueue.Add(calendarEvent);
                    }
                    else
                    {
                        dayQueue.Add(calendarEvent);
                    }
                }
            }
        }

        string prettyDate(DateTime dateTime)
        {
            return $"{dateTime.Year}/{dateTime.Month}/{dateTime.Day}";
        }

        public void DrawYearCalendar(System.DateTime passDate, Transform solarClock)
        {
            if (yearCal)
            {
                Object.Destroy(yearCal);
            }

            yearCal = new GameObject("YearCal");
            foreach (MyEvent ev in yearQueue)
            {
                if (ev.start.Year == passDate.Year)
                {
                    DrawEvent(ev, true).transform.SetParent(yearCal.transform, false);
                }
            }

            yearCal.transform.SetParent(solarClock, false);
        }

        public void DrawDayCalendar(System.DateTime passDate, Transform solarClock)
        {
            if (dayCal)
            {
                Object.Destroy(dayCal);
            }

            dayCal = new GameObject("DayCal");
            foreach (MyEvent ev in dayQueue)
            {
                if (ev.start.Year == passDate.Year &&
                    ev.start.Month == passDate.Month &&
                    ev.start.Day == passDate.Day)
                {
                    DrawEvent(ev, false).transform.SetParent(dayCal.transform, false);
                }
            }

            dayCal.transform.SetParent(solarClock, false);
        }

        static System.DateTime CreateDate(int yr, int month, int day, int hour, int min)
        {
            System.DateTime newDate = new System.DateTime();
            newDate = newDate.AddYears(yr - 1);
            newDate = newDate.AddMonths(month - 1);
            newDate = newDate.AddDays(day - 1);
            newDate = newDate.AddHours(hour);
            newDate = newDate.AddMinutes(min);
            return newDate;
        }

        Circle DrawEvent(MyEvent passEv, bool isYearEvent)
        {
            const float evH = 7;
            float R = 0;
            float prct = 0;
            string displayTitle;
            TimeSpan span = new TimeSpan(passEv.end.Ticks - passEv.start.Ticks);
            if (isYearEvent)
            {
                R = calR - 1;
                prct = (float) span.TotalDays / YEAR;

                displayTitle = passEv.title.Substring(0, Math.Min((int) span.TotalDays, passEv.title.Length));
            }
            else
            {
                R = earthR - 1;
                prct = prct = (float) span.TotalHours / 24;
                displayTitle = passEv.title.Substring(0, Math.Min((int) span.TotalHours * 4, passEv.title.Length));
            }

            Circle newRing = PolygonFactory.NewCirclePoly(SolarClock.Instance.mainMat);
            newRing.DrawRing(R, R - evH, prct, 0, 0, false);
            newRing.SetColor(passEv.color);

            TextBox titleT = TextBox.Create(displayTitle, TextBox.FontType.MainFont, 50, TextAlignmentOptions.Center);
            titleT.transform.SetParent(newRing.transform, false);

            float halfAng = (prct * .5f);
            titleT.transform.localPosition = new Vector3(
                (R - 10) * Mathf.Sin(halfAng * 2 * Mathf.PI),
                0,
                (R - 10) * Mathf.Cos(halfAng * 2 * Mathf.PI));
            titleT.transform.parent.Rotate(Vector3.up, (prct * .5f) * 360 + 180);
            titleT.transform.Rotate(Vector3.right * 90);
            if (isYearEvent || (!isYearEvent && passEv.end.Hour < 6) || (!isYearEvent && passEv.end.Hour > 18))
            {
                titleT.transform.Rotate(Vector3.forward * (180 - 360 * halfAng));
            }
            else
            {
                titleT.transform.Rotate(Vector3.forward * (-360 * halfAng));
            }

            titleT.Color = passEv.color;

            newRing.transform.rotation = Quaternion.AngleAxis(
                isYearEvent
                    ? Orbits.GetEarthOrbitAngle(passEv.end, YEAR, sunSprockOffset)
                    : -360 * (passEv.end.Hour + passEv.end.Minute / 60f) / 24f,
                Vector3.up);

            return newRing;
        }

        public void Toggle()
        {
            vis = !vis;
            yearCal.SetActive(vis);
            dayCal.SetActive(vis);
        }

        // REFERENCE Functions;
        public static int ConvertMonth(int passMonth)
        {
            return passMonth > 11 ? 0 : passMonth;
        }

        public static int ConvertDaytoInt(string passDay)
        {
            switch (passDay)
            {
                case "Sunday":
                    return 0;
                case "Monday":
                    return 1;
                case "Tuesday":
                    return 2;
                case "Wednesday":
                    return 3;
                case "Thursday":
                    return 4;
                case "Friday":
                    return 5;
                case "Saturday":
                    return 6;
                default:
                    return 0;
            }
        }

        public static int ConvertAbrDaytoInt(string passDay)
        {
            switch (passDay)
            {
                case "SUN":
                    return 0;
                case "MON":
                    return 1;
                case "TUE":
                    return 2;
                case "WED":
                    return 3;
                case "THU":
                    return 4;
                case "FRI":
                    return 5;
                case "SAT":
                    return 6;
                default:
                    return 0;
            }
        }

        public static int ConverMonthtoInt(string passDay)
        {
            switch (passDay)
            {
                case "JAN":
                    return 0;
                case "FEB":
                    return 1;
                case "MAR":
                    return 2;
                case "APR":
                    return 3;
                case "MAY":
                    return 4;
                case "JUN":
                    return 5;
                case "JUL":
                    return 6;
                case "AUG":
                    return 7;
                case "SEP":
                    return 8;
                case "OCT":
                    return 9;
                case "NOV":
                    return 10;
                case "DEC":
                    return 11;
                default:
                    return 0;
            }
        }
    }
}
