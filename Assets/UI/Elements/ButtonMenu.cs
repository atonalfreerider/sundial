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
        public readonly Dictionary<string, Button> menuButtons = new Dictionary<string, Button>();

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

        public void AddButton(Button button, float animSec = 0)
        {
            button.transform.SetParent(transform, false);
            while (menuButtons.ContainsKey(button.name))
            {
                // add space at end so that keys are unique
                button.name += " ";
            }
            menuButtons.Add(button.name, button);
            button.parentMenu = this;
            DoLayout(animSec);
        }

        public void RemoveButton(Button button, float animSec = 0)
        {
            Destroy(button.gameObject);
            menuButtons.Remove(button.name);
            DoLayout(animSec);
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
        
        void SetRadial(float animSec = 0)
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

            if (animSec > float.Epsilon)
            {
                StartCoroutine(MoveToHomePositions(animSec));
            }
            else
            {
                foreach (Button button in menuButtons.Values)
                {
                    button.transform.localPosition = button.HomePosition;
                }
            }
        }

        IEnumerator SetList(float animSec = 0)
        {
            // 1 frame delay required to allow button size to get set
            yield return null;
            int count = 0;
            Button previous = null;
            foreach (Button button in menuButtons.Values)
            {
                button.HomePosition = new Vector3(
                    0, 
                    -count * (button.Size.y * .5f + (previous != null ? previous.Size.y * .5f : 0) + .07f),
                    0);
                previous = button;
                count++;
            }

            if (animSec > float.Epsilon)
            {
                StartCoroutine(MoveToHomePositions(animSec));
            }
            else
            {
                foreach (Button button in menuButtons.Values)
                {
                    button.transform.localPosition = button.HomePosition;
                }
            }
        }

        void DoLayout(float animSec = 0)
        {
            switch (menuLayout) {
                case Layout.Radial:
                    SetRadial(animSec);
                    break;
                case Layout.List:
                    if (!gameObject.activeInHierarchy) return;
                    StartCoroutine(SetList(animSec));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        IEnumerator MoveToHomePositions(float animSec)
        {
            // animate;
            float prog = 0;
            while (prog < animSec)
            {
                foreach (Button button in menuButtons.Values)
                    button.transform.localPosition = Vector3.Lerp(button.transform.localPosition, button.HomePosition, Time.deltaTime / (animSec - prog));

                yield return null;
                prog += Time.deltaTime;
            }

            // on complete;
            foreach (Button button in menuButtons.Values)
            {
                button.transform.localPosition = button.HomePosition;
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
