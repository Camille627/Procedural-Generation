using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    [SerializeField] private WeaponData primaryWeapon; // Reference l'arme principale
    [SerializeField] private WeaponData currentWeapon; // Reference l'arme acctuelle

    [SerializeField] private int orderInLayer; // Order in layer du component Graphic de l'arme instanciée
    private float nextFireTime = 0f; // Temps avant de pouvoir tirer de nouveau
    private float fireInterval; // Intervalle de temps entre les tirs
    private Transform currentWeaponInstance;
    private SpriteRenderer weaponGraphicSR;
    private Transform firePoint;

    private Camera playerCamera; // Reference a la camera pour savoir dans quelle direction on vise

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
        if (currentWeapon.reloadType == ReloadType.Automatic)
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
            // Tir semi-automatique ou manuel
            if (Input.GetKeyDown(OptionsManager.fireKey) && Time.time >= nextFireTime)
            {
                Shoot();
                nextFireTime = Time.time + fireInterval;
            }
        }
    }

    /// <summary>
    /// Tire un projectile dans la direction dans laquelle l'arme est actuellement orientée.
    /// Le tir est effectué uniquement si aucun obstacle n'est détecté entre le point de départ et le point de tir.
    /// Les collisions entre le projectile et le tireur sont ignorées.
    /// </summary>
    private void Shoot()
    {
        // Vérifie que l'arme ne traverse pas un obstacle avant de tirer
        LayerMask layerMask = 1 << LayerMask.NameToLayer("Obstacles"); // Inclut uniquement les obstacles
        RaycastHit2D hit = Physics2D.Linecast(currentWeaponInstance.position, firePoint.position, layerMask);
        if (hit.collider != null)
        {
            // Si un obstacle est détecté, le tir est annulé
            return;
        }

        // Détermine la direction du tir en utilisant l'orientation actuelle de l'arme
        Vector3 direction = firePoint.right; // Utilise l'orientation actuelle du FirePoint comme direction du projectile

        // Instancie le projectile au point de tir
        GameObject projectile = Instantiate(currentWeapon.projectilePrefab, firePoint.position, firePoint.rotation);

        // Ignore la collision entre le projectile et le tireur pour éviter les auto-collisions
        Physics2D.IgnoreCollision(projectile.GetComponent<Collider2D>(), gameObject.GetComponent<Collider2D>());

        // Initialise les propriétés du projectile avec la portée et les dégâts de l'arme actuelle
        ProjectileManager projectileScript = projectile.GetComponent<ProjectileManager>();
        if (projectileScript != null)
        {
            projectileScript.Set(currentWeapon.damage, currentWeapon.range);
        }
        else
        {
            // Si le prefab du projectile est invalide, une exception est levée
            throw new System.Exception("Le prefab du projectile n'est pas valide");
        }

        // Applique une vélocité au projectile pour le faire se déplacer dans la direction définie
        projectile.GetComponent<Rigidbody2D>().velocity = direction * currentWeapon.projectileSpeed;
    }


}
