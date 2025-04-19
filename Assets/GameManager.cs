//Author Grok
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    // Singleton instance
    public static GameManager Instance { get; private set; }

    // Game state properties
    public int Lives { get; private set; } = 3;  // Default starting lives
    public int Coins { get; private set; } = 0;  // Total coins collected
    public int Score { get; private set; } = 0;  // Player score
    public int CurrentLevel { get; private set; } = 1;  // Current level index
    private int deathCount = 0;  // Tracks deaths for ad timing

    // Awake ensures Singleton behavior
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);  // Persists across scenes
        }
        else
        {
            Destroy(gameObject);  // Destroy duplicates
        }
        LoadGame();  // Load saved progress on start
    }

    // Coin collection logic
    public void AddCoin()
    {
        Coins++;
        Score += 10;  // Each coin adds 10 points
        if (Coins % 100 == 0)  // Extra life every 100 coins
        {
            Lives++;
        }
        SaveGame();  // Save progress
    }

    // Life loss logic
    public void LoseLife()
    {
        Lives--;
        deathCount++;
        SaveGame();

        if (deathCount >= 3)  // Show ad every 3 deaths
        {
            ShowAd();
            deathCount = 0;
        }

        if (Lives > 0)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);  // Restart level
        }
        else
        {
            SceneManager.LoadScene("GameOver");  // Load Game Over scene
        }
    }

    // Level progression
    public void LoadNextLevel()
    {
        CurrentLevel++;
        ShowAd();  // Show ad between levels
        SceneManager.LoadScene("Level" + CurrentLevel);  // Load next level
        SaveGame();
    }

    // Ad display (placeholder for ad SDK integration)
    public void ShowAd()
    {
        // Integrate Unity Ads or AdMob here, e.g., Advertisement.Show();
        Debug.Log("Showing ad...");
    }

    // Save game state
    public void SaveGame()
    {
        PlayerPrefs.SetInt("Lives", Lives);
        PlayerPrefs.SetInt("Coins", Coins);
        PlayerPrefs.SetInt("Score", Score);
        PlayerPrefs.SetInt("CurrentLevel", CurrentLevel);
        PlayerPrefs.Save();
    }

    // Load game state
    public void LoadGame()
    {
        Lives = PlayerPrefs.GetInt("Lives", 3);  // Default to 3 if no save
        Coins = PlayerPrefs.GetInt("Coins", 0);
        Score = PlayerPrefs.GetInt("Score", 0);
        CurrentLevel = PlayerPrefs.GetInt("CurrentLevel", 1);
    }

    // Reset for new game
    public void ResetGame()
    {
        Lives = 3;
        Coins = 0;
        Score = 0;
        CurrentLevel = 1;
        PlayerPrefs.DeleteAll();
        SceneManager.LoadScene("Level1");
    }

    void Update()
    {
        Debug.Log(Coins);
    }
}