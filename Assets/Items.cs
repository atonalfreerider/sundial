using Assets.UI.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets
{
    public static class Items
    {
        public static GameObject YearLine(string year, float earthLineL)
        {
            GameObject yearLine = new GameObject("yearLine");

            GameObject axisLine = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            axisLine.name = "axisLine";
            axisLine.GetComponent<CapsuleCollider>().enabled = false;
            axisLine.transform.localScale = new Vector3(2f, earthLineL * .5f, 2f);
            axisLine.transform.SetParent(yearLine.transform, false);

            TextBox yearText = TextBox.Create(year, TextBox.FontType.MainFont, 200, TextAlignmentOptions.Right);
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