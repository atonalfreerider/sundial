using Assets;
using Assets.UI.Elements;
using Assets.UI.Text;
using TMPro;

namespace UnityEngine
{
    public class CalendarMenu : MonoBehaviour
    {
        public ButtonMenu calendars;
        private Button showHideButton;

        void Start()
        {
            showHideButton = Button.Create("Calendars", TextBox.FontType.MainFont, 100, TextAlignmentOptions.Left);
            showHideButton.Pad = 20;
            showHideButton.transform.SetParent(transform, false);
            showHideButton.transform.localPosition = new Vector3(
                30,
                270,
                100);
            showHideButton.ToggleButton = true;
            showHideButton.SelectionAction = () => { calendars.Show(!showHideButton.isToggled); };
            
            calendars = ButtonMenu.NewMenu(
                new string[0], 
                "calendars", 
                ButtonMenu.Layout.List, 
                20);
            
            calendars.transform.SetParent(transform, false);
            calendars.transform.localPosition = new Vector3(
                75,
                270,
                100);
            calendars.Show(false);
        }

        public void PassCalendars(string[] calendarNames)
        {
            foreach (string calName in calendarNames)
            {
                Button calSelectButton = Button.Create(calName, TextBox.FontType.MainFont, 100, TextAlignmentOptions.Left);
                calSelectButton.ToggleButton = true;
                calSelectButton.SelectionAction = () => { AddOrRemove(!calSelectButton.isToggled, calName); };
                calendars.AddButton(calSelectButton);
            }
        }

        void AddOrRemove(bool add, string calName)
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