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
        // Assigner le Layer "Projectile" � ce projectile
        gameObject.layer = LayerMask.NameToLayer("Projectile");

        // Ignorer les collisions avec d'autres projectiles sur le m�me Layer
        Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Projectile"), LayerMask.NameToLayer("Projectile"));

        LevelManager.OnLevelEnded += DestroyObject;
        startPosition = transform.position;

        // Detruire dans 5 secondes
        Destroy(gameObject, 5f);
    }

    private void Update()
    {
        // V�rifier si le projectile a atteint sa port�e maximale
        if (Vector3.Distance(startPosition, transform.position) >= range)
        {
            Destroy(gameObject);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // R�cup�ration de l'objet touch�
        var hit = collision.gameObject;
        var health = hit.GetComponent<IDamageable>();
        if (health != null)
        {
            //Application des d�g�ts
            health.TakeDamage(damages);
        }

        Destroy(gameObject);
    }

    private void DestroyObject() { Destroy(gameObject); }

    private void OnDestroy() { LevelManager.OnLevelEnded -= DestroyObject; }
}
