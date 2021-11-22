using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.UI.Text;
using UnityEngine;

namespace Assets.UI.Elements
{

    public class ButtonMenu : MonoBehaviour
    {
        public enum Layout
        {
            List, Radial
        }

        // persistent vars
        public readonly Dictionary<string, Button> menuButtons = new();

        // state vars
        Layout menuLayout;

        void Init(string[] buttonNames, Layout layout, float buttonFontSize)
        {
            TMPro.TextAlignmentOptions alignment = layout == Layout.List ? TMPro.TextAlignmentOptions.Left : TMPro.TextAlignmentOptions.Center;
            menuLayout = layout;
            
            foreach (string buttonName in buttonNames)
            {
                Button button = Button.Create(
                    buttonName,
                    TextBox.FontType.MainFont,
                    buttonFontSize,
                    alignment);
                AddButton(button);
            }
        }

        public void AddButton(Button button)
        {
            button.transform.SetParent(transform, false);
            while (menuButtons.ContainsKey(button.name))
            {
                // add space at end so that keys are unique
                button.name += " ";
            }
            menuButtons.Add(button.name, button);
            button.parentMenu = this;
            DoLayout();
        }

        public void RemoveButton(Button button)
        {
            Destroy(button.gameObject);
            menuButtons.Remove(button.name);
            DoLayout();
        }

        public void Clear()
        {
            foreach (Button button in menuButtons.Values)
            {
                Destroy(button.gameObject);
            }
            menuButtons.Clear();
        }

        public void Show(bool show)
        {
            if (!show)
            {
                StopAllCoroutines();
                gameObject.SetActive(false);
            }
            else
            {
                gameObject.SetActive(true);
                DoLayout();
            }
        }
        
        void SetRadial()
        {
            int longest = menuButtons.Keys.Select(buttonName => buttonName.Length).Concat(new[] {0}).Max();

            float R = Mathf.Max(.1f, .01f * longest);
            float prct;

            int count = 0;
            foreach (Button button in menuButtons.Values)
            {
                prct = (float)count / menuButtons.Values.Count;
                button.HomePosition = new Vector3(
                    R * Mathf.Sin(prct * 2f * Mathf.PI),
                    R * Mathf.Cos(prct * 2f * Mathf.PI),
                    0);
                count++;
            }

            foreach (Button button in menuButtons.Values)
            {
                button.transform.localPosition = button.HomePosition;
            }
        }

        IEnumerator SetList(float animSec = 0)
        {
            // 1 frame delay required to allow button size to get set
            yield return null;
            yield return null;
            int count = 0;
            Button previous = null;
            foreach (Button button in menuButtons.Values)
            {
                button.HomePosition = new Vector3(
                    0,
                    -count * (button.Size.y * .5f + (previous != null ? previous.Size.y * .5f : 0) + 7f),
                    0);
                previous = button;
                count++;
            }

            foreach (Button button in menuButtons.Values)
            {
                button.transform.localPosition = button.HomePosition;
            }
        }

        void DoLayout()
        {
            switch (menuLayout)
            {
                case Layout.Radial:
                    SetRadial();
                    break;
                case Layout.List:
                    if (!gameObject.activeInHierarchy) return;
                    StartCoroutine(SetList());
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void SetButtonColliders(bool isEnabled)
        {
            foreach (Button button in menuButtons.Values)
            {
                button.SetCollider(isEnabled);
            }
        }
        
        public static ButtonMenu NewMenu(string[] buttonNames, string name, Layout layout, float buttonFontSize)
        {            
            ButtonMenu newMenu = new GameObject(name).AddComponent<ButtonMenu>();
            newMenu.Init(buttonNames, layout, buttonFontSize);

            return newMenu;
        }
    }
}
