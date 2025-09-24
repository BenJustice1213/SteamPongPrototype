using UnityEngine;
using TMPro;
using System.Collections;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab;

    public Vector2 spawnAreaMin;
    public Vector2 spawnAreaMax;

    private int enemiesToSpawn = 5;
    private int enemiesAlive = 0;

    private int roundNumber = 1;

    public TextMeshProUGUI roundText;

    private bool roundInProgress = false;

    void Start()
    {
        StartCoroutine(StartRound());
    }

    void Update()
    {
        if (enemiesAlive <= 0 && !roundInProgress)
        {
            enemiesToSpawn++;
            roundNumber++;
            StartCoroutine(StartRound());
        }
    }


    IEnumerator StartRound()
    {
        roundInProgress = true;

        if (roundText != null)
        {
            roundText.text = $"Round {roundNumber}";
            roundText.gameObject.SetActive(true);
        }

        yield return new WaitForSeconds(2f);

        if (roundText != null)
            roundText.gameObject.SetActive(false);

        enemiesAlive = 0;

        SpawnEnemies(enemiesToSpawn);

        roundInProgress = false;
    }


    void SpawnEnemies(int count)
    {
        for (int i = 0; i < count; i++)
        {
            Vector2 spawnPos = new Vector2(
                Random.Range(spawnAreaMin.x, spawnAreaMax.x),
                Random.Range(spawnAreaMin.y, spawnAreaMax.y)
            );

            GameObject enemyObj = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
            enemiesAlive++;

            Enemy enemyScript = enemyObj.GetComponent<Enemy>();
            if (enemyScript != null)
            {
                enemyScript.OnDestroyEvent += EnemyDestroyed;
            }
        }
        Debug.Log($"Spawned {count} enemies.");
    }

    private void EnemyDestroyed()
    {
        enemiesAlive--;
        Debug.Log($"Enemy destroyed, {enemiesAlive} remaining.");
    }
}
