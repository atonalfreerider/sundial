using Assets.GraphicsUtil.Shapes.Lines;
using TMPro;
using UnityEngine;

namespace Assets
{
    public static class GalacticLine
    {
        public static GameObject YearLine(string year, float earthLineL)
        {
            GameObject yearLine = new("yearLine");

            StaticLink axisLine = Object.Instantiate(StaticLink.prototypeStaticLink);
            axisLine.DrawFromTo(new Vector3(0, -earthLineL * .5f, 0), new Vector3(0, earthLineL * .5f, 0));
            axisLine.gameObject.SetActive(true);

            axisLine.name = "axisLine";
            axisLine.transform.SetParent(yearLine.transform, false);

            TextBox yearText = TextBox.Create(year,  TextAlignmentOptions.Right);
            yearText.Size = 200;
            yearText.transform.SetParent(yearLine.transform);
            yearText.transform.localPosition = new Vector3(-10f, -earthLineL * .5f + 10f, 0f);
            yearText.transform.Rotate(Vector3.forward * -90f);
            
            float tickR = 1f;

            StaticLink tick = Object.Instantiate(StaticLink.prototypeStaticLink);
            tick.DrawFromTo(new Vector3(0, -10, 0), new Vector3(0, 10, 0));
            tick.gameObject.SetActive(true);
            
            tick.name = "BigTick";
            
            tick.transform.SetParent(yearLine.transform, false);
            tick.transform.localPosition = new Vector3(0f, -earthLineL * .5f, 0f);
            tick.transform.Rotate(Vector3.forward, 90f);

            for (int ii = 1; ii < 11; ii++)
            {
                tick = Object.Instantiate(StaticLink.prototypeStaticLink);
                tick.DrawFromTo(new Vector3(0, -2.5f, 0), new Vector3(0, 2.5f, 0));
                tick.gameObject.SetActive(true);
                tick.name = "MedTick";
                tick.transform.SetParent(yearLine.transform, false);
                tick.transform.localPosition = new Vector3(0f, -earthLineL * .5f + (ii * earthLineL) / 11f, 0f);
                tick.transform.Rotate(Vector3.forward, 90f);
            }

            return yearLine;
        }
    }
}