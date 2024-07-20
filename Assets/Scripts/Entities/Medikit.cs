using CamBib.Class;
using UnityEngine;

public class Medikit : MonoBehaviour
{
    [SerializeField] private int health;


    private void Start()
    {
        LevelManager.OnLevelEnded += DestroyObject;
    }

    private void DestroyObject() { Destroy(gameObject); }

    private void OnDestroy() { LevelManager.OnLevelEnded -= DestroyObject; }

    // Fonction appel�e lorsque quelque chose entre dans le Collider de la bo�te
    public void OnTriggerEnter2D(Collider2D other)
    {
        // V�rifiez si le joueur entre dans le Collider de la bo�te
        if (other.CompareTag("Player"))
        {
            PlayerHealth playerHealh = other.GetComponent<PlayerHealth>();
            if (playerHealh != null && !playerHealh.IsHealthy())
            {
                // Donne les points de vie au joueur
                playerHealh.Heal(health);
                health = 0;
            }
        }
    }
}

