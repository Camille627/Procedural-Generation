using UnityEngine;

public class EnemyWeaponManager : MonoBehaviour
{
    [SerializeField] private WeaponData primaryWeapon; // R�f�rence � l'arme principale
    [SerializeField] private WeaponData currentWeapon; // R�f�rence � l'arme actuelle

    [SerializeField] private int orderInLayer; // Order in layer du component Graphic de l'arme instanci�e
    private float nextFireTime = 0f; // Temps avant de pouvoir tirer de nouveau
    private float fireInterval; // Intervalle de temps entre les tirs
    private Transform currentWeaponInstance;
    private SpriteRenderer weaponGraphicSR;
    private Transform firePoint;

    private void Awake()
    {
        EquipWeapon(primaryWeapon); // �quipe l'arme principale
    }

    // get
    public WeaponData CurrentWeapon() { return currentWeapon; }

    /// <summary>
    /// �quipe une nouvelle arme pour l'ennemi
    /// </summary>
    public void EquipWeapon(WeaponData weapon)
    {
        currentWeapon = weapon;
        fireInterval = 60f / weapon.fireRate;

        // D�truit l'instance d'arme actuelle
        if (currentWeaponInstance != null)
        {
            Destroy(currentWeaponInstance.gameObject);
        }

        // Instancie l'arme et r�cup�re la r�f�rence � l'instance cr��e
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
    /// Oriente l'arme de l'ennemi vers une position donn�e
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
    /// Tente de tirer sur la cible � une position donn�e.
    /// L'arme est orient�e vers la position sp�cifi�e, et si le cooldown du tir est �coul�,
    /// le tir est effectu�.
    /// </summary>
    /// <param name="targetPosition">La position de la cible vers laquelle l'ennemi tente de tirer.</param>
    /// <returns>
    /// Retourne un entier indiquant le r�sultat du tir :
    /// 0 si le tir a r�ussi (le cooldown �tait �coul� et l'ennemi a tir�),
    /// 1 si le tir a �chou� (le cooldown n'�tait pas encore �coul�).
    /// </returns>
    public int TryShootingAt(Vector3 targetPosition)
    {
        // Oriente l'arme vers la position cible
        AimAtPosition(targetPosition);

        // V�rifie si le cooldown du tir est �coul�
        if (Time.time >= nextFireTime)
        {
            Shoot(); // Effectue le tir
            nextFireTime = Time.time + fireInterval; // Met � jour le temps du prochain tir
            return 0; // Indique que le tir a r�ussi
        }

        return 1; // Indique que le tir a �chou� (cooldown non �coul�)
    }


    /// <summary>
    /// Tire un projectile dans la direction dans laquelle l'arme est actuellement orient�e.
    /// Le tir est effectu� uniquement si aucun obstacle n'est d�tect� entre le point de d�part et le point de tir.
    /// Les collisions entre le projectile et le tireur sont ignor�es.
    /// </summary>
    private void Shoot()
    {
        // V�rifie que l'arme ne traverse pas un obstacle avant de tirer
        LayerMask layerMask = 1 << LayerMask.NameToLayer("Obstacles"); // Inclut uniquement les obstacles
        RaycastHit2D hit = Physics2D.Linecast(currentWeaponInstance.position, firePoint.position, layerMask);
        if (hit.collider != null && hit.collider.CompareTag("Obstacle"))
        {
            // Si un obstacle est d�tect�, le tir est annul�
            return;
        }

        // D�termine la direction du tir en utilisant l'orientation actuelle de l'arme
        Vector3 direction = firePoint.right; // Utilise l'orientation actuelle du FirePoint comme direction du projectile

        // Instancie le projectile au point de tir
        GameObject projectile = Instantiate(currentWeapon.projectilePrefab, firePoint.position, firePoint.rotation);

        // Ignore la collision entre le projectile et le tireur pour �viter les auto-collisions
        Physics2D.IgnoreCollision(projectile.GetComponent<Collider2D>(), gameObject.GetComponent<Collider2D>());

        // Initialise les propri�t�s du projectile avec la port�e et les d�g�ts de l'arme actuelle
        ProjectileManager projectileScript = projectile.GetComponent<ProjectileManager>();
        if (projectileScript != null)
        {
            projectileScript.Set(currentWeapon.damage, currentWeapon.range);
        }
        else
        {
            // Si le prefab du projectile est invalide, une exception est lev�e
            throw new System.Exception("Le prefab du projectile n'est pas valide");
        }

        // Applique une v�locit� au projectile pour le faire se d�placer dans la direction d�finie
        projectile.GetComponent<Rigidbody2D>().velocity = direction * currentWeapon.projectileSpeed;
    }

}
