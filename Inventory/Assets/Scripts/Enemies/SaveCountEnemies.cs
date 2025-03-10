using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Saves the count of child enemies to PlayerPrefs.
/// </summary>
public class SaveCountEnemies : MonoBehaviour
{

    void Start()
    {
        // Get the count of child objects
        int count = transform.childCount;

        // Save the count to PlayerPrefs
        PlayerPrefs.SetInt("EnemyCount", count);
        PlayerPrefs.Save(); // Save changes to PlayerPrefs
    }
}