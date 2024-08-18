using System.Collections;
using UnityEngine;

public class EnemyController : MonoBehaviour, IDamageable
{
    public float maxHealth = 2f;
    private float currentHealth;

    public float meleeDamage = 1f;
    public float meleeRange = 1.5f; // Port�e de l'attaque de m�l�e
    public float meleeAttackCooldown = 1f; // Temps de recharge entre les attaques de m�l�e

    public Transform target;
    public LayerMask obstacleLayer; // Layer des obstacles qui peuvent bloquer la vue

    private float distanceToTarget;
    private bool isTargetVisible;

    private Rigidbody2D rb; // Composant Rigidbody2D pour g�rer la v�locit�

    private enum State { Idle, MeleeAttack }
    private State currentState = State.Idle;

    public float idleWaitTimeClose = 0.2f; // Dur�e de l'�tat d'attente si la cible est proche
    public float idleWaitTimeFar = 2f; // Dur�e de l'�tat d'attente si la cible est �loign�e
    public float closeDistanceThreshold = 20f; // Distance seuil pour consid�rer la cible comme "proche"

    public delegate void GameObjectDelegate(GameObject valeur);
    public event GameObjectDelegate OnKilled;

    private void Start()
    {
        currentHealth = maxHealth;
        target = GameObject.Find("Player").transform;
        rb = GetComponent<Rigidbody2D>(); // Initialiser le Rigidbody2D

        LevelManager.OnLevelEnded += DestroyGameObject;

        StateManager(); // D�marre la machine d'�tat
    }

    private void StateManager()
    {
        UpdateInfo(); // Mise � jour des informations n�cessaires � la prise de d�cision

        // Arbre de d�cision pour la gestion des �tats
        if (distanceToTarget <= meleeRange && isTargetVisible)
        {
            // Si la cible est � port�e de m�l�e et visible, passer � l'�tat d'attaque de m�l�e
            currentState = State.MeleeAttack;
        }
        else
        {
            // Sinon, rester en �tat Idle
            currentState = State.Idle;
        }

        // Appeler la m�thode correspondant � l'�tat actuel
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
        // Calcul de la distance � la cible
        distanceToTarget = Vector2.Distance(transform.position, target.position);

        // V�rification de la visibilit� de la cible avec un raycast
        Vector2 directionToTarget = target.position - transform.position;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, directionToTarget, distanceToTarget, obstacleLayer);

        // Si le raycast ne touche rien, la cible est visible
        isTargetVisible = hit.collider == null;
    }

    private void HandleIdleState()
    {
        // Annuler la v�locit�
        rb.velocity = Vector2.zero;

        float idleWaitTime = distanceToTarget <= closeDistanceThreshold ? idleWaitTimeClose : idleWaitTimeFar;

        StartCoroutine(IdleCoroutine(idleWaitTime));
    }

    private IEnumerator IdleCoroutine(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);

        // Apr�s l'attente, repasser � l'�valuation de l'�tat
        StateManager();
    }

    private void HandleMeleeAttackState()
    {
        // V�rifier si le cooldown est termin� avant de pouvoir attaquer
        StartCoroutine(MeleeAttackCoroutine());
    }

    private IEnumerator MeleeAttackCoroutine()
    {
        // Logique d'attaque de m�l�e
        Debug.Log("Attaque de m�l�e ex�cut�e!");

        // Appliquer les d�g�ts � la cible
        if (distanceToTarget <= meleeRange)
        {
            // Application des d�g�ts au joueur
            IDamageable damageable = target.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage((int)meleeDamage);
            }
        }

        // Attendre avant de pouvoir attaquer � nouveau
        yield return new WaitForSeconds(meleeAttackCooldown);

        // Retourner � l'�tat Idle apr�s l'attaque
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
