using System;
using UnityEngine;
using System.Collections.Generic;
using Assets.UI.Text;
using TMPro;
using UnityEngine.UI;
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
        
        static MyEvent[] RetrieveCalendarEvents()
        {
            DateTime currentYearJan1 = new DateTime(System.DateTime.Now.Year, 1, 1);
            
            MyEvent[] calendarEvents = {};
            using (AndroidJavaClass javaClass = new AndroidJavaClass("com.example.calendar.calendarlibrary.main.EventsActivity"))
            {
                using (AndroidJavaObject activity = javaClass.GetStatic<AndroidJavaObject>("ctx"))
                {
                    AndroidJavaObject obj = activity.Call<AndroidJavaObject>("getAllEventsStartEnd");
                    int[] eventsStartEnd = obj.GetRawObject().ToInt32() != 0 
                        ? AndroidJNIHelper.ConvertFromJNIArray<int[]>(obj.GetRawObject()) 
                        : new int[0];
                    obj.Dispose();
                    
                    AndroidJavaObject obj2 = activity.Call<AndroidJavaObject>("getAllEventsTitles");
                    byte[] eventsTitles = obj2.GetRawObject().ToInt32() != 0 
                        ? AndroidJNIHelper.ConvertFromJNIArray<byte[]>(obj2.GetRawObject()) 
                        : new byte[0];
                    obj2.Dispose();

                    string rectify = System.Text.Encoding.Default.GetString(eventsTitles);
                    string[] split = rectify.Split('|');
                    
                    calendarEvents = new MyEvent[split.Length];
                    for (int i = 0; i < calendarEvents.Length; i++)
                    {
                        string title = split[i];
                        if (i * 2 > eventsStartEnd.Length - 2) break;

                        int start = eventsStartEnd[i * 2];
                        DateTime startDate = currentYearJan1.AddMilliseconds(start);
                        int end = eventsStartEnd[i * 2 + 1];
                        DateTime endDate = currentYearJan1.AddMilliseconds(end);

                        //int TimeZone = Earth.GetTimeZone();

                        calendarEvents[i] = new MyEvent(
                            title,
                            startDate,
                            endDate,
                            GetEventColor(title)
                        );
                    }
                }
            }

            return calendarEvents;
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
            yearQueue.Add(new MyEvent(
                "TEST",
                CreateDate(2019, 1, 1, 0, 0),
                CreateDate(2019,2, 1, 0, 0), 
                Color.red));
            
            dayQueue.Add(
                new MyEvent(
                    "TEST",
                    CreateDate(2019, 2, 24, 1, 0),
                    CreateDate(2019,2, 24, 5, 0), 
                    Color.red));
*/
            
            if(Application.platform != RuntimePlatform.Android) return;
            
            MyEvent[] calendarEvents = RetrieveCalendarEvents();
            foreach (MyEvent calendarEvent in calendarEvents)
            {
                long ticks = calendarEvent.end.Ticks - calendarEvent.start.Ticks;

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

        public void DrawYearCalendar(System.DateTime passDate, Transform solarClock)
        {
            if (yearCal)
            {
                Object.Destroy(yearCal);
            }

            yearCal = new GameObject("YearCal");
            yearCal.transform.Rotate(Vector3.right * -90);
            foreach (MyEvent ev in yearQueue)
            {
                if (ev.start.Year == passDate.Year)
                {
                    DrawEvent(ev, "year").transform.SetParent(yearCal.transform, false);
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
            dayCal.transform.Rotate((Vector3.right * -90));
            foreach (MyEvent ev in dayQueue)
            {
                if (ev.start.Year == passDate.Year &&
                    ev.start.Month == passDate.Month &&
                    ev.start.Day == passDate.Day)
                {
                    DrawEvent(ev, "day").transform.SetParent(dayCal.transform, false);
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

        GameObject DrawEvent(MyEvent passEv, string type)
        {
            const float evH = 7;
            float R = 0;
            float prct = 0;
            int one = -1;
            if (type == "year")
            {
                R = calR - 1;
                one = 1;
                prct = (Orbits.GetEarthOrbitAngle(passEv.start, YEAR) - 
                        Orbits.GetEarthOrbitAngle(passEv.end, YEAR)) / 
                       YEAR;
                prct += 1 / YEAR;
            }
            else
            {
                R = earthR - 1;
                prct = ((passEv.start.Hour - passEv.end.Hour) +
                        (passEv.start.Minute - passEv.end.Minute) / 60f) / 24f;
            }

            GameObject newRing = Shapes.DrawRing(R, R - evH, one * prct, passEv.color, 0, false);

            string displayTitle = passEv.title;
            if (displayTitle.Length > 12)
            {
                displayTitle = displayTitle.Substring(0, 12);
            }
            TextBox titleT = TextBox.Create(displayTitle, TextBox.FontType.MainFont, 50, TextAlignmentOptions.Center);
            titleT.transform.SetParent(newRing.transform);
            titleT.transform.Translate(Vector3.up * (R - 10));
            titleT.transform.parent.Rotate(Vector3.up, one * (prct * .5f) * 360 + 180);
            //titleT.gameObject.SetActive(false);

            newRing.transform.rotation = Quaternion.AngleAxis(
                type == "year"
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
