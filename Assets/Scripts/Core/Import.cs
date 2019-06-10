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

                if (values.Length == 16)
                {
                    int Hip = int.Parse(values[0]);
                    string ConstellationAbbr = values[1];
                    string ConstellationFull = values[2];
                    string ProperName = values[3];
                    string XBayerFlamsteed = values[4];
                    string FlamsteedDes = values[5];
                    string BayerDes = values[6];
                    float RA = float.Parse(values[7]);
                    float RadianRA = float.Parse(values[8]);
                    float Dec = float.Parse(values[9]);
                    float RadianDec = float.Parse(values[10]);
                    float Dist = float.Parse(values[11]);
                    float Mag = float.Parse(values[12]);
                    float AbsMag = float.Parse(values[13]);
                    string Spectrum = values[14];
                    float ColorIndex = 0.0f;
                    float.TryParse(values[15], out ColorIndex);
                    Star star = new Star(Hip, ConstellationAbbr, ConstellationFull, ProperName, XBayerFlamsteed, FlamsteedDes, BayerDes, RA, RadianRA, Dec, RadianDec, Dist, Mag, AbsMag, Spectrum, ColorIndex);

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

    public static List<ConstellationConnection> ImportConstellationConnectionData(string sourceData)
    {
        List<ConstellationConnection> connections = new List<ConstellationConnection>();
        foreach (var line in splitToLines(sourceData))
        {
            if (!line.StartsWith("Constellation"))
            {
                string[] values = line.Split('\t');
                if (values.Length > 4)
                {
                    for (int i = 0; i < values.Length; i++)
                    {
                        if (i > 2 && i % 2 == 0)
                        {
                            ConstellationConnection connection = new ConstellationConnection();
                            connection.constellationNameAbbr = values[0];
                            connection.constellationNameFull = values[1];
                            connection.startStarHipId = int.Parse(values[i - 1]);
                            connection.endStarHipId = int.Parse(values[i]);
                            connections.Add(connection);
                        }
                    }
                }
            }
        }
        return connections;
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
