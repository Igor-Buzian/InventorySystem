using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public Button playButton; // Play button
    public Button exitButton; // Exit button
    public GameObject buttonPanel; // Panel containing the buttons

    private void Start()
    {
        // Set initial position of buttons off-screen
        playButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(-550, 50); 
        exitButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(-550, -50); 

        StartCoroutine(ShowButtons());
    }

    private IEnumerator ShowButtons()
    {
        float duration = 0.3f; // Faster animation duration for Play button
        float elapsedTime = 0f;
        Vector2 targetPositionPlay = new Vector2(0, 50); // Target position for Play button
        Vector2 targetPositionExit = new Vector2(0, -50); // Target position for Exit button

        // Move play button into view
        RectTransform playRt = playButton.GetComponent<RectTransform>();
        Vector2 startPositionPlay = playRt.anchoredPosition;

        while (elapsedTime < duration)
        {
            playRt.anchoredPosition = Vector2.Lerp(startPositionPlay, targetPositionPlay, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        playRt.anchoredPosition = targetPositionPlay;

        // Delay for exit button
        yield return new WaitForSeconds(0.2f); // Short delay before moving Exit button

        // Move exit button into view
        RectTransform exitRt = exitButton.GetComponent<RectTransform>();
        Vector2 startPositionExit = exitRt.anchoredPosition;
        elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            exitRt.anchoredPosition = Vector2.Lerp(startPositionExit, targetPositionExit, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        exitRt.anchoredPosition = targetPositionExit;
    }

    public void OnPlayButtonClicked()
    {
        StartCoroutine(HideButtonsAndLoadScene());
    }

    public void OnExitButtonClicked()
    {
        Application.Quit();
    }

    private IEnumerator HideButtonsAndLoadScene()
    {
        float duration = 0.3f; // Shorter duration for hiding buttons
        float elapsedTime = 0f;
        Vector2 targetPositionPlay = new Vector2(-550, 50); // Move off-screen for Play button
        Vector2 targetPositionExit = new Vector2(-550, -50); // Move off-screen for Exit button

        // Hide play button
        RectTransform playRt = playButton.GetComponent<RectTransform>();
        Vector2 startPositionPlay = playRt.anchoredPosition;

        while (elapsedTime < duration)
        {
            playRt.anchoredPosition = Vector2.Lerp(startPositionPlay, targetPositionPlay, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        playRt.anchoredPosition = targetPositionPlay;

        // Hide exit button
        RectTransform exitRt = exitButton.GetComponent<RectTransform>();
        Vector2 startPositionExit = exitRt.anchoredPosition;
        elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            exitRt.anchoredPosition = Vector2.Lerp(startPositionExit, targetPositionExit, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        exitRt.anchoredPosition = targetPositionExit;
        SceneManager.LoadScene("Gameplay"); 
    }
}