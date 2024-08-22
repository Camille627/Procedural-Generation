using System.Collections;
using UnityEngine;

/// <summary>
/// Contrôle le comportement d'un ennemi dans le jeu, y compris la gestion des états, la prise de dégâts, 
/// et l'utilisation d'une arme pour attaquer une cible.
/// </summary>
public class EnemyController : MonoBehaviour, IDamageable
{
    // --- Variables de base ---

    public float maxHealth = 2f; // Vie maximale de l'ennemi
    private float currentHealth; // Vie actuelle de l'ennemi

    public float meleeDamage = 1f; // Dégâts de l'attaque de mêlée
    public float meleeRange = 1.5f; // Portée de l'attaque de mêlée
    public float meleeAttackCooldown = 1f; // Temps d'attente entre les attaques de mêlée

    public float chargeSpeed = 5f; // Vitesse de la charge
    public float chargeRange = 10f; // Portée minimale pour commencer à charger

    public float aimingTime = 1f; // Temps de visée
    public float shootCooldown = 0.2f; // Temps de pause après l'action du tir

    public Transform target; // Cible de l'ennemi (le joueur)
    public LayerMask obstacleLayer; // Masque de couche pour détecter les obstacles

    private float distanceToTarget; // Distance actuelle à la cible
    private bool isTargetVisible; // Indique si la cible est visible

    private Rigidbody2D rb; // Référence au Rigidbody2D de l'ennemi

    public float refreshTime = 0.5f; // Temps de rafraîchissement des informations
    public float idleWaitTimeFar = 2f; // Temps d'attente en état Idle si la cible est loin
    public float closeDistanceThreshold = 15f; // Distance considérée comme "proche" pour l'état Idle

    private EnemyWeaponManager weaponManager; // Gestionnaire d'arme de l'ennemi

    public delegate void GameObjectDelegate(GameObject valeur);
    public event GameObjectDelegate OnKilled; // Événement déclenché lorsque l'ennemi est tué

    // --- Enumération des états possibles ---
    private enum State { Idle, Charge, MeleeAttack, Aim, Shoot }
    private State currentState = State.Idle;

    // --- Méthodes Unity ---

    private void Start()
    {
        InitializeEnemy();
    }

    /// <summary>
    /// Initialise les variables et les composants de l'ennemi.
    /// </summary>
    private void InitializeEnemy()
    {
        currentHealth = maxHealth;
        target = GameObject.Find("Player").transform;
        rb = GetComponent<Rigidbody2D>();

        weaponManager = GetComponent<EnemyWeaponManager>();

        LevelManager.OnLevelEnded += DestroyGameObject;

        StateManager();
    }

    private void OnDestroy()
    {
        LevelManager.OnLevelEnded -= DestroyGameObject;
    }

    // --- Gestion des états ---

    /// <summary>
    /// Gère les transitions et les actions en fonction de l'état actuel de l'ennemi.
    /// </summary>
    private void StateManager()
    {
        UpdateInfo();

        if (distanceToTarget <= meleeRange && isTargetVisible)
        {
            currentState = State.MeleeAttack;
        }
        else if (distanceToTarget <= chargeRange && isTargetVisible)
        {
            currentState = State.Charge;
        }
        else if (distanceToTarget <= weaponManager.CurrentWeapon().range && isTargetVisible)
        {
            currentState = State.Aim;
        }
        else
        {
            currentState = State.Idle;
        }

        switch (currentState)
        {
            case State.Idle:
                HandleIdleState();
                break;

            case State.Charge:
                HandleChargeState();
                break;

            case State.MeleeAttack:
                HandleMeleeAttackState();
                break;

            case State.Aim:
                HandleAimState();
                break;

            case State.Shoot:
                HandleShootState();
                break;
        }
    }

    /// <summary>
    /// Met à jour les informations sur la distance à la cible et la visibilité.
    /// </summary>
    private void UpdateInfo()
    {
        distanceToTarget = Vector2.Distance(transform.position, target.position);

        Vector2 directionToTarget = target.position - transform.position;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, directionToTarget, distanceToTarget, obstacleLayer);

        isTargetVisible = hit.collider == null;

