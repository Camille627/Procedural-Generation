using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CamBibUnity;

public class WeaponManager : MonoBehaviour
{
    [SerializeField] private WeaponData primaryWeapon; // Reference l'arme principale
    [SerializeField] private WeaponData currentWeapon; // Reference l'arme acctuelle

    private Camera playerCamera; // Reference a la camera pour savoir dans quelle direction on vise

    [SerializeField] private int orderInLayer; // Order in layer du component Graphic de l'arme instanciée
    private float nextFireTime = 0f; // Temps avant de pouvoir tirer de nouveau
    private float fireInterval; // Intervalle de temps entre les tirs
    private Transform currentWeaponInstance;
    private SpriteRenderer weaponGraphicSR;
    private Transform firePoint;

    // get
    public WeaponData CurrentWeapon() { return currentWeapon; }

    private void Start()
    {
        playerCamera = Camera.main;
        EquipWeapon(primaryWeapon);
    }

    private void Update()
    {
        FaceMouse();

        if (Input.GetKey(OptionsManager.fireKey)) { HandleShooting(); }
    }

    // Changement d'arme

    public void EquipWeapon(WeaponData weapon)
    {
        currentWeapon = weapon;
        fireInterval = 60f / weapon.fireRate;

        // Detruis l'instance d'arme actuelle
        if (currentWeaponInstance != null) { Destroy(currentWeaponInstance.gameObject); }

        // Instancier l'arme et récupérer la référence à l'instance créée
        GameObject weaponInstance = Instantiate(weapon.model, transform.position, Quaternion.identity, transform);
        if (weaponInstance == null) { throw new System.Exception("Weapon instance could not be created."); }
        currentWeaponInstance = weaponInstance.transform;
        
        weaponGraphicSR = weaponInstance.transform.Find("Graphic").GetComponent<SpriteRenderer>();
        if (weaponGraphicSR == null) { throw new System.Exception("SpriteRenderer not found in the weapon instance."); }

        firePoint = weaponInstance.transform.Find("Graphic/FirePoint");
        if (firePoint == null) { throw new System.Exception("FirePoint not found in the weapon instance."); }

        // Graphic Order in Layer
        weaponGraphicSR.sortingOrder = orderInLayer;
    }

    // Visée

    /// <summary>
    /// Oriente dans la direction de la souris
    /// </summary>
    private void FaceMouse()
    {
        Vector3 direction = playerCamera.ScreenToWorldPoint(Input.mousePosition) - transform.position;
        direction.z = 0; // Annule l'incidence de z

        currentWeaponInstance.right = direction;
        if (direction.x > 0.01f) { weaponGraphicSR.flipY = false; }
        else if (direction.x < -0.01f) { weaponGraphicSR.flipY = true; }
        
        // faire passer l'arme derrière le joueur quand il tire vers le haut.
        //    if (direction.y > 0.1f) { spriteRenderer.sortingOrder = 9; }
        //    else if (direction.y < -0.1f) { spriteRenderer.sortingOrder = 12; }
    }
    
    // Tir
    public void HandleShooting()
    {
        if (currentWeapon.isAutomatic)
        {
            // Tir automatique
            if (Input.GetKey(OptionsManager.fireKey) && Time.time >= nextFireTime)
            {
                Shoot();
                nextFireTime = Time.time + fireInterval;
            }
        }
        else
        {
            // Tir semi-automatique
            if (Input.GetKeyDown(OptionsManager.fireKey) && Time.time >= nextFireTime)
            {
                Shoot();
                nextFireTime = Time.time + fireInterval;
            }
        }
    }

    private void Shoot()
    {
        // Verifie que l'arme ne traverse pas un mur
        LayerMask layerMask = 1 << LayerMask.NameToLayer("Obstacles"); // inclu donc les murs uniquements
        RaycastHit2D hit = Raycast2DUtils.Entre2Positions(currentWeaponInstance.position, firePoint.position, layerMask);
        if (hit.collider != null && hit.collider.CompareTag("Mur")) { return; }
        // Instancie le projectile au point de départ
        Vector3 direction = (playerCamera.ScreenToWorldPoint(Input.mousePosition) - transform.position).normalized;
        GameObject projectile = Instantiate(currentWeapon.projectilePrefab, firePoint.position, firePoint.rotation);
        // Initialiser le projectile avec la portée et les dégâts de l'arme
        ProjectileManager projectileScript = projectile.GetComponent<ProjectileManager>();
        if (projectileScript != null) { projectileScript.Set(currentWeapon.damage, currentWeapon.range); }
        else { throw new System.Exception("Le prefab du projectile n'est pas valide"); }
        // Ajout de vélocité à la balle
        projectile.GetComponent<Rigidbody2D>().velocity = direction * currentWeapon.projectileSpeed;
    }
}
