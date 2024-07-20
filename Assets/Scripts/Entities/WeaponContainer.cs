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

    // Fonction appel�e lorsque quelque chose entre dans le Collider de la bo�te
    public void OnTriggerEnter2D(Collider2D other)
    {
        // V�rifiez si le joueur entre dans le Collider de la bo�te
        if (other.CompareTag("Player"))
        {
            // Traitez la d�tection de la bo�te d'arme par le joueur
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
