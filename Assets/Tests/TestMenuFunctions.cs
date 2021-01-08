using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

public class TestMenuFunctions
{
    // A Test behaves as an ordinary method
    [Test]
    public void TestMenuFunctionsSimplePasses()
    {
        // Use the Assert class to test conditions
        Assert.That(!string.IsNullOrEmpty(SimulationManager.Instance.LocalUsername));
        Color testColor = UserRecord.GetColorForUsername("RedFox5");
        Assert.That(testColor.r > 0.5f);
    }

    // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
    // `yield return null;` to skip a frame.
    [UnityTest]
    public IEnumerator TestDateControls()
    {
        // Use the Assert class to test conditions.
        // Use yield to skip a frame.
        yield return null;
        
        GameObject menu = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/UI/NewMainUI.prefab");

        Assert.NotNull(menu);
        
        // InfoPanelController infoPanel = menu.GetComponentInChildren<InfoPanelController>();
        // string displayedText = infoPanel.pushPinDetailsText.text;
        // Assert.IsTrue(displayedText.Contains(DateTime.UtcNow.ToShortDateString()));
        int currentYear = SimulationManager.Instance.CurrentSimulationTime.Year;
        int currentMonth = SimulationManager.Instance.CurrentSimulationTime.Month;
        int currentDay = SimulationManager.Instance.CurrentSimulationTime.DayOfYear;
        
        MenuController menuPanel = menu.GetComponentInChildren<MenuController>();
        menuPanel.ChangeYear(1);
        Assert.That(SimulationManager.Instance.CurrentSimulationTime.Year > currentYear);
        menuPanel.ChangeYear(-2);
        Assert.That(SimulationManager.Instance.CurrentSimulationTime.Year < currentYear);

        if (currentMonth != 12)
        {
            menuPanel.ChangeMonth(1);
            Assert.That(SimulationManager.Instance.CurrentSimulationTime.Month > currentMonth);
        }
        menuPanel.ChangeMonth(-1);
        Assert.That(SimulationManager.Instance.CurrentSimulationTime.Month <= currentMonth);

        if (currentDay != 365)
        {
            menuPanel.ChangeDay(1);
            Assert.That(SimulationManager.Instance.CurrentSimulationTime.DayOfYear > currentDay);
        }
        menuPanel.ChangeDay(-1);
        Assert.That(SimulationManager.Instance.CurrentSimulationTime.DayOfYear <= currentDay);

    }
}

