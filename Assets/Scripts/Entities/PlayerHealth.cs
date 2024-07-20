using UnityEngine;
using UnityEngine.SceneManagement; // Pour recharger la scène en cas de mort du joueur, par exemple

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private int maxHealth; // Vie maximale du joueur
    [SerializeField] private int currentHealth; // Vie actuelle du joueur

    // Evénement pour gérer la mort du joueur
    public delegate void DeathDelegate();
    public event DeathDelegate OnDeath;

    // Evénement pour mettre à jour l'affichage de la vie
    public delegate void HealthChanged(int currentHealth);
    public static event HealthChanged OnHealthChanged;

    // get
    public int CurrentHealth() { return currentHealth; }
    public int MaxHealth() { return maxHealth; }

    // set
    /// <summary>
    /// Initialise la vie actuelle du joueur à son maximum
    /// </summary>
    public void SetCurrentHealth() { currentHealth = maxHealth; OnHealthChanged?.Invoke(currentHealth); }
    /// <summary>
    /// Initialise la vie du joueur à la valeur voulue
    /// </summary>
    /// <param name="value"></param>
    public void SetCurrentHealth(int value )
    { 
        if(value < 0) { throw new System.Exception("La valeur ne peut être négative"); }
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
    /// Recevoir des dégats
    /// </summary>
    /// <param name="amount">Nombre de points de vie à enlever</param>
    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        OnHealthChanged?.Invoke(currentHealth);

        // Vérifie si le joueur est mort
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// Soigner le joueur
    /// </summary>
    /// <param name="amount">Nombre de points de vie à restituer</param>
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
    /// Gérer la mort du joueur
    /// </summary>
    private void Die()
    {
        Debug.Log("Player died!");
        OnDeath?.Invoke();
    }
}
