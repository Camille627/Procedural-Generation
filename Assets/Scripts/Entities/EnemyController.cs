using System.Collections;
using UnityEngine;

/// <summary>
/// Contr�le le comportement d'un ennemi dans le jeu, y compris la gestion des �tats, la prise de d�g�ts, 
/// et l'utilisation d'une arme pour attaquer une cible.
/// </summary>
public class EnemyController : MonoBehaviour, IDamageable
{
    // --- Variables de base ---

    public float maxHealth = 2f; // Vie maximale de l'ennemi
    private float currentHealth; // Vie actuelle de l'ennemi

    public float meleeDamage = 1f; // D�g�ts de l'attaque de m�l�e
    public float meleeRange = 1.5f; // Port�e de l'attaque de m�l�e
    public float meleeAttackCooldown = 1f; // Temps d'attente entre les attaques de m�l�e

    public float chargeSpeed = 5f; // Vitesse de la charge
    public float chargeRange = 10f; // Port�e minimale pour commencer � charger

    public float aimingTime = 1f; // Temps de vis�e
    public float shootCooldown = 0.2f; // Temps de pause apr�s l'action du tir

    public Transform target; // Cible de l'ennemi (le joueur)
    public LayerMask obstacleLayer; // Masque de couche pour d�tecter les obstacles

    private float distanceToTarget; // Distance actuelle � la cible
    private bool isTargetVisible; // Indique si la cible est visible

    private Rigidbody2D rb; // R�f�rence au Rigidbody2D de l'ennemi

    public float refreshTime = 0.5f; // Temps de rafra�chissement des informations
    public float idleWaitTimeFar = 2f; // Temps d'attente en �tat Idle si la cible est loin
    public float closeDistanceThreshold = 15f; // Distance consid�r�e comme "proche" pour l'�tat Idle

    private EnemyWeaponManager weaponManager; // Gestionnaire d'arme de l'ennemi

    public delegate void GameObjectDelegate(GameObject valeur);
    public event GameObjectDelegate OnKilled; // �v�nement d�clench� lorsque l'ennemi est tu�

    // --- Enum�ration des �tats possibles ---
    private enum State { Idle, Charge, MeleeAttack, Aim, Shoot }
    private State currentState = State.Idle;

    // --- M�thodes Unity ---

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

    // --- Gestion des �tats ---

    /// <summary>
    /// G�re les transitions et les actions en fonction de l'�tat actuel de l'ennemi.
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
    /// Met � jour les informations sur la distance � la cible et la visibilit�.
    /// </summary>
    private void UpdateInfo()
    {
        distanceToTarget = Vector2.Distance(transform.position, target.position);

        Vector2 directionToTarget = target.position - transform.position;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, directionToTarget, distanceToTarget, obstacleLayer);

        isTargetVisible = hit.collider == null;

        Debug.DrawRay(transform.position, directionToTarget.normalized * distanceToTarget, Color.red);
    }

    // --- �tats individuels ---

    /// <summary>
    /// G�re l'�tat Idle o� l'ennemi reste immobile et attend.
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
    /// G�re l'�tat Charge o� l'ennemi se d�place rapidement vers sa cible.
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
    /// G�re l'�tat de m�l�e o� l'ennemi attaque sa cible en corps � corps.
    /// </summary>
    private void HandleMeleeAttackState()
    {
        StartCoroutine(MeleeAttackCoroutine());
    }

    private IEnumerator MeleeAttackCoroutine()
    {
        Debug.Log("Attaque de m�l�e ex�cut�e!");

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
    /// G�re l'�tat Aim o� l'ennemi se pr�pare � tirer sur la cible.
    /// </summary>
    private void HandleAimState()
    {
        rb.velocity = Vector2.zero; // L'ennemi s'arr�te pour viser
        StartCoroutine(AimCoroutine());
    }

    private IEnumerator AimCoroutine()
    {
        float elapsedTime = 0f;

        // Boucle tant que l'ennemi est dans l'�tat "Aim"
        while (currentState == State.Aim)
        {
            UpdateInfo();

            // Faire en sorte que l'ennemi vise la position actuelle de la cible
            weaponManager.AimAtPosition(target.position);

            // Si les conditions pour tirer sont remplies, passer � l'�tat "Shoot"
            if (isTargetVisible && distanceToTarget <= weaponManager.CurrentWeapon().range)
            {
                elapsedTime += Time.deltaTime;
                if (elapsedTime >= aimingTime)
                {
                    currentState = State.Shoot; // Passer � l'�tat "Shoot" apr�s avoir vis� assez longtemps
                    HandleShootState();
                    yield break; // Quitter la coroutine une fois que l'�tat de tir est enclench�
                }
            }
            else
            {
                // Si les conditions ne sont plus remplies, recalculer l'�tat appropri�
                StateManager();
                yield break; // Quitter la coroutine car l'�tat a chang�
            }

            yield return null; // Attendre la prochaine frame avant de r��valuer la situation
        }
    }

    /// <summary>
    /// G�re l'�tat de tir o� l'ennemi tire sur la cible.
    /// </summary>
    private void HandleShootState()
    {
        StartCoroutine(ShootCoroutine());
    }

    private IEnumerator ShootCoroutine()
    {
        weaponManager.HandleShooting(target.position);

        yield return new WaitForSeconds(shootCooldown); // L'ennemi marque une pause apr�s le tir

        currentState = State.Idle; // Retour � l'�tat Idle apr�s le tir
        StateManager();
    }

    // --- Gestion des d�g�ts ---

    /// <summary>
    /// Applique des d�g�ts � l'ennemi et g�re sa mort si les points de vie atteignent z�ro.
    /// </summary>
    /// <param name="amount">Quantit� de d�g�ts � infliger.</param>
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
    /// D�truit le GameObject de l'ennemi.
    /// </summary>
    private void DestroyGameObject()
    {
        Destroy(gameObject);
    }
}
