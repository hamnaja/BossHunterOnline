using UnityEngine;

public enum WeaponType { Greatsword, DualBlade, Hammer, Spear, Bow }

[CreateAssetMenu(fileName = "New Weapon", menuName = "BossHunter/Weapon Data")]
public class WeaponData : ItemData
{
    public WeaponType weaponType;
    public float baseDamage;
    public float attackSpeedModifier;
    public float staggerValue;
}