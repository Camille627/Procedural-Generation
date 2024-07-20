using UnityEngine;

public class Health : MonoBehaviour
{
    public const float maxHealth = 1f;
    
    private float currentHealth = maxHealth;

    /// <summary>
    /// Délégué pour l'événement de destruction d'un gameObject.
    /// </summary>
    /// <param name="enemy">Le GameObject qui a été détruit.</param>
    public delegate void DestroyedDelegate(GameObject enemy);

    /// <summary>
    /// Événement déclenché lorsque le gameObject est détruit.
    /// </summary>
    public event DestroyedDelegate OnDestroyed;

    /// <summary>
    /// Méthode appelée par Unity juste avant que l'objet ne soit détruit.
    /// Déclenche l'événement OnDestroyed pour notifier les abonnés.
    /// </summary>
    void OnDestroy()
    {
        // Vérifie s'il y a des abonnés à l'événement OnDestroyed
        if (OnDestroyed != null)
        {
            // Déclenche l'événement en passant ce GameObject comme paramètre
            OnDestroyed(gameObject);
        }
    }

    //prendre des dégats
    public void TakeDamage(int amount)
    {
        //diminution de vie en fonction des dégats
        currentHealth -= amount;

        //si la vie actuelle atteint 0 le personnage meurt
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Destroy(gameObject);
        }

    }
}
