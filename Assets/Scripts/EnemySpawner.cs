using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Wargon.ezs.Unity;

public class EnemySpawner : MonoBehaviour {
    [SerializeField] private float delayPerWave;
    [SerializeField] private float delayPerEnemy;
    private float delayPerWaveCounter;
    private float delayPerEnemyCounter;
    private int enemySpawnCounter;
    [SerializeField] private int enemiesPerSpawn;
    [SerializeField] private List<MonoEntity> enemies;
    [SerializeField] private List<MonoEntity> rareEnemies;
    public bool Active = true;
    void Update() {
        if(!gameObject.activeInHierarchy) return;
        if(!Active) return;
        var dt = Time.deltaTime;
        if (delayPerWaveCounter > 0) {
            delayPerWaveCounter -= dt;
        }
        else {
            if (delayPerEnemyCounter > 0) {
                delayPerEnemyCounter -= dt;
            }
            else {
                SpawnEnemy(enemies);
                delayPerEnemyCounter = delayPerEnemy;
                enemySpawnCounter++;
                if (enemySpawnCounter == enemiesPerSpawn) {
                    enemySpawnCounter = 0;
                    delayPerWaveCounter = delayPerWave;
                }
            }
        }
    }

    private void SpawnEnemy(List<MonoEntity> list) {
        var index = Random.Range(0, list.Count);
        var enemyToSpawn = list[index];
        ObjectPool.ReuseEntity(enemyToSpawn, transform.position, Quaternion.identity);
    }
}
