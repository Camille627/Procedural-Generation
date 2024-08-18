using System.Collections;
using UnityEngine;

public class EnemyController : MonoBehaviour, IDamageable
{
    public float maxHealth = 2f;
    private float currentHealth;

    public float meleeDamage = 1f;
    public float meleeRange = 1.5f; // Portée de l'attaque de mêlée
    public float meleeAttackCooldown = 1f; // Temps de recharge entre les attaques de mêlée

    public Transform target;
    public LayerMask obstacleLayer; // Layer des obstacles qui peuvent bloquer la vue

    private float distanceToTarget;
    private bool isTargetVisible;

    private Rigidbody2D rb; // Composant Rigidbody2D pour gérer la vélocité

    private enum State { Idle, MeleeAttack }
    private State currentState = State.Idle;

    public float idleWaitTimeClose = 0.2f; // Durée de l'état d'attente si la cible est proche
    public float idleWaitTimeFar = 2f; // Durée de l'état d'attente si la cible est éloignée
    public float closeDistanceThreshold = 20f; // Distance seuil pour considérer la cible comme "proche"

    public delegate void GameObjectDelegate(GameObject valeur);
    public event GameObjectDelegate OnKilled;

    private void Start()
    {
        currentHealth = maxHealth;
        target = GameObject.Find("Player").transform;
        rb = GetComponent<Rigidbody2D>(); // Initialiser le Rigidbody2D

        LevelManager.OnLevelEnded += DestroyGameObject;

        StateManager(); // Démarre la machine d'état
    }

    private void StateManager()
    {
        UpdateInfo(); // Mise à jour des informations nécessaires à la prise de décision

        // Arbre de décision pour la gestion des états
        if (distanceToTarget <= meleeRange && isTargetVisible)
        {
            // Si la cible est à portée de mêlée et visible, passer à l'état d'attaque de mêlée
            currentState = State.MeleeAttack;
        }
        else
        {
            // Sinon, rester en état Idle
            currentState = State.Idle;
        }

        // Appeler la méthode correspondant à l'état actuel
        switch (currentState)
        {
            case State.Idle:
                HandleIdleState();
                break;

            case State.MeleeAttack:
                HandleMeleeAttackState();
                break;
        }
    }

    private void UpdateInfo()
    {
        // Calcul de la distance à la cible
        distanceToTarget = Vector2.Distance(transform.position, target.position);

        // Vérification de la visibilité de la cible avec un raycast
        Vector2 directionToTarget = target.position - transform.position;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, directionToTarget, distanceToTarget, obstacleLayer);

        // Si le raycast ne touche rien, la cible est visible
        isTargetVisible = hit.collider == null;
    }

    private void HandleIdleState()
    {
        // Annuler la vélocité
        rb.velocity = Vector2.zero;

        float idleWaitTime = distanceToTarget <= closeDistanceThreshold ? idleWaitTimeClose : idleWaitTimeFar;

        StartCoroutine(IdleCoroutine(idleWaitTime));
    }

    private IEnumerator IdleCoroutine(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);

        // Après l'attente, repasser à l'évaluation de l'état
        StateManager();
    }

    private void HandleMeleeAttackState()
    {
        // Vérifier si le cooldown est terminé avant de pouvoir attaquer
        StartCoroutine(MeleeAttackCoroutine());
    }

    private IEnumerator MeleeAttackCoroutine()
    {
        // Logique d'attaque de mêlée
        Debug.Log("Attaque de mêlée exécutée!");

        // Appliquer les dégâts à la cible
        if (distanceToTarget <= meleeRange)
        {
            // Application des dégâts au joueur
            IDamageable damageable = target.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage((int)meleeDamage);
            }
        }

        // Attendre avant de pouvoir attaquer à nouveau
        yield return new WaitForSeconds(meleeAttackCooldown);

        // Retourner à l'état Idle après l'attaque
        currentState = State.Idle;
        StateManager();
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            OnKilled?.Invoke(gameObject);
            Destroy(gameObject);
        }
    }

    private void DestroyGameObject()
    {
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        LevelManager.OnLevelEnded -= DestroyGameObject;
    }
}
