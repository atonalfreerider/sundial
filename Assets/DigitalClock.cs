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
            /*
                    Text sundial = Items.NewText("SUNDIAL", Color.white, 30, TextAnchor.MiddleCenter, true);
                    sundial.transform.SetParent(this.transform);
                    sundial.transform.Translate(Vector3.down * 1f);
                    */
            sundialText = transform.GetChild(0).GetComponent<Text>();
            sundialText.text = "SUNDIAL";

            /*
            date = Items.NewText(GetDate(System.DateTime.Now), Color.white, 60, TextAnchor.MiddleCenter, true);
            date.transform.SetParent(this.transform);
            date.transform.Translate(Vector3.down * 3f);
            date.transform.Translate(Vector3.forward * 5f);
    
            time = Items.NewText(GetTime(System.DateTime.Now), Color.white, 60, TextAnchor.MiddleCenter, true);
            time.transform.SetParent(this.transform);
            time.transform.Translate(Vector3.down * 3f);
            time.transform.Translate(Vector3.back * 5f);
            */

            date = transform.GetChild(1).GetComponent<Text>();
            time = transform.GetChild(2).GetComponent<Text>();

            SetTime(System.DateTime.Now);
        }

        static string GetDate(System.DateTime passDate)
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

        static string GetTime(System.DateTime passDate)
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

        public void SetTime(System.DateTime passDate)
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