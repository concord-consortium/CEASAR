using System.Collections.Generic;
using UnityEngine;
public static class DataImport
{
    public static List<Star> ImportStarData(string sourceData)
    {
        List<Star> stars = new List<Star>();
        foreach (var line in splitToLines(sourceData))
        {
            if (!line.StartsWith("Hip"))
            {
                string[] values = line.Split('\t');

                if (values.Length == 11)
                {
                    int Hip = int.Parse(values[0]);
                    string Constellation = values[1];
                    string ProperName = values[2];
                    string XByerFlamsteed = values[3];
                    float RA = float.Parse(values[4]);
                    float Dec = float.Parse(values[5]);
                    float Dist = float.Parse(values[6]);
                    float Mag = float.Parse(values[7]);
                    float AbsMag = float.Parse(values[8]);
                    string Spectrum = values[9];
                    float ColorIndex = 0.0f;
                    float.TryParse(values[10], out ColorIndex);
                    Star star = new Star(Hip, Constellation, ProperName, XByerFlamsteed, RA, Dec, Dist, Mag, AbsMag, Spectrum, ColorIndex);

                    stars.Add(star);
                }
            }
        }
        return stars;
    }
    public static List<City> ImportCityData(string sourceData)
    {
        List<City> cities = new List<City>();
        foreach (var line in splitToLines(sourceData))
        {
            if (!line.StartsWith("city_ascii"))
            {
                string[] values = line.Split('\t');
                if (values.Length == 5)
                {
                    City city = new City();
                    city.Name = values[0];
                    city.Lat = float.Parse(values[1]);
                    city.Lng = float.Parse(values[2]);
                    city.Country = values[3];
                    city.Timezone = float.Parse(values[4]);
                    cities.Add(city);
                }
            }
        }
        return cities;
    }
    private static IEnumerable<string> splitToLines(this string input)
    {
        if (input == null)
        {
            yield break;
        }

        using (System.IO.StringReader reader = new System.IO.StringReader(input))
        {
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                yield return line;
            }
        }
    }
}
