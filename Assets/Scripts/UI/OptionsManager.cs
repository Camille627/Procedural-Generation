using UnityEngine;

public class OptionsManager : MonoBehaviour
{
    // Singleton instance accessible publiquement mais d�finie en priv�
    public static OptionsManager Instance { get; private set; }

    // D�finition des touches par d�faut pour les diff�rentes actions du jeu
    public static KeyCode exitKey = KeyCode.Escape;
    public static KeyCode upKey = KeyCode.W;
    public static KeyCode downKey = KeyCode.S;
    public static KeyCode leftKey = KeyCode.A;
    public static KeyCode rightKey = KeyCode.D;
    public static KeyCode aimModeKey = KeyCode.LeftShift; // Mode vis�e
    public static KeyCode fireKey = KeyCode.Mouse0; // Tirer
    
    // CheatCodes
    public static KeyCode cheatKey = KeyCode.Backslash; // Touche pour entrer un cheatCode
    public static KeyCode[] regenLevelCode = { KeyCode.G, KeyCode.E, KeyCode.N }; // r�g�n�rer le niveau

    // Sensivity cooldown (temps entre deux activations successives quand n�c�ssaire)
    public static float cooldown = 0.25f;

    private void Awake()
    {
        // Impl�mentation du pattern Singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Ne pas d�truire l'objet lors du chargement d'une nouvelle sc�ne
        }
        else
        {
            Destroy(gameObject); // D�truire l'objet en cas de doublon
        }
    }
}
