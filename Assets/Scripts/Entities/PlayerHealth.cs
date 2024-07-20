using UnityEngine;
using UnityEngine.SceneManagement; // Pour recharger la sc�ne en cas de mort du joueur, par exemple

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private int maxHealth; // Vie maximale du joueur
    [SerializeField] private int currentHealth; // Vie actuelle du joueur

    // Ev�nement pour g�rer la mort du joueur
    public delegate void DeathDelegate();
    public event DeathDelegate OnDeath;

    // Ev�nement pour mettre � jour l'affichage de la vie
    public delegate void HealthChanged(int currentHealth);
    public static event HealthChanged OnHealthChanged;

    // get
    public int CurrentHealth() { return currentHealth; }
    public int MaxHealth() { return maxHealth; }

    // set
    /// <summary>
    /// Initialise la vie actuelle du joueur � son maximum
    /// </summary>
    public void SetCurrentHealth() { currentHealth = maxHealth; OnHealthChanged?.Invoke(currentHealth); }
    /// <summary>
    /// Initialise la vie du joueur � la valeur voulue
    /// </summary>
    /// <param name="value"></param>
    public void SetCurrentHealth(int value )
    { 
        if(value < 0) { throw new System.Exception("La valeur ne peut �tre n�gative"); }
        currentHealth = value > maxHealth ? maxHealth : value;
        OnHealthChanged?.Invoke(currentHealth); 
    }
    public void SetMaxHealth(int value) { maxHealth = value; }


    private void Start()
    {
        // Initialisation de la vie du joueur
        SetCurrentHealth();
    }

    public bool IsHealthy()
    {
        return currentHealth < maxHealth ? false : true;
    }

    /// <summary>
    /// Recevoir des d�gats
    /// </summary>
    /// <param name="amount">Nombre de points de vie � enlever</param>
    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        OnHealthChanged?.Invoke(currentHealth);

        // V�rifie si le joueur est mort
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// Soigner le joueur
    /// </summary>
    /// <param name="amount">Nombre de points de vie � restituer</param>
    public void Heal(int amount)
    {
        currentHealth += amount;
        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }
        OnHealthChanged?.Invoke(currentHealth);
    }

    /// <summary>
    /// G�rer la mort du joueur
    /// </summary>
    private void Die()
    {
        Debug.Log("Player died!");
        OnDeath?.Invoke();
    }
}
