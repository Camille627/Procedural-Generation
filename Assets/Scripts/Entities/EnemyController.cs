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
    private float fireCooldown = 0f;   // Temps restant avant le prochain tir
    public float fireCooldownDuration = 0.2f; // dur�e d'une pause apr�s tir
    private int shotsFiredInBurst = 0; // Compte le nombre de tirs dans une rafale (pour semi-automatic)
    private float burstDuration = 2f;  // Dur�e de la rafale pour automatic
    private bool isFiringBurst = false; // Indique si une rafale est en cours

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
        //InitializeEnemy();
    }

    private void Update()
    {
        // Diminue le fireCooldown � chaque frame
        //fireCooldown -= Time.deltaTime;
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
        float elapsedTime = 0;

        // Boucle tant que l'ennemi est dans l'�tat "Aim"
        while (true)
        {
            UpdateInfo();
            elapsedTime += Time.deltaTime;

            // Faire en sorte que l'ennemi vise la position actuelle de la cible
            weaponManager.AimAtPosition(target.position);

            if (elapsedTime >= aimingTime)
            {
                // Si les conditions pour tirer sont remplies, passer � l'�tat "Shoot"
                if (isTargetVisible &&
                    distanceToTarget <= weaponManager.CurrentWeapon().range)
                {
                    currentState = State.Shoot; // Passer � l'�tat "Shoot" apr�s avoir vis� assez longtemps
                    HandleShootState();
                    yield break; // Quitter la coroutine une fois que l'�tat de tir est enclench�
                }
                else
                {
                    // Si les conditions ne sont plus remplies, recalculer l'�tat appropri�
                    StateManager();
                    yield break; // Quitter la coroutine car l'�tat a chang�
                }
            }
            
            yield return null; // Attendre la prochaine frame avant de r��valuer la situation
        }
    }

    /// <summary>
    /// G�re l'�tat de tir o� l'ennemi tire sur la cible.
    /// </summary>
    private void HandleShootState()
    {
        if (fireCooldown <= 0f)
        {
            Debug.Log("tryshoot");
            int isFireDone;
            isFireDone = weaponManager.TryShootingAt(target.position);
            if (isFireDone == 1) { StateManager(); return; }
            WeaponData weapon = weaponManager.CurrentWeapon();

            switch (weapon.reloadType)
            {
                case ReloadType.Manual:
                    // Applique un cooldown apr�s chaque tir
                    fireCooldown = fireCooldownDuration;
                    break;

                case ReloadType.SemiAutomatic:
                    shotsFiredInBurst++;

                    // Si trois tirs ont �t� effectu�s, applique un cooldown
                    if (shotsFiredInBurst >= 3)
                    {
                        shotsFiredInBurst = 0;  // R�initialise le compteur de tirs
                        fireCooldown = fireCooldownDuration;
                    }
                    break;

                case ReloadType.Automatic:
                    // Si la rafale n'est pas d�j� en cours, d�marrez-la
                    if (!isFiringBurst)
                    {
                        StartCoroutine(StartAutomaticFireBurst());
                    }
                    break;
            }
        }

        StateManager();
    }

    private IEnumerator StartAutomaticFireBurst()
    {
        isFiringBurst = true;
        yield return new WaitForSeconds(burstDuration);

        // Applique un cooldown apr�s la rafale
        fireCooldown = fireCooldownDuration;
        isFiringBurst = false;
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
