using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/*
B0	-0.30	26500 K	#a7bcff
B5	-0.16	13800 K	#bbccff
A0	0.00	9850 K	#d1dbff
A5	+0.14	8260 K	#e2e7ff
F0	+0.31	7030 K	#f7f5ff
F5	+0.43	6400 K	#fff8f8
G0	+0.59	5900 K	#fff3ea
G5	+0.66	5660 K	#fff0e3
K0	+0.82	5240 K	#ffead5
K5	+1.15	4400 K	#ffddb4
M0	+1.41	3750 K	#ffcf95
M5	+1.61	3100 K	#ffbd6f
*/

public static class StarColor
{
    private static Dictionary<double, string> colorLookupTable;
    private static Dictionary<double, Color> colorLookups;

    // This is a simple implementation - using MainSequence values from
    // http://www.vendian.org/mncharity/dir3/starcolor/details.html, we don't have the luminosity
    // class for each star in our current dataset. This implementation aims for an approximation
    public static Color GetColorFromColorIndexSimple(double colorIndex)
    {
        Color color = Color.white;
        if (colorLookupTable == null)
        {
            colorLookupTable = new Dictionary<double, string>();
            colorLookupTable.Add(-0.3, "#a7bcff");
            colorLookupTable.Add(-0.16, "#bbccff");
            colorLookupTable.Add(0, "#d1dbff");
            colorLookupTable.Add(0.14, "#e2e7ff");
            colorLookupTable.Add(0.31, "#f7f5ff");
            colorLookupTable.Add(0.43, "#fff8f8");
            colorLookupTable.Add(0.59, "#fff3ea");
            colorLookupTable.Add(0.66, "#fff0e3");
            colorLookupTable.Add(0.82, "#ffead5");
            colorLookupTable.Add(1.15, "#ffddb4");
            colorLookupTable.Add(1.41, "#ffcf95");
            colorLookupTable.Add(1.61, "#ffbd6f");
            colorLookupTable.Add(1.65, "#ffcc8f");
            colorLookupTable.Add(1.7, "#ffc885");
            colorLookupTable.Add(1.75, "#ffc178");
            colorLookupTable.Add(1.8, "#ffb765");
            colorLookupTable.Add(1.85, "#ffa94b");
            colorLookupTable.Add(1.9, "#ff9523");
            // colorLookupTable.Add(1.95, "#ff7b00");
            // colorLookupTable.Add(2.0, "#ff5200");
        }

        double closestColorIdx = 0;
        closestColorIdx = colorLookupTable.Keys.OrderBy(v => Math.Abs((long)v - colorIndex)).First();
        
        ColorUtility.TryParseHtmlString(colorLookupTable[closestColorIdx], out color);
        return color;
    }
}
