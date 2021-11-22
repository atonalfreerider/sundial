using System.Collections;
using Assets.GraphicsUtil.Shapes;
using Assets.GraphicsUtil.Shapes.Lines;
using Assets.UI.Text;
using TMPro;
using UnityEngine;

namespace Assets.UI.Elements
{
    public class Button : MonoBehaviour, ISelectable
    {
        TextBox textBox;

        Rectangle buttonBack;
        Line buttonOutline;
        BoxCollider boxCollider;
        public ButtonMenu parentMenu;

        public Vector3 HomePosition;
        public bool ToggleButton = false;
        public bool isToggled = false;
        bool isLoadingBar = false;
        public string toggleString = "";

        public float Pad = 0;
        Color normalColor = new(0, 0, 0, 0.02f);
        public Vector2 Size = Vector2.zero;
        readonly Vector2 loadBarDim = new(.5f, .035f);

        #region Actions

        public delegate void TakeAction();

        public delegate void TakeActionWithBool(bool toggle);
        
        public TakeAction SelectionAction { private get; set; }
        public TakeActionWithBool SelectionActionWithBool { private get; set; }
        
        TakeAction HighlightAction { get; set; }
        TakeAction UnhighlightAction { get; set; }

        #endregion

        public static Button Create(
            string buttonText,
            TextBox.FontType fontType,
            float fontSize,
            TextAlignmentOptions align)
        {
            TextBox textBox =
                TextBox.Create(buttonText, fontType, fontSize, align);

            GameObject gameObject = textBox.gameObject;
            gameObject.name = $"Button: {buttonText}";
            Button button = gameObject.AddComponent<Button>();
            button.textBox = textBox;
            textBox.TextField.OnPreRenderText += button.DoDelayBounds;

            button.buttonBack = Instantiate(NewCube.transRectPoly, button.transform, false);
            button.buttonBack.transform.localRotation =
                Quaternion.AngleAxis(90, Vector3.right);
            button.buttonBack.SetColor(button.ColorForState(ButtonState.Normal));

            button.boxCollider = gameObject.AddComponent<BoxCollider>();

            // TODO: this can be refactored into 4 scalable lines...
            button.buttonOutline = PolygonFactory.NewLinePoly(SolarClock.Instance.mainMat, false);
            button.buttonOutline.DrawLine(
                new[]
                {
                    Vector3.zero, new Vector3(0.1f, 0, 0)
                },
                0.1f,
                false,
                2);
            button.buttonOutline.transform.SetParent(button.transform, false);
            button.buttonOutline.transform.localRotation =
                Quaternion.AngleAxis(90, Vector3.right);
            button.buttonOutline.SetColor(Color.white);
            button.DoDelayBounds(textBox.TextField);

            return button;
        }

        void DoDelayBounds(TMP_Text textComponent)
        {
            StartCoroutine(DelayBounds(textComponent));
        }

        void DoDelayBounds(TMP_TextInfo textInfo)
        {
            StartCoroutine(DelayBounds(textInfo.textComponent));
        }

        IEnumerator DelayBounds(TMP_Text textComponent)
        {
            yield return null;

            Vector2 size = textComponent.rectTransform.rect.size;
            if (size.magnitude < float.Epsilon)
            {
                size = new Vector2(textComponent.bounds.size.x, textComponent.bounds.size.y);
            }

            RedrawButtonShape(textComponent, size);
        }

        void RedrawButtonShape(TMP_Text textComponent, Vector2 size)
        {
            Size = size;
            Vector3 center = Vector3.zero;
            switch (textComponent.alignment)
            {
                case TextAlignmentOptions.Left:
                    center = new Vector3((Size.x + Pad) * 0.5f - Pad * 0.5f, 0, 0);
                    break;
                case TextAlignmentOptions.Right:
                    center = new Vector3(-((Size.x + Pad) * 0.5f - Pad * 0.5f), 0, 0);
                    break;
            }

            if (!isLoadingBar)
            {
                buttonBack.transform.localScale =
                    new Vector3(Size.x + Pad, 1, Size.y + Pad * 0.5f);
                buttonBack.transform.localPosition = center + transform.forward * .001f;
            }

            boxCollider.size = new Vector3(Size.x, Size.y, 0.05f);
            boxCollider.center = center;

            Vector3[] frame =
            {
                new(-Size.x * 0.5f - Pad, 0, Size.y * 0.5f + Pad * 0.5f),
                new(Size.x * 0.5f + Pad, 0, Size.y * 0.5f + Pad * 0.5f),
                new(Size.x * 0.5f + Pad, 0, -Size.y * 0.5f - Pad * 0.5f),
                new(-Size.x * 0.5f - Pad, 0, -Size.y * 0.5f - Pad * 0.5f)
            };

            buttonOutline.DrawLine(frame, 0.002f, true, 2);
            buttonOutline.transform.localPosition = center;
        }

