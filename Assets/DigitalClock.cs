using System;
using Assets.UI.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets
{
    public class DigitalClock : MonoBehaviour
    {
        TextBox date;
        TextBox time;
        TextBox sundialText;

        void Awake()
        {
            sundialText = TextBox.Create("SUNDIAL", TextBox.FontType.MainFont, 80, TextAlignmentOptions.Center);
            sundialText.transform.SetParent(transform, false);

            date = TextBox.Create("", TextBox.FontType.MainFont, 140, TextAlignmentOptions.Center);
            date.transform.SetParent(transform, false);
            date.transform.Translate(Vector3.down * 10);
            time = TextBox.Create("", TextBox.FontType.MainFont, 140, TextAlignmentOptions.Center);
            time.transform.SetParent(transform, false);
            time.transform.Translate(Vector3.up * 10);
            
            SetTime(DateTime.Now);
        }

        static string GetDate(DateTime passDate)
        {
            string day = passDate.Day.ToString();
            if (passDate.Day < 10)
            {
                day = "0" + day;
            }

            string month = passDate.Month.ToString();
            if (passDate.Month < 10)
            {
                month = "0" + month;
            }

            string year = passDate.Year.ToString();
            year = year.Substring(2, 2);
            return $"{day}/{month}/{year}";
        }

        static string GetTime(DateTime passDate)
        {
            string sec = passDate.Second.ToString();
            string min = passDate.Minute.ToString();
            string hr = passDate.Hour.ToString();
            if (passDate.Second < 10)
            {
                sec = "0" + sec;
            }

            if (passDate.Minute < 10)
            {
                min = "0" + min;
            }

            if (passDate.Hour < 10)
            {
                hr = "0" + hr;
            }

            return $"{hr} : {min} : {sec}";
        }

        public void SetTime(DateTime passDate)
        {
            // sundialText.text = GetDate(passDate).ToString() + "\r\n" + "SUNDIAL\r\n" + GetTime(passDate).ToString();
            if (gameObject.activeInHierarchy)
            {
                date.Text = GetDate(passDate);
                time.Text = GetTime(passDate);
            }
        }
    }
}