using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Endless Wave Spawner - Spawns enemies in groups with scaling difficulty
/// Supports manual wave calls with early call bonuses
/// </summary>
public class EndlessWaveSpawner : MonoBehaviour
{
    [Header("Enemy Configuration")]
    [SerializeField] private List<GameObject> enemyPrefabs = new List<GameObject>();
    [Tooltip("Number of enemy groups per wave")]
    [SerializeField] private int groupsPerWave = 4;
    [Tooltip("Enemies per group (min-max)")]
    [SerializeField] private Vector2Int enemiesPerGroup = new Vector2Int(8, 12);

    [Header("Waypoints")]
    [SerializeField] private Transform[] waypoints;

    [Header("Spawn Timing")]
    [SerializeField] private float spawnInterval = 0.5f;
    [SerializeField] private float groupDelay = 2f;
    [SerializeField] private float waveDelay = 1f;

    [Header("Difficulty Scaling")]
    [Tooltip("Health multiplier increase every X waves")]
    [SerializeField] private int wavesPerHealthIncrease = 5;
    [SerializeField] private float healthMultiplierPerTier = 1.2f;
    [Tooltip("Extra enemies added every X waves")]
    [SerializeField] private int wavesPerEnemyIncrease = 3;
    [SerializeField] private int extraEnemiesPerIncrease = 2;

    [Header("Early Wave Bonus")]
    [SerializeField] private bool enableEarlyWaveBonus = true;
    [SerializeField] private int minEarlyWaveBonus = 10;
    [SerializeField] private int maxEarlyWaveBonus = 30;
    [SerializeField] private float autoWaveDelay = 10f;

    [Header("UI References")]
    [SerializeField] private UnityEngine.UI.Button callWaveButton;

    private int currentWave = 0;
    private bool isSpawning = false;
    private bool canCallWave = true;
    private float autoWaveTimer = 0f;
    private bool isAutoWaveActive = false;

    void Start()
    {
        if (enemyPrefabs.Count == 0)
        {
            Debug.LogError("EndlessWaveSpawner: No enemy prefabs assigned!");
        }

        if (waypoints == null || waypoints.Length == 0)
        {
            Debug.LogError("EndlessWaveSpawner: No waypoints assigned!");
        }

        // Setup call wave button
        if (callWaveButton != null)
        {
            callWaveButton.onClick.AddListener(() => CallNextWave(true));
        }
        else
        {
            Debug.LogWarning("EndlessWaveSpawner: Call Wave Button not assigned! Assign it in the inspector to manually call waves.");
        }
    }

    void OnDestroy()
    {
        // Clean up button listener
        if (callWaveButton != null)
        {
            callWaveButton.onClick.RemoveListener(() => CallNextWave(true));
        }
    }

    void Update()
    {
        // Auto wave timer
        if (isAutoWaveActive && !isSpawning)
        {
            autoWaveTimer -= Time.deltaTime;
            
            if (autoWaveTimer <= 0f)
            {
                CallNextWave(false); // No bonus for auto wave
            }
        }
    }

    public void CallNextWave(bool manualCall = true)
    {
        if (!canCallWave || isSpawning) return;

        // Early wave bonus
        if (manualCall && isAutoWaveActive && enableEarlyWaveBonus)
        {
            float timeRemaining = autoWaveTimer;
            if (timeRemaining > 0f)
            {
                int bonus = Mathf.RoundToInt(Mathf.Lerp(minEarlyWaveBonus, maxEarlyWaveBonus, timeRemaining / autoWaveDelay));
                
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.AddGold(bonus);
                    
                    // Show bonus text
                    FloatingText.Create(Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0.8f, 10f)),
                        $"+{bonus}G Early Bonus!",
                        Color.cyan);
                }
            }
        }

        currentWave++;
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetCurrentWave(currentWave);
        }

        StartCoroutine(SpawnWave());
    }

    private IEnumerator SpawnWave()
    {
        isSpawning = true;
        canCallWave = false;
        isAutoWaveActive = false;

        yield return new WaitForSeconds(waveDelay);

        // Calculate difficulty scaling
        int difficultyTier = (currentWave - 1) / wavesPerHealthIncrease;
        float healthMultiplier = Mathf.Pow(healthMultiplierPerTier, difficultyTier);

        int extraEnemies = ((currentWave - 1) / wavesPerEnemyIncrease) * extraEnemiesPerIncrease;

        // Spawn groups
        for (int group = 0; group < groupsPerWave; group++)
        {
            int enemiesInGroup = Random.Range(enemiesPerGroup.x, enemiesPerGroup.y + 1) + extraEnemies;
            
            for (int i = 0; i < enemiesInGroup; i++)
            {
                SpawnEnemy(healthMultiplier);
                yield return new WaitForSeconds(spawnInterval);
            }

            // Delay between groups (except last group)
            if (group < groupsPerWave - 1)
            {
                yield return new WaitForSeconds(groupDelay);
            }
        }

        isSpawning = false;
        canCallWave = true;
        
        // Start auto wave timer
        isAutoWaveActive = true;
        autoWaveTimer = autoWaveDelay;
    }

    private void SpawnEnemy(float healthMultiplier)
    {
        if (enemyPrefabs.Count == 0 || waypoints.Length == 0) return;

        // Pick random enemy type
        GameObject enemyPrefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Count)];
        
        // Spawn at first waypoint
        GameObject enemyObj = Instantiate(enemyPrefab, waypoints[0].position, Quaternion.identity);
        
        // Initialize enemy
        Enemy enemy = enemyObj.GetComponent<Enemy>();
        if (enemy != null)
        {
            // Scale health based on difficulty
            float baseHealth = 100f; // Default, will be overridden by prefab values
            enemy.SetMaxHealth(baseHealth * healthMultiplier);
        }

        // Initialize waypoint follower
        WaypointFollower follower = enemyObj.GetComponent<WaypointFollower>();
        if (follower == null)
        {
            follower = enemyObj.AddComponent<WaypointFollower>();
        }
        
        follower.Initialize(waypoints);
    }

    public bool IsSpawning() => isSpawning;
    public int GetCurrentWave() => currentWave;
    public bool CanCallWave() => canCallWave && !isSpawning;
    public float GetAutoWaveTimeRemaining() => autoWaveTimer;
}
