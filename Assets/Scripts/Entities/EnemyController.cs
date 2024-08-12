using System.Collections;
using UnityEngine;

public class EnemyController : MonoBehaviour, IDamageable
{
    public const float maxHealth = 1f; // Quantité maximale de points de vie
    public float moveSpeed; // Vitesse de déplacement de l'ennemi
    public float updateInterval; // Intervalle entre les mises à jour en secondes
    public float detectionRange; // Distance de détection du joueur
    public LayerMask layerMask; // LayerMask pour la detection du joueur

    private float currentHealth = maxHealth; // Quantité de points de vie actuelle
    private GameObject target; // Référence à la cible de l'entité
    private Rigidbody2D rb2D; // Référence au Rigidbody2D de l'entité

    [SerializeField] private int damageAmount = 1; // Dégâts infligés au joueur par l'ennemi
    [SerializeField] private float attackRange = 1.0f; // Distance d'attaque de l'ennemi
    [SerializeField] private float attackCooldown = 1.0f; // Temps entre chaque attaque
    private float attackTimer; // Chronomètre pour gérer le temps entre les attaques


    /* Pour prévenir le LevelManager de a destruction */
    /// <summary>
    /// Délégué pour l'événement de destruction de l'entité.
    /// </summary>
    /// <param name="valeur">Le GameObject qui a été détruit.</param>
    public delegate void GameOjectDelegate(GameObject valeur);
    /// <summary>
    /// Événement déclenché lorsque le gameObject est détruit.
    /// </summary>
    public event GameOjectDelegate OnKilled;

    /* IDamageable */
    /// <summary>
    /// Gère les dégats reçus par l'entité
    /// (Implémentation de IDamageable)
    /// </summary>
    /// <param name="amount">quantité de dégats reçu</param>
    public void Hit(int amount)
    {
        //diminution de vie en fonction des dégats
        currentHealth -= amount;

        //si la vie actuelle atteint 0 le personnage meurt
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            // Déclenche l'événement de mort
            OnKilled?.Invoke(gameObject);
            // Détruit le gameObject
            Destroy(gameObject);
        }

    }


    private void Start()
    {
        // Abonnement à l'évènement de fin du niveau (destruction du GameObject avec expression lambda)
        LevelManager.OnLevelEnded += DestroyGameObject;

        // Récupération de variable
        rb2D = GetComponent<Rigidbody2D>();
        if (rb2D == null) { throw new System.Exception("No component Rigidbody2D on this gameObject"); }

        // Upadate tous les updateInterval secondes
        StartCoroutine(UpdateEnemy());
    }


    private void Update()
    {
        // Vérifie si le joueur est à portée d'attaque
        if (target != null && Vector3.Distance(transform.position, target.transform.position) <= attackRange)
        {
            // Vérifie si l'ennemi peut attaquer (cooldown terminé)
            if (attackTimer <= 0f)
            {
                AttackPlayer();
                attackTimer = attackCooldown; // Réinitialise le chronomètre de cooldown
            }
        }

        // Réduit le chronomètre de cooldown
        if (attackTimer > 0f)
        {
            attackTimer -= Time.deltaTime;
        }
    }


    /// <summary>
    /// Attaquer le joueur
    /// </summary>
    private void AttackPlayer()
    {
        // Récupère le script PlayerHealth attaché au joueur
        PlayerHealth playerHealth = target.GetComponent<PlayerHealth>();

        // Si le joueur a un script PlayerHealth, lui inflige des dégâts
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(damageAmount);
            Debug.Log("Le joueur a pris " + damageAmount + " dégât(s) de l'ennemi.");
        }
    }

    public void FindPlayer()
    {
        target = GameObject.FindGameObjectWithTag("Player");
    }

    IEnumerator UpdateEnemy()
    {
        while (true)
        {
            if (target != null)
            {
                float distanceToPlayer = Vector3.Distance(transform.position, target.transform.position);
                if (distanceToPlayer <= detectionRange)
                {
                    Vector2 directionToPlayer = (target.transform.position - transform.position).normalized;
                    RaycastHit2D hit = Physics2D.Raycast(transform.position, directionToPlayer, detectionRange, layerMask);

                    if (hit.collider != null && hit.collider.gameObject.CompareTag("Player"))
                    {
                        // Le raycast a touché le joueur, déplacer l'ennemi
                        rb2D.velocity = directionToPlayer * moveSpeed;
                    }
                    else
                    {
                        // Le raycast n'a pas touché le joueur, arrêter l'ennemi
                        rb2D.velocity = Vector2.zero;
                    }
                }
                else
                {
                    // Le joueur est hors de portée, arrêter l'ennemi
                    rb2D.velocity = Vector2.zero;
                }
            }
            else
            {
                FindPlayer();
            }

            yield return new WaitForSeconds(updateInterval);
        }
    }


    /* OnDestroy */

    /// <summary>
    /// Abonnée à l'evenement OnLevelEnded du LevelManager
    /// </summary>
    private void DestroyGameObject() { Destroy(gameObject); }

    /// <summary>
    /// Méthode appelée par Unity juste avant que l'objet ne soit détruit.
    /// Déclenche l'événement OnDestroyed pour notifier les abonnés.
    /// Se désabonne de l'évènement OnLevelEnded du LevelManager
    /// </summary>
    void OnDestroy()
    {
        // Désabonnement de la destruction lors de la fin du niveau
        LevelManager.OnLevelEnded -= DestroyGameObject;

    }
}
