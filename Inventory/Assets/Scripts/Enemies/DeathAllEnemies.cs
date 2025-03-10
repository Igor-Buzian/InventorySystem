using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages the state when all enemies are defeated.
/// Displays a victory canvas when all enemies are dead.
/// </summary>
public class DeathAllEnemies : MonoBehaviour
{
    [SerializeField] GameObject VictoryCanvas; // The canvas to display upon victory
    int EnemyCount; // The current count of remaining enemies

    /// <summary>
    /// Called when the object is disabled.
    /// Decreases the enemy count and checks for victory.
    /// </summary>
    private void OnDisable()
    {
        EnemyCount = PlayerPrefs.GetInt("EnemyCount", 0); // Retrieve the current enemy count from PlayerPrefs
        EnemyCount--; // Decrease the enemy count

        if (EnemyCount <= 0) // Check if all enemies are defeated
        {
            VictoryCanvas.SetActive(true); // Activate the victory canvas
            PlayerPrefs.SetInt("EnemyCount", 0); // Reset enemy count in PlayerPrefs
            Time.timeScale = 0; // Pause the game
        }
        else
        {
            PlayerPrefs.SetInt("EnemyCount", EnemyCount); // Update the enemy count in PlayerPrefs
        }
    }
}