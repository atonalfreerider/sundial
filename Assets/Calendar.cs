using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Assets.GraphicsUtil.Shapes;
using Assets.UI;
using TMPro;
using Object = UnityEngine.Object;

namespace Assets
{
    public struct MyEvent
    {
        public readonly string parentCalendar;
        public readonly string title;
        public DateTime start;
        public DateTime end;
        public Color color;
        public readonly bool isYearEvent;

        public MyEvent(
            string parentCalendar,
            string passTitle,
            DateTime passStart,
            DateTime passEnd,
            Color passColor, 
            int timeZoneOffset)
        {
            this.parentCalendar = parentCalendar;
            title = passTitle;
            color = passColor;

            start = passStart;
            end = passEnd;
            long ticks = end.Ticks - start.Ticks;
            isYearEvent = ticks >= TimeSpan.TicksPerDay;
            
            if (isYearEvent) return;
            
            // all android calendar dates are UTC -> localize day events
            start = passStart.AddHours(timeZoneOffset);
            end = passEnd.AddHours(timeZoneOffset);
        }
    }

    public class Calendar
    {
        readonly float YEAR;
        readonly float calR;
        readonly float earthR;

        GameObject dayCal;
        GameObject yearCal;

        readonly List<string> displayedCalendars = new();
        Dictionary<string, MyEvent[]> calendarEvents;
        readonly List<MyEvent> yearQueue = new();
        readonly List<MyEvent> dayQueue = new();

        public static readonly string[] daysofweekAbr =
        {
            "SUN", "MON", "TUE", "WED", "THU", "FRI", "SAT"
        };

        public static readonly string[] monthofYrAbr =
        {
            "JAN", "FEB", "MAR", "APR", "MAY", "JUN", "JUL", "AUG", "SEP", "OCT", "NOV", "DEC"
        };

        static readonly Color EventColor = new(.7f, .7f, 0, .5f);

        // INIT Functions
        public Calendar(float YEAR, float calR, float earthR)
        {
            this.YEAR = YEAR;
            this.calR = calR;
            this.earthR = earthR;
  
            //TestCalendar();

            if (Application.platform != RuntimePlatform.Android) return;

            calendarEvents = RetrieveAndroidCalendarEvents();
            CalendarMenu calendarMenu = GameObject.Find("Menu").GetComponent<CalendarMenu>();
            calendarMenu.PassCalendars(calendarEvents.Keys.ToArray());
        }

        static Dictionary<string, MyEvent[]> RetrieveAndroidCalendarEvents()
        {
            Dictionary<string, MyEvent[]> calendarsAndEvents = new Dictionary<string, MyEvent[]>();
            DateTime Jan1Of1970 = new(1970, 1, 1);
            int timeZoneOffset = Earth.GetTimeZone();

            using AndroidJavaClass javaClass = new("com.unity3d.player.UnityPlayer");
            using AndroidJavaObject activity = javaClass.GetStatic<AndroidJavaObject>("currentActivity");
            AndroidJavaObject calObj = activity.Call<AndroidJavaObject>("getAllCalendarNames");

            if (calObj.GetRawObject().ToInt32() == 0)
            {
                // this returns empty if calendars are empty OR if this is the version without the calendar plugin
                calObj.Dispose();
                return calendarsAndEvents;
            }
                    
            byte[] calendarNamesBytes = calObj.GetRawObject().ToInt32() != 0
                ? AndroidJNIHelper.ConvertFromJNIArray<byte[]>(calObj.GetRawObject())
                : Array.Empty<byte>();
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
                    : Array.Empty<int>();
                obj.Dispose();

                AndroidJavaObject obj2 = activity.Call<AndroidJavaObject>("getAllEventsTitles", index);
                byte[] eventsTitles = obj2.GetRawObject().ToInt32() != 0
                    ? AndroidJNIHelper.ConvertFromJNIArray<byte[]>(obj2.GetRawObject())
                    : Array.Empty<byte>();
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
                        calName,
                        title,
                        startDate,
                        endDate,
                        EventColor,
                        timeZoneOffset
                    );
                }

                calendarsAndEvents.Add(calName, calendarEvents);

