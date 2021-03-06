using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "Unlockable", menuName = "Ye Olde Shop/Unlockable", order = 0)]
public class Unlockable : SerializedScriptableObject, IComparable<Unlockable>
{
    [HideInInspector]
    public UnityEvent<Unlockable> UnlockableUpdated;


    public string UnlockableNameKey => Localisation.Get(unlockableStringKey, Language.English) + " " + unlockableNameAddition;


    public string UnlockableName => Localisation.Get(unlockableStringKey) + " " + unlockableNameAddition;
    public UnlockCategory Category => category;


    [SerializeField] StringKey unlockableStringKey;

    [Tooltip("Add to unlocks with same name but different levels")]
    [SerializeField] private string unlockableNameAddition;
    [field: SerializeField]
    public string UnlockableDescription { get; private set; }
    [field: SerializeField] public Sprite UnlockableIcon { get; private set; }
    [SerializeField] UnlockCategory category;

    [Space]
    [SerializeField] List<UnlockableReward> rewards;

    [field: SerializeField] public int DifficultyIncrease { get; private set; } = 0;

    // TODO: list of items required for build


    [SerializeField] bool unlocked;
    public bool Unlocked
    {
        get => runTimeUnlocked;
        set
        {
            runTimeUnlocked = value;

        }
    }
    private bool runTimeUnlocked;


    private void OnEnable()
    {
        runTimeUnlocked = unlocked;
    }

    public void InitializeEvent()
    {
        UnlockableUpdated = new UnityEvent<Unlockable>();
    }

    internal void Unlock()
    {

        Unlocked = true;
        UnlockableUpdated.Invoke(this);
        foreach (var reward in rewards)
        {
            reward.Unlock();
        }
    }

    public int CompareTo(Unlockable other)
    {
        if (this.Unlocked && !other.Unlocked)
        {
            return -1;
        }
        if (!this.Unlocked && other.Unlocked)
        {
            return 1;
        }
        return this.UnlockableName.CompareTo(other.UnlockableName);
    }
}

public enum UnlockCategory
{
    Classes,
    Altar,
    Resources,
    Stats,
}