using System.Collections;
using UnityEngine;

public class EnemyController2 : MonoBehaviour
{
    public float moveSpeed; // Vitesse de d�placement de l'ennemi
    public float updateInterval; // Intervalle entre les mises � jour en secondes
    public float detectionRange; // Distance de d�tection du joueur
    public LayerMask layerMask; // LayerMask pour la detection du joueur

    private GameObject player; // R�f�rence au joueur
    private Rigidbody2D rb2D; // R�f�rence au Rigidbody2D de l'ennemi
    
    [SerializeField] private int damageAmount = 1; // D�g�ts inflig�s au joueur par l'ennemi
    [SerializeField] private float attackRange = 1.0f; // Distance d'attaque de l'ennemi
    [SerializeField] private float attackCooldown = 1.0f; // Temps entre chaque attaque
    private float attackTimer; // Chronom�tre pour g�rer le temps entre les attaques

    private void Start()
    {
        LevelManager.OnLevelEnded += DestroyObject;

        // R�cup�ration de variable
        rb2D = GetComponent<Rigidbody2D>();
        if ( rb2D == null) { throw new System.Exception("No component Rigidbody2D on this gameObject"); }
        
        // Upadate tous les updateInterval secondes
        StartCoroutine(UpdateEnemy());
    }

    
    private void Update()
    {
        // V�rifie si le joueur est � port�e d'attaque
        if (player != null && Vector3.Distance(transform.position, player.transform.position) <= attackRange)
        {
            // V�rifie si l'ennemi peut attaquer (cooldown termin�)
            if (attackTimer <= 0f)
            {
                AttackPlayer();
                attackTimer = attackCooldown; // R�initialise le chronom�tre de cooldown
            }
        }

        // R�duit le chronom�tre de cooldown
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
        // R�cup�re le script PlayerHealth attach� au joueur
        PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();

        // Si le joueur a un script PlayerHealth, lui inflige des d�g�ts
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(damageAmount);
            Debug.Log("Le joueur a pris " + damageAmount + " d�g�t(s) de l'ennemi.");
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
                        // Le raycast a touch� le joueur, d�placer l'ennemi
                        rb2D.velocity = directionToPlayer * moveSpeed;
                    }
                    else
                    {
                        // Le raycast n'a pas touch� le joueur, arr�ter l'ennemi
                        rb2D.velocity = Vector2.zero;
                    }
                }
                else
                {
                    // Le joueur est hors de port�e, arr�ter l'ennemi
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
