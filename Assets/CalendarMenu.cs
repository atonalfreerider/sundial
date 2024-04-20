using Assets.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Assets
{
    [RequireComponent(typeof(Menu))]
    public class CalendarMenu : MonoBehaviour
    {
     
        public void PassCalendars(string[] calendarNames)
        {
            Menu menu = GetComponent<Menu>();
            foreach (string calName in calendarNames)
            {
                GameObject calSelectButtonGO = new GameObject(calName);
                RectTransform rectTransform = calSelectButtonGO.AddComponent<RectTransform>();
                rectTransform.sizeDelta = new Vector2(250, 80);
                calSelectButtonGO.AddComponent<CanvasRenderer>();
                Image image = calSelectButtonGO.AddComponent<Image>();
                image.color = Color.black;
                Outline outline = calSelectButtonGO.AddComponent<Outline>();
                outline.effectColor = new Color(1, 1, 1, 0.5f);
                Toggle calSelectButton = calSelectButtonGO.AddComponent<Toggle>();
                calSelectButton.onValueChanged.AddListener((x) => AddOrRemove(x, calName));
                HorizontalLayoutGroup horizontalLayoutGroup = calSelectButtonGO.AddComponent<HorizontalLayoutGroup>();
                horizontalLayoutGroup.childControlWidth = true;
                horizontalLayoutGroup.childAlignment = TextAnchor.MiddleCenter;
                
                Text text = new GameObject("Text").AddComponent<Text>();
                text.alignment = TextAnchor.MiddleCenter;
                text.horizontalOverflow = HorizontalWrapMode.Wrap;
                text.font = Resources.Load<Font>("FRAMDCN");
                text.text = calName;
                text.fontSize = 40;
                text.transform.SetParent(calSelectButtonGO.transform);
                
                menu.GetComponent<Menu>().AddCalendarButton(calSelectButtonGO);
                calSelectButtonGO.SetActive(false);
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