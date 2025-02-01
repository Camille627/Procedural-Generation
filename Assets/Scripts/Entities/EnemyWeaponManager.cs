using UnityEngine;

public class EnemyWeaponManager : MonoBehaviour
{
    [SerializeField] private WeaponData primaryWeapon; // Référence à l'arme principale
    [SerializeField] private WeaponData currentWeapon; // Référence à l'arme actuelle

    [SerializeField] private int orderInLayer; // Order in layer du component Graphic de l'arme instanciée
    private float nextFireTime = 0f; // Temps avant de pouvoir tirer de nouveau
    private float fireInterval; // Intervalle de temps entre les tirs
    private Transform currentWeaponInstance;
    private SpriteRenderer weaponGraphicSR;
    private Transform firePoint;

    private void Awake()
    {
        EquipWeapon(primaryWeapon); // Équipe l'arme principale
    }

    // get
    public WeaponData CurrentWeapon() { return currentWeapon; }

    /// <summary>
    /// Équipe une nouvelle arme pour l'ennemi
    /// </summary>
    public void EquipWeapon(WeaponData weapon)
    {
        currentWeapon = weapon;
        fireInterval = 60f / weapon.fireRate;

        // Détruit l'instance d'arme actuelle
        if (currentWeaponInstance != null)
        {
            Destroy(currentWeaponInstance.gameObject);
        }

        // Instancie l'arme et récupère la référence à l'instance créée
        GameObject weaponInstance = Instantiate(weapon.model, transform.position, Quaternion.identity, transform);
        if (weaponInstance == null)
        {
            throw new System.Exception("Weapon instance could not be created.");
        }
        currentWeaponInstance = weaponInstance.transform;

        weaponGraphicSR = weaponInstance.transform.Find("Graphic").GetComponent<SpriteRenderer>();
        if (weaponGraphicSR == null)
        {
            throw new System.Exception("SpriteRenderer not found in the weapon instance.");
        }

        firePoint = weaponInstance.transform.Find("Graphic/FirePoint");
        if (firePoint == null)
        {
            throw new System.Exception("FirePoint not found in the weapon instance.");
        }

        // Graphic Order in Layer
        weaponGraphicSR.sortingOrder = orderInLayer;
    }

    /// <summary>
    /// Oriente l'arme de l'ennemi vers une position donnée
    /// </summary>
    public void AimAtPosition(Vector3 targetPosition)
    {
        Vector3 direction = targetPosition - transform.position;
        direction.z = 0; // Annule l'incidence de z

        currentWeaponInstance.right = direction;
        if (direction.x > 0.01f)
        {
            weaponGraphicSR.flipY = false;
        }
        else if (direction.x < -0.01f)
        {
            weaponGraphicSR.flipY = true;
        }
    }

    /// <summary>
    /// Tente de tirer sur la cible à une position donnée.
    /// L'arme est orientée vers la position spécifiée, et si le cooldown du tir est écoulé,
    /// le tir est effectué.
    /// </summary>
    /// <param name="targetPosition">La position de la cible vers laquelle l'ennemi tente de tirer.</param>
    /// <returns>
    /// Retourne un entier indiquant le résultat du tir :
    /// 0 si le tir a réussi (le cooldown était écoulé et l'ennemi a tiré),
    /// 1 si le tir a échoué (le cooldown n'était pas encore écoulé).
    /// </returns>
    public int TryShootingAt(Vector3 targetPosition)
    {
        // Oriente l'arme vers la position cible
        AimAtPosition(targetPosition);

        // Vérifie si le cooldown du tir est écoulé
        if (Time.time >= nextFireTime)
        {
            Shoot(); // Effectue le tir
            nextFireTime = Time.time + fireInterval; // Met à jour le temps du prochain tir
            return 0; // Indique que le tir a réussi
        }

        return 1; // Indique que le tir a échoué (cooldown non écoulé)
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
        if (hit.collider != null && hit.collider.CompareTag("Obstacle"))
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
