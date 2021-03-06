using System.Collections.Generic;
using System.Linq;
using MoreMountains.Tools;
using MoreMountains.TopDownEngine;
using Sirenix.OdinInspector;
using UnityEngine;
using static MoreMountains.TopDownEngine.CharacterStates;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] bool spawnEnemiesInRoom = true;
    [ShowIf("@spawnEnemiesInRoom")]
    [SerializeField] int howManyEnemiesToSpawn = 0;

    [Space]

    [SerializeField] Room room;
    [SerializeField] Transform spawnersParent;

    private List<Character> enemies;

    private void Start()
    {
        if (!spawnEnemiesInRoom)
        {
            Destroy(this);
        }

        enemies = new List<Character>();

        room.GetComponent<RoomEntrances>().RoomGenerated.AddListener(SpawnEnemies);
    }

    public void SpawnEnemies()
    {
        System.Random rand = MissionManager.instance.Rand;

        List<EnemySpawnPoint> remainingSpawns = new List<EnemySpawnPoint>();

        foreach (Transform spawn in spawnersParent)
        {
            if (spawn.gameObject.activeSelf)
            {
                remainingSpawns.Add(spawn.GetComponent<EnemySpawnPoint>());
            }
        }

        if (spawnEnemiesInRoom && howManyEnemiesToSpawn == 0)
        {
            howManyEnemiesToSpawn = remainingSpawns.Count;
        }


        for (var i = 0; i < howManyEnemiesToSpawn && remainingSpawns.Count > 0; i++)
        {
            int index = rand.Next(0, remainingSpawns.Count);
            List<AIBrain> enemiesPrefabs = remainingSpawns[i].UnlockedEnemiesToSpawn;
            Transform spawn = remainingSpawns[i].transform;

            Character enemy = Instantiate(enemiesPrefabs[rand.Next(0, enemiesPrefabs.Count)], spawn.position, spawn.rotation).GetComponent<Character>();
            var enemyHealth = enemy.GetComponent<Health>();

            enemies.Add(enemy);

            enemyHealth.OnDeath += CheckIfEnemiesAreAllDead;
            enemy.transform.parent = room.transform;
        }

        // foreach (Transform child in spawnersParent)
        // {
        //     if (child.gameObject.activeSelf)
        //     {
        //         Character enemy = Instantiate(enemiesPrefabs[rand.Next(0, enemiesPrefabs.Count)], child.position, child.rotation).GetComponent<Character>();
        //         enemies.Add(enemy);
        //         var enemyHealth = enemy.GetComponent<Health>();
        //         enemyHealth.OnDeath += CheckIfEnemiesAreAllDead;
        //         enemy.transform.parent = room.transform;
        //     }
        // }

        DisableAI();

        room.OnPlayerEntersRoomForTheFirstTime.AddListener(OnPlayerEnteredRoom);
        room.OnPlayerEntersRoom.AddListener(OnPlayerEnteredRoom);
        room.OnPlayerExitsRoom.AddListener(OnPlayerLeftRoom);
    }

    void OnPlayerEnteredRoom()
    {
        EnableAI();
    }

    void OnPlayerLeftRoom()
    {
        DisableAI();
    }

    void EnableAI()
    {
        foreach (var enemy in enemies)
        {
            if (enemy.ConditionState.CurrentState == CharacterStates.CharacterConditions.Dead) { continue; }

            enemy.UnFreeze();
            var enemyBrain = enemy.CharacterBrain;
            enemyBrain.BrainActive = true;
            enemyBrain.ResetBrain();
        }
    }

    void DisableAI()
    {
        foreach (var enemy in enemies)
        {
            if (enemy.ConditionState.CurrentState == CharacterStates.CharacterConditions.Dead) { continue; }

            enemy.Freeze();
            var enemyBrain = enemy.CharacterBrain;
            enemyBrain.ResetBrain();
            enemyBrain.BrainActive = false;
        }
    }

    void CheckIfEnemiesAreAllDead()
    {
        foreach (var enemy in enemies)
        {
            if (enemy.ConditionState.CurrentState != CharacterConditions.Dead)
            {
                Debug.Log("Still people alive!");
                return;
            }
        }
        Debug.Log("All dead! Open Doors!");
        GetComponent<RoomEntrances>().OpenDoors();
    }
}