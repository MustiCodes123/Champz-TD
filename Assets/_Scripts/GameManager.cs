using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

/// <summary>
/// Core Game Manager - Handles gold, lives, game state
/// Singleton pattern for easy access from any script
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Game Settings")]
    [SerializeField] private int startingGold = 1000;
    [SerializeField] private int startingLives = 20;

    [Header("Enemy Rewards")]
    [SerializeField] private int minGoldPerKill = 35;
    [SerializeField] private int maxGoldPerKill = 50;

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI goldText;
    [SerializeField] private TextMeshProUGUI livesText;
    [SerializeField] private TextMeshProUGUI waveText;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject victoryPanel;

    // Current game state
    private int currentGold;
    private int currentLives;
    private int currentWave = 0;
    private bool isGameOver = false;

    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        // Initialize game state
        currentGold = startingGold;
        currentLives = startingLives;
        
        UpdateUI();
        
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
        
        if (victoryPanel != null)
            victoryPanel.SetActive(false);
    }

    #region Gold Management
    
    public int GetCurrentGold() => currentGold;

    public bool CanAfford(int cost)
    {
        return currentGold >= cost;
    }

    public void SpendGold(int amount)
    {
        currentGold -= amount;
        UpdateUI();
    }

    public void AddGold(int amount)
    {
        currentGold += amount;
        UpdateUI();
    }

    public int GetRandomEnemyGold()
    {
        return Random.Range(minGoldPerKill, maxGoldPerKill + 1);
    }

    #endregion

    #region Lives Management

    public int GetCurrentLives() => currentLives;

    public void ReduceLives(int amount = 1)
    {
        if (isGameOver) return;

        currentLives -= amount;
        UpdateUI();

        if (currentLives <= 0)
        {
            currentLives = 0;
            GameOver();
        }
    }

    #endregion

    #region Wave Management

    public void SetCurrentWave(int wave)
    {
        currentWave = wave;
        UpdateUI();
    }

    public int GetCurrentWave() => currentWave;

    #endregion

    #region Game State

    private void GameOver()
    {
        isGameOver = true;
        Debug.Log("Game Over!");
        
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);
        
        Time.timeScale = 0f; // Pause game
    }

    public void Victory()
    {
        Debug.Log("Victory!");
        
        if (victoryPanel != null)
            victoryPanel.SetActive(true);
        
        Time.timeScale = 0f; // Pause game
    }

    public bool IsGameOver() => isGameOver;

    #endregion

    #region UI Management

    private void UpdateUI()
    {
        if (goldText != null)
            goldText.text = $"Gold: {currentGold}";

        if (livesText != null)
            livesText.text = $"Lives: {currentLives}";

        if (waveText != null)
            waveText.text = $"Wave: {currentWave}";
    }

    #endregion

    #region Time Management
    public void PauseGame()
    {
        AudioManager.Instance.PlaySound(SoundType.ButtonClick);
        Time.timeScale = 0f;
    }

    public void ContinueGame()
    {
        AudioManager.Instance.PlaySound(SoundType.ButtonClick);
        Time.timeScale = 1f;
    }

    #endregion

    #region Scene Management

    public void RestartGame()
    {
        AudioManager.Instance.PlaySound(SoundType.ButtonClick);
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void LoadMainMenu()
    {
        AudioManager.Instance.PlaySound(SoundType.ButtonClick);
        Time.timeScale = 1f;
        SceneManager.LoadScene(0); // Assumes main menu is scene 0
    }

    #endregion

    public void PlayButtonClickSound()
    {
        AudioManager.Instance.PlaySound(SoundType.ButtonClick);
    }
}