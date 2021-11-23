using System;
using Assets;
using Assets.UI.Elements;
using Assets.UI.Text;
using TMPro;

namespace UnityEngine
{
    public class CalendarMenu : MonoBehaviour
    {
        public ButtonMenu calendars;
        public Button showHideButton;

        void Start()
        {
            showHideButton = Button.Create("Calendars", TextBox.FontType.MainFont, 200, TextAlignmentOptions.Center);
            showHideButton.Pad = 20;
            showHideButton.transform.SetParent(transform, false);
            if (Application.platform == RuntimePlatform.Android)
            {
                showHideButton.transform.localPosition = new Vector3(
                    0,
                    -240,
                    100);
            }
            else
            {
                showHideButton.transform.localPosition = new Vector3(
                    -200,
                    -100,
                    100);
            }

            showHideButton.ToggleButton = true;
            showHideButton.SelectionAction = () =>
            {
                calendars.Show(!showHideButton.isToggled);
            };
            
            calendars = ButtonMenu.NewMenu(
                Array.Empty<string>(), 
                "calendars", 
                ButtonMenu.Layout.List, 
                30);
            
            calendars.transform.SetParent(transform, false);
            if (Application.platform == RuntimePlatform.Android)
            {
                calendars.transform.localPosition = new Vector3(
                    -40,
                    270,
                    100);
            }
            else
            {
                calendars.transform.localPosition = new Vector3(
                    -240,
                    100,
                    100);
            }

            calendars.Show(false);
        }

        public void PassCalendars(string[] calendarNames)
        {
            foreach (string calName in calendarNames)
            {
                Button calSelectButton = Button.Create(calName, TextBox.FontType.MainFont, 200, TextAlignmentOptions.Left);
                calSelectButton.ToggleButton = true;
                calSelectButton.SelectionAction = () =>
                {
                    AddOrRemove(!calSelectButton.isToggled, calName);
                };
                calendars.AddButton(calSelectButton);
            }
        }

        static void AddOrRemove(bool add, string calName)
        {
            if (add)
            {
                SolarClock.Instance.calendar.AddDisplayedCalendar(calName);
            }
            else
            {
                SolarClock.Instance.calendar.RemoveDisplayedCalendar(calName);
            }
        }
    }
}