        public void Set(string buttonText)
        {
            textBox.Text = buttonText;
            name = buttonText;
        }

        void PushButton()
        {
            if (isLoadingBar) return;

            SelectionAction?.Invoke();
            SelectionActionWithBool?.Invoke(!isToggled);
        }

        public void SetLoadStatus(float prct)
        {
            DrawBar(prct, loadBarDim);
            buttonBack.transform.localScale = new Vector3(
                (loadBarDim.x + Pad) * prct,
                1,
                loadBarDim.y + Pad * 0.5f);
        }

        public void SetIndeterminate(float prct)
        {
            DrawBar(prct * 2, loadBarDim);
            buttonBack.transform.localScale = new Vector3(
                (loadBarDim.x + Pad) * .1f,
                1,
                loadBarDim.y + Pad * 0.5f);
        }

        void DrawBar(float prct, Vector2 dim)
        {
            Vector3 center = Vector3.zero;
            switch (textBox.Alignment)
            {
                case TextAlignmentOptions.Left:
                    center = new Vector3(
                        (dim.x + Pad) * 0.5f * prct - Pad * 0.5f,
                        0,
                        0);
                    break;
                case TextAlignmentOptions.Center:
                    center = new Vector3(
                        (dim.x + Pad) * 0.5f * prct - (dim.x + Pad) * 0.5f,
                        0,
                        0);
                    break;
            }

            buttonBack.transform.localPosition = center;
        }

        public void MakeLoadingBar()
        {
            isLoadingBar = true;
            SetFixedSize(new Vector2(.5f, .035f));
            buttonBack.transform.localScale = new Vector3(.01f, 1, .01f);
            buttonBack.transform.localPosition = Vector3.zero;
        }

        public void SetFixedSize(Vector2 size)
        {
            textBox.TextField.overflowMode = TextOverflowModes.Ellipsis;
            textBox.TextField.enableWordWrapping = false;
            textBox.TextField.autoSizeTextContainer = false;
            textBox.TextField.rectTransform.sizeDelta = size;
        }

        public void SetCollider(bool isEnabled)
        {
            boxCollider.enabled = isEnabled;
        }

        public void RemoveFromParentMenu()
        {
            if (parentMenu == null) return;
            
            parentMenu.RemoveButton(this);
        }

        public void SetColor(Color textColor, Color backColor, Color outlineColor)
        {
            normalColor = backColor;
            buttonBack.SetColor(normalColor);
            textBox.Color = textColor;
            buttonOutline.SetColor(outlineColor);
        }

        public void SetWrap(float width)
        {
            textBox.TextField.enableWordWrapping = true;
            textBox.TextField.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
            textBox.TextField.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, textBox.TextField.preferredHeight);
        }

        #region ISelectable

        public Transform SelectionTarget => transform;

        public void Highlight()
        {
            HighlightButton(ButtonState.Selected);
            HighlightAction?.Invoke();
        }

        public void Unhighlight()
        {
            HighlightButton(ButtonState.Normal);
            UnhighlightAction?.Invoke();
        }

        void HighlightButton(ButtonState newState)
        {
            if (ToggleButton || isToggled) return;

            if (!isLoadingBar && buttonBack != null)
            {
                buttonBack.SetColor(ColorForState(newState));
            }
        }

        public void RequestSelection()
        {
            PushButton();
        }

        public bool RequestDeselection()
        {
            return false;
        }

        #endregion

        #region Button State & Color

        enum ButtonState
        {
            Normal,
            /// <summary>
            /// "Hover/highlight" state
            /// </summary>
            Selected,
            Pushed
        }

        Color ColorForState(ButtonState buttonState)
        {
            switch (buttonState)
            {
                case ButtonState.Normal:
                    return normalColor;
                case ButtonState.Selected:
                    return new Color(0.1f, 0,0.7f, 0.2f);
                case ButtonState.Pushed:
                    return new Color(1, 1, 0, 0.2f);
                default:
                    Debug.LogErrorFormat(
                        "Unknown `ButtonState` {0} {1}",
                        nameof(buttonState),
                        buttonState);
                    return new Color(1, 0, 1);
            }
        }

        #endregion
    }
}