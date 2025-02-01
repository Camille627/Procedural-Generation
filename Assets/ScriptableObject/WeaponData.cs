using UnityEngine;

public enum ReloadType
{
    Manual,
    SemiAutomatic,
    Automatic
}

[CreateAssetMenu(fileName = "NewWeaponData", menuName = "Weapon Data", order = 51)]
public class WeaponData : ScriptableObject
{
    public string weaponName;
    public int damage = 1; // Dégâts de l'arme
    public ReloadType reloadType = ReloadType.Manual; // Type de rechargement de l'arme
    public int fireRate = 60; // Coups par minute
    public int range = 10; // Portée de l'arme
    public int projectileSpeed = 30; 

    public GameObject model;
    public GameObject projectilePrefab;
}