                count++;
            }

            return calendarsAndEvents;
        }

        #region DRAW

        void FilterIntoYearAndDayCalendars()
        {
            yearQueue.Clear();
            dayQueue.Clear();

            foreach (var (calendarName, eventList) in calendarEvents)
            {
                if (!displayedCalendars.Contains(calendarName)) continue;

                foreach (MyEvent calendarEvent in eventList)
                {
                    //Debug.Log($"{calendarEvent.title}:{prettyDate(calendarEvent.start)}-{prettyDate(calendarEvent.end)}");

                    if (calendarEvent.isYearEvent)
                    {
                        yearQueue.Add(calendarEvent);
                    }
                    else
                    {
                        dayQueue.Add(calendarEvent);
                    }
                }
            }

            DateTime date2 = DateTime.Now;
            DrawYearCalendar(date2, SolarClock.Instance.transform);
            DrawDayCalendar(date2, SolarClock.Instance.earth.earthSys.transform);
            Toggle(true);
        }
      
        public void AddDisplayedCalendar(string calToDisplay)
        {
            displayedCalendars.Add(calToDisplay);
            FilterIntoYearAndDayCalendars();
        }

        public void RemoveDisplayedCalendar(string calToRemove)
        {
            if (displayedCalendars.Contains(calToRemove))
            {
                displayedCalendars.Remove(calToRemove);
            }

            FilterIntoYearAndDayCalendars();
        }
        
        public void DrawYearCalendar(DateTime passDate, Transform solarClock)
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
                    DrawEvent(
                        ev,
                        true,
                        displayedCalendars.IndexOf(ev.parentCalendar)).transform.SetParent(yearCal.transform, false);
                }
            }

            yearCal.transform.SetParent(solarClock, false);
        }

        public void DrawDayCalendar(DateTime passDate, Transform earthSysTransform)
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
                    DrawEvent(
                        ev,
                        false,
                        displayedCalendars.IndexOf(ev.parentCalendar)).transform.SetParent(dayCal.transform, false);
                }
            }

            dayCal.transform.SetParent(earthSysTransform, false);
        }

        Circle DrawEvent(MyEvent passEv, bool isYearEvent, int index)
        {
            float evH = earthR * .1166f;
            float R = 0;
            float prct = 0;
            string displayTitle;
            TimeSpan span = new(passEv.end.Ticks - passEv.start.Ticks);
            if (isYearEvent)
            {
                R = calR - earthR * .0166f - index * evH;
                prct = (float) span.TotalDays / YEAR;
                displayTitle = passEv.title[..Math.Min((int) span.TotalDays, passEv.title.Length)];
            }
            else
            {
                R = earthR - earthR * .0166f - index * evH;
                prct = (float) span.TotalHours / 24;
                displayTitle = passEv.title[..Math.Min((int) span.TotalHours * 4, passEv.title.Length)];
            }

            Circle newRing = PolygonFactory.NewCirclePoly(PolygonFactory.Instance.mainMat);
            newRing.DrawRing(R, R - evH, prct, 0);
            newRing.SetColor(passEv.color);

            TextBox titleT = TextBox.Create(displayTitle, TextAlignmentOptions.Center);
            titleT.Size = 50;
            titleT.transform.SetParent(newRing.transform, false);

            float halfAng = (prct * .5f);
            titleT.transform.localPosition = new Vector3(
                (R - earthR * .1666f) * Mathf.Sin(halfAng * 2 * Mathf.PI),
                0,
                (R - earthR * .1666f) * Mathf.Cos(halfAng * 2 * Mathf.PI));
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
                    ? Orbits.GetEarthOrbitAngle(passEv.end)
                    : -360 * (passEv.end.Hour + passEv.end.Minute / 60f) / 24f,
                Vector3.up);

            return newRing;
        }

        void Toggle(bool toShow)
        {
            yearCal.SetActive(toShow);
            dayCal.SetActive(toShow);
        }

        #endregion

        #region REFERENCE
        
        public static int ConvertMonth(int passMonth)
        {
            return passMonth > 11 ? 0 : passMonth;
        }

        public static int ConvertDaytoInt(string passDay)
        {
            return passDay switch
            {
                "Sunday" => 0,
                "Monday" => 1,
                "Tuesday" => 2,
                "Wednesday" => 3,
                "Thursday" => 4,
                "Friday" => 5,
                "Saturday" => 6,
                _ => 0
            };
        }

        public static int ConvertAbrDaytoInt(string passDay)
        {
            return passDay switch
            {
                "SUN" => 0,
                "MON" => 1,
                "TUE" => 2,
                "WED" => 3,
                "THU" => 4,
                "FRI" => 5,
                "SAT" => 6,
                _ => 0
            };
        }

        public static int ConverMonthtoInt(string passDay)
        {
            return passDay switch
            {
                "JAN" => 0,
                "FEB" => 1,
                "MAR" => 2,
                "APR" => 3,
                "MAY" => 4,
                "JUN" => 5,
                "JUL" => 6,
                "AUG" => 7,
                "SEP" => 8,
                "OCT" => 9,
                "NOV" => 10,
                "DEC" => 11,
                _ => 0
            };
        }

        static DateTime CreateDate(int yr, int month, int day, int hour, int min)
        {
            DateTime newDate = new();
            newDate = newDate.AddYears(yr - 1);
            newDate = newDate.AddMonths(month - 1);
            newDate = newDate.AddDays(day - 1);
            newDate = newDate.AddHours(hour);
            newDate = newDate.AddMinutes(min);
            return newDate;
        }
        
        string PrettyDate(DateTime dateTime)
        {
            return $"{dateTime.Year}/{dateTime.Month}/{dateTime.Day}";
        }

        #endregion
        
        void TestCalendar()
        {
            calendarEvents = new Dictionary<string, MyEvent[]>();
            DateTime now = DateTime.Now;
            string testCal = "1:testCal";
            MyEvent testYearEvent = new(
                testCal,
                "TEST",
                CreateDate(now.Year, 1, 1, 0, 0),
                CreateDate(now.Year, 2, 1, 0, 0),
                EventColor,
                0);

            MyEvent testDayEvent =
                new(
                    testCal,
                    "TEST",
                    CreateDate(now.Year, now.Month, now.Day, 1, 0),
                    CreateDate(now.Year, now.Month, now.Day, 5, 0),
                    EventColor,
                    0);

            calendarEvents.Add(testCal, new[] {testYearEvent, testDayEvent});

            string testCal2 = "2:testCal";
            MyEvent testYearEvent2 = new(
                testCal2,
                "TEST",
                CreateDate(now.Year, 1, 1, 0, 0),
                CreateDate(now.Year, 2, 1, 0, 0),
                EventColor,
                0);

            MyEvent testDayEvent2 =
                new(
                    testCal2,
                    "TEST",
                    CreateDate(now.Year, now.Month, now.Day, 1, 0),
                    CreateDate(now.Year, now.Month, now.Day, 5, 0),
                    EventColor,
                    0);

            calendarEvents.Add(testCal2, new[] {testYearEvent2, testDayEvent2});

            CalendarMenu calendarMenu = GameObject.Find("Menu").GetComponent<CalendarMenu>();
            calendarMenu.PassCalendars(calendarEvents.Keys.ToArray());
        }
    }
}
