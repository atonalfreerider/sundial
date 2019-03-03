using System.Threading.Tasks;
using Assets.GraphicsUtil.Shapes;
using Assets.GraphicsUtil.Shapes.Lines;
using Assets.UI.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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

        public float Pad = 0;
        Color normalColor = new Color(0, 0, 0, 0.02f);
        public Vector2 Size = Vector2.zero;

        public TMP_SpriteAsset SpriteAsset
        {
            set { textBox.SpriteAsset = value; }
        }

        #region Actions

        public delegate void TakeAction();

        public delegate Task TakeAsyncAction();

        public TakeAction SelectionAction { private get; set; }
        public TakeAsyncAction AsyncSelectionAction { private get; set; }
        public TakeAction HighlightAction { private get; set; }
        public TakeAction UnhighlightAction { private get; set; }

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
            textBox.RectTransformDimensionsChange +=
                button.OnRectTransformDimensionsChange;

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
                    Vector3.zero, new Vector3(0.001f, 0, 0)
                },
                1,
                false,
                2);
            button.buttonOutline.transform.SetParent(button.transform, false);
            button.buttonOutline.transform.localRotation =
                Quaternion.AngleAxis(90, Vector3.right);
            button.buttonOutline.SetColor(Color.white);

            button.RedrawButtonShape();

            return button;
        }

        void OnRectTransformDimensionsChange()
        {
            RedrawButtonShape();
        }

        void RedrawButtonShape()
        {
            Size = textBox.RectTransform.rect.size;
            Vector3 center = Vector3.zero;
            switch (textBox.Alignment) {
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
                buttonBack.transform.localPosition = center;
            }

            boxCollider.size = new Vector3(Size.x + Pad, Size.y + Pad, 10);
            boxCollider.center = center;

            Vector3[] frame =
            {
                new Vector3(-Size.x * 0.5f - Pad, 0, Size.y * 0.5f + Pad * 0.5f),
                new Vector3(Size.x * 0.5f + Pad, 0, Size.y * 0.5f + Pad * 0.5f),
                new Vector3(Size.x * 0.5f + Pad, 0, -Size.y * 0.5f - Pad * 0.5f),
                new Vector3(-Size.x * 0.5f - Pad, 0, -Size.y * 0.5f - Pad * 0.5f)
            };

            buttonOutline.DrawLine(frame, .5f, true, 2);
            buttonOutline.transform.localPosition = center;
        }

        public void Set(string buttonText)
        {
            textBox.Text = buttonText;
            name = buttonText;
        }

        void PushButton()
        {
            if (isLoadingBar) { return; }
            
            SelectionAction?.Invoke();
            AsyncSelectionAction?.Invoke();
            if (!ToggleButton)
            {
                buttonBack.SetColor(ColorForState(ButtonState.Pushed));
                buttonBack.SetColor(
                    ColorForState(ButtonState.Normal),
                    0);
            }
            else
            {
                SetToggleState(!isToggled);
            }
        }

        public void SetLoadStatus(float prct)
        {
            Vector2 dim = textBox.RectTransform.rect.size;
            Vector3 center = Vector3.zero;
            switch (textBox.Alignment)
            {
                case TextAlignmentOptions.Left:
                    center = new Vector3((dim.x + Pad) * 0.5f * prct - Pad * 0.5f, 0, 0);
                    break;
                case TextAlignmentOptions.Center:
                    center = new Vector3((dim.x + Pad) * 0.5f * prct - (dim.x + Pad) * 0.5f, 0, 0);
                    break;
            }

            buttonBack.transform.localScale =
                new Vector3((dim.x + Pad) * prct, 1, dim.y + Pad * 0.5f);
            buttonBack.transform.localPosition = center;
        }

        public void SetToggleState(bool isToggled)
        {
            buttonBack.SetColor(isToggled ? 
                ColorForState(ButtonState.Pushed) : 
                ColorForState(ButtonState.Normal));

            this.isToggled = isToggled;
        }

        public void MakeLoadingBar()
        {
            isLoadingBar = true;
            buttonBack.transform.localScale = new Vector3(.01f, 1, .01f);
            buttonBack.transform.localPosition = Vector3.zero;
        }

        public void SetCollider(bool isEnabled)
        {
            boxCollider.enabled = isEnabled;
        }

        public Bounds Bounds => textBox.Bounds;

        public void SetBoxCollider(bool isEnabled)
        {
            boxCollider.enabled = isEnabled;
        }

        public void RemoveFromParentMenu()
        {
            parentMenu.RemoveButton(this, 0.3f);
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
            textBox.ContentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            textBox.RectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
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
            if (ToggleButton && isToggled) { return; }

            if (!isLoadingBar)
            {
                buttonBack.SetColor(ColorForState(newState));
            }
        }

        public void RequestSelection()
        {
            PushButton();
        }

        public void RequestDeselection() { }

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