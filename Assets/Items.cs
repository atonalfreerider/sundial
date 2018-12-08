using UnityEngine;
using UnityEngine.UI;

namespace Assets
{
    public static class Items
    {
        static Font mainFont;

        public static void Init(Font passFont)
        {
            mainFont = passFont;
        }

        public static void AddCanvas(GameObject passGO)
        {
            passGO.transform.Rotate(Vector3.right * 90f);
            RectTransform rectT = passGO.AddComponent<RectTransform>();
            rectT.sizeDelta = new Vector2(100f, 100f);
            Canvas can = passGO.AddComponent<Canvas>();
            can.renderMode = RenderMode.WorldSpace;
            passGO.AddComponent<CanvasScaler>();
            passGO.AddComponent<GraphicRaycaster>();
        }

        static void AddCanvas2(GameObject passGO)
        {
            passGO.transform.Rotate(Vector3.up * 90f);
            RectTransform rectT = passGO.AddComponent<RectTransform>();
            rectT.sizeDelta = new Vector2(100f, 100f);
            Canvas can = passGO.AddComponent<Canvas>();
            can.renderMode = RenderMode.WorldSpace;
            passGO.AddComponent<CanvasScaler>();
            passGO.AddComponent<GraphicRaycaster>();
        }

        public static Text NewText(string passText, Color color1, int fontSize, TextAnchor align, bool outline)
        {
            GameObject textBox = new GameObject();
            textBox.transform.Rotate(Vector3.right * 90f);

            Text text = textBox.AddComponent<Text>();
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.resizeTextMaxSize = 100;
            text.resizeTextMinSize = 1;
            //text.material = mainFontMat;        
            text.name = passText;
            text.font = mainFont;
            text.alignment = align;
            text.text = passText;
            text.fontSize = fontSize;
            text.color = color1;

            textBox.transform.localScale = new Vector3(.1f, .1f, .1f);

            return text;
        }

        static Text NewText2(string passText, Color color1, int fontSize, TextAnchor align, bool outline)
        {
            GameObject textBox = new GameObject();

            Text text = textBox.AddComponent<Text>();
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.resizeTextMaxSize = 100;
            text.resizeTextMinSize = 1;
            //text.material = mainFontMat;        
            text.name = passText;
            text.font = mainFont;
            text.alignment = align;
            text.text = passText;
            text.fontSize = fontSize;
            text.color = color1;

            textBox.transform.localScale = new Vector3(.1f, .1f, .1f);
            textBox.transform.Rotate(Vector3.up * -90f);
            return text;
        }

        public static GameObject YearLine(string year, float earthLineL)
        {
            GameObject yearLine = new GameObject("yearLine");
            AddCanvas2(yearLine);

            GameObject axisLine = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            axisLine.name = "axisLine";
            axisLine.GetComponent<CapsuleCollider>().enabled = false;
            axisLine.transform.localScale = new Vector3(2f, earthLineL * .5f, 2f);
            axisLine.transform.SetParent(yearLine.transform, false);

            Text yearText = NewText2(year, Color.white, 200, TextAnchor.MiddleRight, false);
            yearText.transform.SetParent(yearLine.transform);
            yearText.transform.localPosition = new Vector3(-10f, -earthLineL * .5f + 10f, 0f);
            yearText.transform.Rotate(Vector3.forward * -90f);
            
            float tickR = 1f;

            GameObject tick = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            tick.name = "BigTick";
            tick.GetComponent<CapsuleCollider>().enabled = false;
            tick.transform.localScale = new Vector3(tickR, 20f, tickR);
            tick.transform.SetParent(yearLine.transform, false);
            tick.transform.localPosition = new Vector3(0f, -earthLineL * .5f, 0f);
            tick.transform.Rotate(Vector3.forward, 90f);

            for (int ii = 1; ii < 11; ii++)
            {
                tick = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                tick.name = "MedTick";
                tick.GetComponent<CapsuleCollider>().enabled = false;
                tick.transform.localScale = new Vector3(tickR, 5f, tickR);
                tick.transform.SetParent(yearLine.transform, false);
                tick.transform.localPosition = new Vector3(0f, -earthLineL * .5f + (ii * earthLineL) / 11f, 0f);
                tick.transform.Rotate(Vector3.forward, 90f);
            }

            return yearLine;
        }
    }
}