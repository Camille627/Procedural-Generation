using System.Collections;
using UnityEngine;

public class EnemyController2 : MonoBehaviour
{
    public float moveSpeed; // Vitesse de déplacement de l'ennemi
    public float updateInterval; // Intervalle entre les mises à jour en secondes
    public float detectionRange; // Distance de détection du joueur
    public LayerMask layerMask; // LayerMask pour la detection du joueur

    private GameObject player; // Référence au joueur
    private Rigidbody2D rb2D; // Référence au Rigidbody2D de l'ennemi
    
    [SerializeField] private int damageAmount = 1; // Dégâts infligés au joueur par l'ennemi
    [SerializeField] private float attackRange = 1.0f; // Distance d'attaque de l'ennemi
    [SerializeField] private float attackCooldown = 1.0f; // Temps entre chaque attaque
    private float attackTimer; // Chronomètre pour gérer le temps entre les attaques

    private void Start()
    {
        LevelManager.OnLevelEnded += DestroyObject;

        // Récupération de variable
        rb2D = GetComponent<Rigidbody2D>();
        if ( rb2D == null) { throw new System.Exception("No component Rigidbody2D on this gameObject"); }
        
        // Upadate tous les updateInterval secondes
        StartCoroutine(UpdateEnemy());
    }

    
    private void Update()
    {
        // Vérifie si le joueur est à portée d'attaque
        if (player != null && Vector3.Distance(transform.position, player.transform.position) <= attackRange)
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
    private void DestroyObject() { Destroy(gameObject); }

    private void OnDestroy() { LevelManager.OnLevelEnded -= DestroyObject; }

    /// <summary>
    /// Attaquer le joueur
    /// </summary>
    private void AttackPlayer()
    {
        // Récupère le script PlayerHealth attaché au joueur
        PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();

        // Si le joueur a un script PlayerHealth, lui inflige des dégâts
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(damageAmount);
            Debug.Log("Le joueur a pris " + damageAmount + " dégât(s) de l'ennemi.");
        }
    }

    public void FindPlayer()
    {
        player = GameObject.FindGameObjectWithTag("Player");
    }

    IEnumerator UpdateEnemy()
    {
        while (true)
        {
            if (player != null)
            {
                float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);
                if (distanceToPlayer <= detectionRange)
                {
                    Vector2 directionToPlayer = (player.transform.position - transform.position).normalized;
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
}
