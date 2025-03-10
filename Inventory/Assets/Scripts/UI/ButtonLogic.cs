using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Manages the functionality of UI buttons in the game.
/// </summary>
public class ButtonLogic : MonoBehaviour
{
    /// <summary>
    /// Loads the appropriate level based on player's progress.
    /// If the player has completed level 1, load level 2; otherwise, load level 1.
    /// </summary>
    public void PlayButton()
    {
        if (PlayerPrefs.HasKey("Pass1Lvl"))
        {
            SceneManager.LoadScene("lvl 2"); // Load level 2 if level 1 is completed
        }
        else
        {
            SceneManager.LoadScene("lvl 1"); // Load level 1 otherwise
        }
    }

    /// <summary>
    /// Quits the application.
    /// </summary>
    public void QuitButton()
    {
        Time.timeScale = 1;
        Application.Quit(); // Quit the application
    }

    /// <summary>
    /// Loads level 2 and ensures the game is running at normal speed.
    /// </summary>
    public void NextLevel()
    {
        Time.timeScale = 1; // Set time scale to normal
        SceneManager.LoadScene("lvl 2"); // Load level 2
    }

    /// <summary>
    /// Returns to the main menu and ensures the game is running at normal speed.
    /// </summary>
    public void BackToMenuButton()
    {
        Time.timeScale = 1; // Set time scale to normal
        SceneManager.LoadScene("Start Scene"); // Load the start scene
    }
}