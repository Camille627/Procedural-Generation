using System.Collections;
using UnityEngine;

public class EnemyController : MonoBehaviour, IDamageable
{
    public const float maxHealth = 1f; // Quantit� maximale de points de vie
    public float moveSpeed; // Vitesse de d�placement de l'ennemi
    public float updateInterval; // Intervalle entre les mises � jour en secondes
    public float detectionRange; // Distance de d�tection du joueur
    public LayerMask layerMask; // LayerMask pour la detection du joueur

    private float currentHealth = maxHealth; // Quantit� de points de vie actuelle
    private GameObject target; // R�f�rence � la cible de l'entit�
    private Rigidbody2D rb2D; // R�f�rence au Rigidbody2D de l'entit�

    [SerializeField] private int damageAmount = 1; // D�g�ts inflig�s au joueur par l'ennemi
    [SerializeField] private float attackRange = 1.0f; // Distance d'attaque de l'ennemi
    [SerializeField] private float attackCooldown = 1.0f; // Temps entre chaque attaque
    private float attackTimer; // Chronom�tre pour g�rer le temps entre les attaques


    /* Pour pr�venir le LevelManager de a destruction */
    /// <summary>
    /// D�l�gu� pour l'�v�nement de destruction de l'entit�.
    /// </summary>
    /// <param name="valeur">Le GameObject qui a �t� d�truit.</param>
    public delegate void GameOjectDelegate(GameObject valeur);
    /// <summary>
    /// �v�nement d�clench� lorsque le gameObject est d�truit.
    /// </summary>
    public event GameOjectDelegate OnKilled;

    /* IDamageable */
    /// <summary>
    /// G�re les d�gats re�us par l'entit�
    /// (Impl�mentation de IDamageable)
    /// </summary>
    /// <param name="amount">quantit� de d�gats re�u</param>
    public void Hit(int amount)
    {
        //diminution de vie en fonction des d�gats
        currentHealth -= amount;

        //si la vie actuelle atteint 0 le personnage meurt
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            // D�clenche l'�v�nement de mort
            OnKilled?.Invoke(gameObject);
            // D�truit le gameObject
            Destroy(gameObject);
        }

    }


    private void Start()
    {
        // Abonnement � l'�v�nement de fin du niveau (destruction du GameObject avec expression lambda)
        LevelManager.OnLevelEnded += DestroyGameObject;

        // R�cup�ration de variable
        rb2D = GetComponent<Rigidbody2D>();
        if (rb2D == null) { throw new System.Exception("No component Rigidbody2D on this gameObject"); }

        // Upadate tous les updateInterval secondes
        StartCoroutine(UpdateEnemy());
    }


    private void Update()
    {
        // V�rifie si le joueur est � port�e d'attaque
        if (target != null && Vector3.Distance(transform.position, target.transform.position) <= attackRange)
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


    /// <summary>
    /// Attaquer le joueur
    /// </summary>
    private void AttackPlayer()
    {
        // R�cup�re le script PlayerHealth attach� au joueur
        PlayerHealth playerHealth = target.GetComponent<PlayerHealth>();

        // Si le joueur a un script PlayerHealth, lui inflige des d�g�ts
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(damageAmount);
            Debug.Log("Le joueur a pris " + damageAmount + " d�g�t(s) de l'ennemi.");
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


    /* OnDestroy */

    /// <summary>
    /// Abonn�e � l'evenement OnLevelEnded du LevelManager
    /// </summary>
    private void DestroyGameObject() { Destroy(gameObject); }

    /// <summary>
    /// M�thode appel�e par Unity juste avant que l'objet ne soit d�truit.
    /// D�clenche l'�v�nement OnDestroyed pour notifier les abonn�s.
    /// Se d�sabonne de l'�v�nement OnLevelEnded du LevelManager
    /// </summary>
    void OnDestroy()
    {
        // D�sabonnement de la destruction lors de la fin du niveau
        LevelManager.OnLevelEnded -= DestroyGameObject;

    }
}
