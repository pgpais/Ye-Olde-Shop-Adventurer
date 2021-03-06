using MoreMountains.TopDownEngine;
using UnityEngine;

[CreateAssetMenu(fileName = "ClassData", menuName = "Ye-Olde-Shop-Adventurer/Class Data", order = 0)]
public class CharacterClassData : ScriptableObject, UnlockableReward
{
    public bool Unlocked => unlocked;

    public string ClassName
    {
        get => Localisation.Get(classNameKey);
    }

    private bool unlocked = false;

    [Tooltip("The name given to this class")]
    public StringKey classNameKey;

    [Tooltip("How much health this class adds (or subtracts) to base value")]
    public int healthModifier = 0;

    [Tooltip("How much damage this class adds (or subtracts) to base value")]
    public int attackDamageModifier = 0;

    [Tooltip("The multiplayer for base movement speed from this class")]
    public float movementSpeedMultiplier = 0;

    public Weapon initialWeapon;

    public bool startsUnlocked = false;


    public void Unlock()
    {
        unlocked = true;
    }

    private void OnEnable()
    {
        unlocked = startsUnlocked;
    }
}
