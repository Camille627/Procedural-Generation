using UnityEngine;

public class ProjectileManager : MonoBehaviour
{
    private int range;
    private int damages;
    private Vector3 startPosition;

    public void Set(int weaponDamage, int weaponRange)
    {
        damages = weaponDamage;
        range = weaponRange;
    }

    private void Start()
    {
        // Assigner le Layer "Projectile" à ce projectile
        gameObject.layer = LayerMask.NameToLayer("Projectile");

        // Ignorer les collisions avec d'autres projectiles sur le même Layer
        Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Projectile"), LayerMask.NameToLayer("Projectile"));

        LevelManager.OnLevelEnded += DestroyObject;
        startPosition = transform.position;

        // Detruire dans 5 secondes
        Destroy(gameObject, 5f);
    }

    private void Update()
    {
        // Vérifier si le projectile a atteint sa portée maximale
        if (Vector3.Distance(startPosition, transform.position) >= range)
        {
            Destroy(gameObject);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Récupération de l'objet touché
        var hit = collision.gameObject;
        var health = hit.GetComponent<IDamageable>();
        if (health != null)
        {
            //Application des dégâts
            health.TakeDamage(damages);
        }

        Destroy(gameObject);
    }

    private void DestroyObject() { Destroy(gameObject); }

    private void OnDestroy() { LevelManager.OnLevelEnded -= DestroyObject; }
}
