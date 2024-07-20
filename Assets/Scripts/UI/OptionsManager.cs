using UnityEngine;

public class OptionsManager : MonoBehaviour
{
    // Singleton instance accessible publiquement mais définie en privé
    public static OptionsManager Instance { get; private set; }

    // Définition des touches par défaut pour les différentes actions du jeu
    public static KeyCode exitKey = KeyCode.Escape;
    public static KeyCode upKey = KeyCode.W;
    public static KeyCode downKey = KeyCode.S;
    public static KeyCode leftKey = KeyCode.A;
    public static KeyCode rightKey = KeyCode.D;
    public static KeyCode aimModeKey = KeyCode.LeftShift; // Mode visée
    public static KeyCode fireKey = KeyCode.Mouse0; // Tirer
    
    // CheatCodes
    public static KeyCode cheatKey = KeyCode.Backslash; // Touche pour entrer un cheatCode
    public static KeyCode[] regenLevelCode = { KeyCode.G, KeyCode.E, KeyCode.N }; // régénérer le niveau

    // Sensivity cooldown (temps entre deux activations successives quand nécéssaire)
    public static float cooldown = 0.25f;

    private void Awake()
    {
        // Implémentation du pattern Singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Ne pas détruire l'objet lors du chargement d'une nouvelle scène
        }
        else
        {
            Destroy(gameObject); // Détruire l'objet en cas de doublon
        }
    }
}