        Debug.DrawRay(transform.position, directionToTarget.normalized * distanceToTarget, Color.red);
    }

    // --- États individuels ---

    /// <summary>
    /// Gère l'état Idle où l'ennemi reste immobile et attend.
    /// </summary>
    private void HandleIdleState()
    {
        rb.velocity = Vector2.zero;

        float idleWaitTime = distanceToTarget <= closeDistanceThreshold ? refreshTime : idleWaitTimeFar;

        StartCoroutine(IdleCoroutine(idleWaitTime));
    }

    private IEnumerator IdleCoroutine(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);

        StateManager();
    }

    /// <summary>
    /// Gère l'état Charge où l'ennemi se déplace rapidement vers sa cible.
    /// </summary>
    private void HandleChargeState()
    {
        StartCoroutine(ChargeCoroutine());
    }

    private IEnumerator ChargeCoroutine()
    {
        while (currentState == State.Charge)
        {
            UpdateInfo();

            if (distanceToTarget > chargeRange || !isTargetVisible)
            {
                currentState = State.Idle;
                StateManager();
                yield break;
            }

            if (distanceToTarget <= meleeRange)
            {
                currentState = State.MeleeAttack;
                StateManager();
                yield break;
            }

            Vector2 directionToTarget = (target.position - transform.position).normalized;
            rb.velocity = directionToTarget * chargeSpeed;

            yield return new WaitForSeconds(refreshTime);
        }
    }

    /// <summary>
    /// Gère l'état de mêlée où l'ennemi attaque sa cible en corps à corps.
    /// </summary>
    private void HandleMeleeAttackState()
    {
        StartCoroutine(MeleeAttackCoroutine());
    }

    private IEnumerator MeleeAttackCoroutine()
    {
        Debug.Log("Attaque de mêlée exécutée!");

        if (distanceToTarget <= meleeRange)
        {
            IDamageable damageable = target.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage((int)meleeDamage);
            }
        }

        yield return new WaitForSeconds(meleeAttackCooldown);

        currentState = State.Idle;
        StateManager();
    }

    /// <summary>
    /// Gère l'état Aim où l'ennemi se prépare à tirer sur la cible.
    /// </summary>
    private void HandleAimState()
    {
        rb.velocity = Vector2.zero; // L'ennemi s'arrête pour viser
        StartCoroutine(AimCoroutine());
    }

    private IEnumerator AimCoroutine()
    {
        float elapsedTime = 0f;

        // Boucle tant que l'ennemi est dans l'état "Aim"
        while (currentState == State.Aim)
        {
            UpdateInfo();

            // Faire en sorte que l'ennemi vise la position actuelle de la cible
            weaponManager.AimAtPosition(target.position);

            // Si les conditions pour tirer sont remplies, passer à l'état "Shoot"
            if (isTargetVisible && distanceToTarget <= weaponManager.CurrentWeapon().range)
            {
                elapsedTime += Time.deltaTime;
                if (elapsedTime >= aimingTime)
                {
                    currentState = State.Shoot; // Passer à l'état "Shoot" après avoir visé assez longtemps
                    HandleShootState();
                    yield break; // Quitter la coroutine une fois que l'état de tir est enclenché
                }
            }
            else
            {
                // Si les conditions ne sont plus remplies, recalculer l'état approprié
                StateManager();
                yield break; // Quitter la coroutine car l'état a changé
            }

            yield return null; // Attendre la prochaine frame avant de réévaluer la situation
        }
    }

    /// <summary>
    /// Gère l'état de tir où l'ennemi tire sur la cible.
    /// </summary>
    private void HandleShootState()
    {
        StartCoroutine(ShootCoroutine());
    }

    private IEnumerator ShootCoroutine()
    {
        weaponManager.HandleShooting(target.position);

        yield return new WaitForSeconds(shootCooldown); // L'ennemi marque une pause après le tir

        currentState = State.Idle; // Retour à l'état Idle après le tir
        StateManager();
    }

    // --- Gestion des dégâts ---

    /// <summary>
    /// Applique des dégâts à l'ennemi et gère sa mort si les points de vie atteignent zéro.
    /// </summary>
    /// <param name="amount">Quantité de dégâts à infliger.</param>
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

    /// <summary>
    /// Détruit le GameObject de l'ennemi.
    /// </summary>
    private void DestroyGameObject()
    {
        Destroy(gameObject);
    }
}
