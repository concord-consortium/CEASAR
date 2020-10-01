using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public static class DataImport
{
    public static void ImportAllData(string starData, int maxStars, string cityData, string constellationData)
    {
        DataManager dataManager = DataManager.Instance;
        dataManager.Stars = importStarData(starData);
        dataManager.Cities = importCityData(cityData);
        dataManager.Connections = importConstellationConnectionData(constellationData);
        dataManager.MaxStarCount = maxStars;
    }
    
    private static List<Star> importStarData(string sourceData)
    {
        int numStars = 0;
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
                    numStars++;
                }
            }
        }
        return stars;
    }
    private static List<City> importCityData(string sourceData)
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

    private static List<ConstellationConnection> importConstellationConnectionData(string sourceData)
    {
        List<ConstellationConnection> connections = new List<ConstellationConnection>();
        foreach (var line in splitToLines(sourceData))
        {
            if (!line.StartsWith("ConstellationID"))
            {
                string[] values = line.Split('\t');
                if (values.Length >= 6)
                {
                    for (int i = 0; i < values.Length; i++)
                    {
                        if (i > 4 && (i - 1) % 2 == 0 && (values[0])[0] != '.')
                        {
                            ConstellationConnection connection = new ConstellationConnection();
                            connection.constellationNameAbbr = values[1];
                            connection.constellationNameFull = values[2];
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
