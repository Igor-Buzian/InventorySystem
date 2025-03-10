using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Live : MonoBehaviour
{
    /// <summary>
    /// It creates a delay before restarting the scene
    /// </summary>
    public void PlayerDeath()
    {
        StartCoroutine(LoadCurrentScene());
    }
    /// <summary>
    /// It creates a delay before Clearing enemy body
    /// </summary>
    public void EnemyDeath()
    {
        StartCoroutine(ClearBody());
    }
    IEnumerator LoadCurrentScene()
    {
        yield return new WaitForSeconds(5f);
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    IEnumerator ClearBody()
    {
        yield return new WaitForSeconds(5f);
        gameObject.SetActive(false);
    }

}
