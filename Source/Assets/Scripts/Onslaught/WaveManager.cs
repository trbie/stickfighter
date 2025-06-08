using System.Collections;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    [Header("Wave Configuration")]
    public Enemy enemy;

    public Vector3 respawnPosition;
    public float[] healthScaleFactors = { 0.70f, 1.0f, 1.30f };
    public float[] speedScaleFactors = { 0.70f, 1.0f, 1.30f };

    private int currentWave = 0;
    private float baseMaxHealth;
    private float baseSpeed;
    private bool isWaveInProgress = false;
    private Vector3 playerStartPosition;

    private void Awake()
    {
        if (respawnPosition == Vector3.zero)
        {
            respawnPosition = enemy.transform.position;
        }

        // Store base values for proper scaling
        baseMaxHealth = enemy.maxHealth;
        baseSpeed = enemy.speed;

        // Store player's starting position
        if (enemy.enemy != null)
        {
            playerStartPosition = enemy.enemy.transform.position;
        }

        if (GameMode.Current == GameMode.Mode.Onslaught)
        {
            ApplyCurrentWaveStats();
        }
    }

    private void Update()
    {
        if (GameMode.Current != GameMode.Mode.Onslaught) return;
        if (currentWave >= healthScaleFactors.Length) return;
        if (isWaveInProgress) return;

        if (enemy.state == Fighter.State.Dead)
        {
            StartCoroutine(HandleEnemyDeath());
        }
    }

    private IEnumerator HandleEnemyDeath()
    {
        isWaveInProgress = true;
        currentWave++;

        if (GameMode.Current == GameMode.Mode.Onslaught)
        {
            if (currentWave + 1 <= 3)
            {
                GameManager.Instance.waveText.text = $"{currentWave + 1}/3";
            }
            else
            {
                GameManager.Instance.waveText.gameObject.SetActive(false);
            }
        }

        if (currentWave >= healthScaleFactors.Length)
        {
            // All waves completed - player wins
            GameManager.Instance.winnerText.text = "You survived the Onslaught!";
            GameManager.Instance.winnerText.transform.parent.gameObject.SetActive(true);
            yield break;
        }

        yield return new WaitForSeconds(1f);
        ReviveEnemyForNextWave();
        ApplyCurrentWaveStats();
        isWaveInProgress = false;
    }

    private void ReviveEnemyForNextWave()
    {
        // Use the public revival method
        enemy.ReviveAtPosition(respawnPosition);

        // Reset player position to starting position
        if (enemy.enemy != null)
        {
            enemy.enemy.transform.position = playerStartPosition;
        }
    }

    private void ApplyCurrentWaveStats()
    {
        if (currentWave >= healthScaleFactors.Length) return;

        float healthMultiplier = healthScaleFactors[currentWave];
        float speedMultiplier = speedScaleFactors[currentWave];

        // Apply scaling based on BASE values to prevent compounding
        float newMaxHealth = baseMaxHealth * healthMultiplier;
        float newSpeed = baseSpeed * speedMultiplier;

        enemy.SetMaxHealth(newMaxHealth);
        enemy.health = newMaxHealth;
        enemy.speed = newSpeed;

        if (enemy.healthbar != null)
        {
            enemy.healthbar.SetHealth(enemy.health);
        }
    }
}