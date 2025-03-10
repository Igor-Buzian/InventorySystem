using System;
using UnityEngine;


public class VictoryCanvasEnabled : MonoBehaviour
{
    public event Action OnVictoryCanvasEnabled; // Event with no parameters

    /// <summary>
    /// Saves the inventory and triggers the victory canvas activation event.
    /// </summary>
    public void SaveInventory()
    {
        OnVictoryCanvasEnabled?.Invoke(); // Notify subscribers of the event
        PlayerPrefs.SetInt("Pass1Lvl", 1); // Save level completion status
        Debug.Log("VictoryCanvasEnabled activated, event triggered.");
    }
}