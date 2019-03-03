using System;
using UnityEngine;
using UnityEngine.UI;

namespace Assets
{
    public class DigitalClock : MonoBehaviour
    {
        Text date;
        Text time;
        Text sundialText;

        void Awake()
        {
            sundialText = transform.GetChild(0).GetComponent<Text>();
            sundialText.text = "SUNDIAL";

            date = transform.GetChild(1).GetComponent<Text>();
            time = transform.GetChild(2).GetComponent<Text>();

            SetTime(DateTime.Now);
        }

        static string GetDate(DateTime passDate)
        {
            string day = passDate.Day.ToString();
            if (passDate.Day < 10)
                day = "0" + day;

            string month = passDate.Month.ToString();
            if (passDate.Month < 10)
                month = "0" + month;

            string year = passDate.Year.ToString();
            year = year[2].ToString() + year[3].ToString();
            return day + " / " + month + " / " + year;
        }

        static string GetTime(DateTime passDate)
        {
            string sec = passDate.Second.ToString();
            string min = passDate.Minute.ToString();
            string hr = passDate.Hour.ToString();
            if (passDate.Second < 10)
                sec = "0" + sec;
            if (passDate.Minute < 10)
                min = "0" + min;
            if (passDate.Hour < 10)
                hr = "0" + hr;
            return hr + " : " + min + " : " + sec;
        }

        public void SetTime(DateTime passDate)
        {
            // sundialText.text = GetDate(passDate).ToString() + "\r\n" + "SUNDIAL\r\n" + GetTime(passDate).ToString();
            if (gameObject.activeInHierarchy)
            {
                date.text = GetDate(passDate);
                time.text = GetTime(passDate);
            }
        }
    }
}