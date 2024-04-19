using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace Assets.UI
{
    public class Menu : MonoBehaviour
    {
        Button quitButton;
        Toggle showClockButton;
        Button galacticButton;
        readonly List<GameObject> calendarButtons = new List<GameObject>();
        
        void Awake()
        {
            quitButton = transform.GetChild(0).Find("Quit").GetComponent<Button>();
            quitButton.gameObject.SetActive(false);
            
            showClockButton = transform.GetChild(0).Find("ShowClock").GetComponent<Toggle>();
            showClockButton.gameObject.SetActive(false);
            
            galacticButton = transform.GetChild(0).Find("Galactic").GetComponent<Button>();
            galacticButton.gameObject.SetActive(false);
        }

        public void AddCalendarButton(GameObject button)
        {
            calendarButtons.Add(button);
            button.transform.SetParent(transform.GetChild(0));
        }

        public void OnMenuButtonClicked(bool isToggled)
        {
            quitButton.gameObject.SetActive(isToggled);
            showClockButton.gameObject.SetActive(isToggled);
            galacticButton.gameObject.SetActive(isToggled);
            foreach (GameObject calendarButton in calendarButtons)
            {
                calendarButton.SetActive(isToggled);
            }
        }
    }
}