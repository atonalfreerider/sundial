using UnityEngine;
using System.Collections.Generic;
using System.IO;
using UnityEngine.UI;

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

        const string loadFile = "johnbvoorhees_calendar.txt";

        float sunSprockOffset;

        // INIT Functions;
        public void Init(float passYEAR, float passCR, float passER, float passSunSprockOffset)
        {
            YEAR = passYEAR;
            calR = passCR;
            earthR = passER;
            sunSprockOffset = passSunSprockOffset;

            StreamReader or = File.OpenText(loadFile);
            string calText = or.ReadToEnd();

            calText = StringParse.ChopBlock(calText, "END:VTIMEZONE\r\n");

            yearQueue = new List<MyEvent>();
            dayQueue = new List<MyEvent>();

            string block1;
            int[] startDate;
            int[] endDate;
            string evName;
            string dtStart;
            string dtEnd;
            Color evColor;
            int TimeZone = Earth.GetTimeZone();

            while (calText.Contains("END:VEVENT"))
            {
                block1 = StringParse.Block(calText, "BEGIN:VEVENT", "END:VEVENT");
                evName = StringParse.TextAfterChar(StringParse.GetLine(block1, "SUMMARY:"), ":");
                switch (evName)
                {
                    case "ABQ":
                        evColor = new Color(1f, 1f, 0f, .5f);
                        break;
                    case "SF":
                        evColor = new Color(1f, 0f, 0f, .5f);
                        break;
                    case "Bay Area":
                        evColor = new Color(1f, 0f, 0f, .5f);
                        break;
                    case "LA":
                        evColor = new Color(1f, 0f, 0f, .5f);
                        break;
                    case "BOULDER":
                        evColor = new Color(0f, 1f, 0f, .5f);
                        break;
                    case "Boulder":
                        evColor = new Color(0f, 1f, 0f, .5f);
                        break;
                    default:
                        evColor = new Color(0f, 0f, 1f, .5f);
                        break;
                }

                dtStart = StringParse.GetLine(block1, "DTSTART");
                dtEnd = StringParse.GetLine(block1, "DTEND");
                if (dtStart.Contains(";"))
                {
                    // daylong event;
                    startDate = StringParse.ShortDateFromString(StringParse.TextAfterChar(dtStart, ":"));
                    endDate = StringParse.ShortDateFromString(StringParse.TextAfterChar(dtEnd, ":"));
                    yearQueue.Add(new MyEvent(
                        evName, 
                        CreateDate(
                            startDate[0], 
                            startDate[1], 
                            startDate[2], 
                            0, 
                            0),
                        CreateDate(
                            endDate[0], 
                            endDate[1], 
                            endDate[2], 
                            0, 
                            0),
                        evColor));
                }
                else
                {
                    // hourly event;
                    startDate = StringParse.DateFromString(StringParse.TextAfterChar(dtStart, ":"));
                    endDate = StringParse.DateFromString(StringParse.TextAfterChar(dtEnd, ":"));
                    dayQueue.Add(new MyEvent(
                        evName,
                        CreateDate(
                            startDate[0],
                            startDate[1], 
                            startDate[2], 
                            startDate[3] + TimeZone,
                            startDate[4]),
                        CreateDate(
                            endDate[0], 
                            endDate[1],
                            endDate[2],
                            endDate[3] + TimeZone,
                            endDate[4]), 
                        evColor));
                }

                calText = StringParse.ChopBlock(calText, "END:VEVENT\r\n");
                // Debug.Log("Y:" + startDate[0].ToString() + ",M:" + startDate[1].ToString() + "D:" + startDate[2].ToString() + "H:" + startDate[3].ToString() + "M:" + startDate[4].ToString());
            }
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

            Text titleT = Items.NewText(passEv.title, passEv.color, 50, TextAnchor.MiddleCenter, false);
            titleT.transform.SetParent(newRing.transform);
            titleT.transform.Translate(Vector3.down * (R - 6));
            titleT.transform.parent.Rotate(Vector3.up, one * (prct * .5f) * 360 + 180);
            titleT.gameObject.SetActive(false);

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
