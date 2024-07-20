using CamBib.Class;
using UnityEngine;

public class WeaponContainer : MonoBehaviour
{
    [SerializeField] private WeaponData contenu;
    private LevelManager manager;
    
    public void SetContenu(WeaponData valeur) { contenu = valeur; }

    private void Start()
    {
        LevelManager.OnLevelEnded += DestroyObject;
        // Set contenu
        LevelManager lM = GameObject.Find("GameManager").GetComponent<LevelManager>();
        SetContenu(new TabRepartition<WeaponData>(lM.weapons).Observation());
    }

    private void DestroyObject() { Destroy(gameObject); }

    private void OnDestroy() { LevelManager.OnLevelEnded -= DestroyObject; }

    // Fonction appelée lorsque quelque chose entre dans le Collider de la boîte
    public void OnTriggerEnter2D(Collider2D other)
    {
        // Vérifiez si le joueur entre dans le Collider de la boîte
        if (other.CompareTag("Player"))
        {
            // Traitez la détection de la boîte d'arme par le joueur
            WeaponManager weaponManager = other.GetComponent<WeaponManager>();
            if (weaponManager != null)
            {
                // Echange l'arme
                WeaponData temp = weaponManager.CurrentWeapon();
                weaponManager.EquipWeapon(contenu);
                contenu = temp;
            }
        }
    }
}
