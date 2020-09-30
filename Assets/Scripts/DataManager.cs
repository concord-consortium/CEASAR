using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public struct ConstellationNamePair
{
    public string shortName;
    public string fullName;
}
// This class is the manager Singleton, and contains specific references to application-level objects
public class DataManager
{
    protected DataManager() {
       
    }
    private static DataManager instance;

    public static DataManager GetInstance()
    {
        return instance ?? (instance = new DataManager());
    }
    private List<Star> stars;
    private int maxStarCount = 0;

    public List<City> Cities;

    private List<string> cityNames;
    public List<string> CityNames
    {
        get 
        {
            if (cityNames == null)
            {
                cityNames = Cities.Select(c => c.Name).ToList();
            }
            return cityNames;
        }
    }
    public List<ConstellationConnection> Connections;

    public int MaxStarCount
    {
        get => maxStarCount;
        set
        {
            maxStarCount = value;
            filteredStars = stars.OrderBy(s => s.Mag).Take(maxStarCount).ToList();
        }
    }

    private List<Star> filteredStars;

    public List<Star> Stars
    {
        get
        {
            if (filteredStars == null)
            {
                updateData();
            }
            return filteredStars;
        }
        set
        {
            stars = value;
            updateData();
        }
    }

    private void updateData()
    {
        if (maxStarCount > 0 && maxStarCount < stars.Count)
        {
            filteredStars = stars.OrderBy(s => s.Mag).Take(maxStarCount).ToList();
        }
        else
        {
            filteredStars = stars;
        }

        MinMag = filteredStars.Min(s => s.Mag);
        MaxMag = filteredStars.Max(s => s.Mag);
        ConstellationFullNames = new List<string>(filteredStars.GroupBy(s => s.ConstellationFullName).Select(s => s.First().ConstellationFullName));
        ConstellationNames = new List<ConstellationNamePair>(filteredStars
            .GroupBy(s => s.ConstellationFullName)
            .Select(s => new ConstellationNamePair { shortName = s.First().Constellation, fullName = s.First().ConstellationFullName }));
    }
    
    // Calculated from data
    public float MinMag { get; private set; }
    public float MaxMag { get; private set; }
    
    public List<string> ConstellationFullNames{ get; private set; }
    public List<ConstellationNamePair> ConstellationNames { get; private set; }

    public List<Star> AllStarsInConstellation(string constellationName)
    {
        return Stars.Where(s => s.Constellation == constellationName).ToList();
    }
    public List<Star> AllStarsInConstellationByFullName(string constellationFullName)
    {
        return Stars.Where(s => s.ConstellationFullName == constellationFullName).ToList();
    }
    
    public float GetRelativeMagnitude(float starMagnitude)
    {
        float max = MaxMag + Mathf.Abs(MinMag);
        return max - (starMagnitude + Mathf.Abs(MinMag));
    }

}
