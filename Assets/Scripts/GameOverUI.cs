using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class GameOverUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private Button retryButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private TextMeshProUGUI gameOverText;

    private bool isGameOver = false;

    private void OnEnable()
    {
        PlayerStatusManager.OnPlayerDeath += ShowGameOver;
    }

    private void OnDisable()
    {
        PlayerStatusManager.OnPlayerDeath -= ShowGameOver;
    }

    private void Start()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        retryButton.onClick.AddListener(OnRetryClicked);
        quitButton.onClick.AddListener(OnQuitClicked);
    }

    private void ShowGameOver()
    {
        if (isGameOver) return;

        StartCoroutine(DelayedShowGameOver());
    }

    private void OnRetryClicked()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void OnQuitClicked()
    {
        Time.timeScale = 1f;
        Application.Quit();
    }

    private IEnumerator DelayedShowGameOver()
    {
        yield return new WaitForSeconds(1f);

        isGameOver = true;
        Time.timeScale = 0f; //freeze game
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        gameOverPanel.SetActive(true);
        if (gameOverText != null)
            gameOverText.text = "GAME OVER";
    }
}