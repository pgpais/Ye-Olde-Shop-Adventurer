using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuffSpawner : MonoBehaviour
{
    [field: SerializeField] public Transform Spawner { get; private set; }

    [SerializeField] BuffPickable buffPickablePrefab;

    [SerializeField] bool spawnOnStart;

    private void Start()
    {

        var entrances = GetComponent<RoomEntrances>();
        entrances.RoomGenerated.AddListener(Initialize);
        Debug.Log("listener");
    }

    public void Initialize()
    {
        var rand = MissionManager.instance.Rand;
        var missionManager = MissionManager.instance;
        List<Buff> unlockedBuffs = missionManager.BuffList.GetUnlockedBuffs();
        if (unlockedBuffs.Count <= 0)
        {
            Debug.LogError("No unlocked buffs!");
            return;
        }
        SpawnBuff(unlockedBuffs[rand.Next(0, unlockedBuffs.Count)]);
    }

    void SpawnBuff(Buff buff)
    {
        Instantiate(buffPickablePrefab, Spawner).buffToGive = buff;
    }
}