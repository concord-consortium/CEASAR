using System.Collections.Generic;
using UnityEngine;
public static class DataImport{
public static List<Star> ImportStarData(string sourceData){
    List<Star> stars = new List<Star>();
    foreach(var line in splitToLines(sourceData))
    {
       if (!line.StartsWith("Constellation")){
        string[] values = line.Split('\t');
        if (values.Length == 6){
          Star star = new Star();
          star.Constellation = values[0];
          star.XByerFlamsteed = values[1];
          star.RA = float.Parse(values[2]);
          star.Dec = float.Parse(values[3]);
          star.Dist = float.Parse(values[4]);
          star.Mag = float.Parse(values[5]);
          stars.Add(star);
        }
      }
    }
    return stars;
  }
  public static List<City> ImportCityData(string sourceData){
    List<City> cities = new List<City>();
    foreach(var line in splitToLines(sourceData))
    {
      if(!line.StartsWith("city_ascii")){
        string[] values = line.Split('\t');
        if (values.Length == 5){
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
            while( (line = reader.ReadLine()) != null)
            {
                yield return line;
            }
        }
    }
}
