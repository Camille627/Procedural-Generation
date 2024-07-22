using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using CamBib.Class;
using CamBib.Fonctions;
using CamBibUnity;
using SommetArrete;

using Random = System.Random;


public class LevelManager : MonoBehaviour
{
    // Assets � instancier
    [SerializeField] private Tilemap[] tilemaps; // Tableau des Tilemaps utilis�es pour le niveau.
    [SerializeField] private TileBase[] tiles; // Tableau des Tiles utilis�es pour le niveau.
    [SerializeField] private MaDict<string, GameObject> gameObjects; // Les GameObjects utilis�s pour le niveau
    [SerializeField] public WeaponData[] weapons; // Armes disponibles


    public GameObject player; // R�f�rence du joueur
    public Camera playerCamera; // R�f�rence de la camera
    public GameObject enemyPrefab; // Pr�fabriqu� pour les ennemis.

    private int seed = (int)DateTime.Now.Ticks; // Seed pour le jeu
    private Random random;
    private static bool isPlaying = false; // indique si le jeu est en cours
    private static List<GameObject> activeEnemies = new List<GameObject>(); // Liste des ennemis actifs dans le niveau.

    public static event Action OnLevelEnded; // Ev�nement d�clench� lorsque le niveau est termin�

    private void Start()
    {
        random = new Random(seed);
        player = GameObject.FindGameObjectWithTag("Player"); ; // R�f�rence du joueur

        // Initialisation et g�n�ration du niveau
        SetLevel();
    }

    /// <summary>
    /// OnApplicationQuit est une m�thode appel�e par Unity lorsque l'application ou l'�diteur est en train de se fermer.
    /// </summary>
    /// <remarks>
    /// Cette m�thode est utile pour effectuer des actions de nettoyage ou de sauvegarde avant que l'application ne soit compl�tement ferm�e.
    /// Notez que cette m�thode est appel�e lorsque l'application est en train de se quitter, ce qui inclut l'arr�t du mode Play dans l'�diteur Unity.
    /// </remarks>
    void OnApplicationQuit()
    {
        isPlaying = false;
    }

    /// <summary>
    /// M�thode appel�e lorsqu'un ennemi est d�truit.
    /// Met � jour la liste des ennemis actifs et v�rifie si le niveau est compl�t�.
    /// </summary>
    /// <param name="enemy">Le GameObject de l'ennemi d�truit.</param>
    private void OnEnemyDestroyed(GameObject enemy)
    {
        if (!isPlaying)
        {
            return;
        }

        activeEnemies.Remove(enemy);
        CheckLevelCompletion();
    }

    private void OnPlayerDeath()
    {
        LevelFailed();
    }

    //// Fin du niveau

    /// <summary>
    /// V�rifie si le niveau est compl�t� (s'il ne reste plus d'ennemis).
    /// Si oui, r�g�n�re un nouveau niveau.
    /// </summary>
    private void CheckLevelCompletion()
    {
        if (activeEnemies.Count == 0)
        {
            LevelCompleted();
        }
    }

    /// <summary>
    /// Action � r�aliser lorsque le niveau est compl�t�.
    /// Actuellement, cela g�n�re un nouveau niveau.
    /// </summary>
    private void LevelCompleted()
    {
        Debug.Log("Level Completed!");
        StartCoroutine(EndLevel());
    }

    /// <summary>
    /// Action � r�aliser lorsque le joueur � �chou� (il n'a plus de vie)
    /// </summary>
    private void LevelFailed()
    {
        Debug.Log("Level Failed!");
        StartCoroutine(EndLevel());
    }

    /// <summary>
    /// Supprime ce qu'il reste du niveau et lance la suite
    /// </summary>
    public IEnumerator EndLevel()
    {
        OnLevelEnded?.Invoke();
        // Desabonnement
        player.GetComponent<PlayerHealth>().OnDeath -= OnPlayerDeath;
        // Maj indicateur
        isPlaying = false;
        // Vide la scene
        TilemapUtils.ClearTilemaps(tilemaps);
        activeEnemies.Clear();

        yield return null;

        // Met en place un nouveau niveau
        SetLevel();
    }


    //// Generation du niveau

    /// <summary>
    /// G�n�re le niveau en construisant la Tilemap et en positionnant le joueur et les ennemis.
    /// </summary>
    public void SetLevel()
    {
        Debug.Log(" Generating level...");

        // G�n�ration de la direction moyenne
        DirectionPrecedente generatrice = new DirectionPrecedente(random.Next());
        generatrice.SetDirectionsDiscretes(IntUtils.Directions2D(2)); // adapt� pour se d�placer au clavier
        generatrice.SetDeltaIndexMax(2);
        // G�n�ration du graphe de la tranch�e
        Graphe<DataSommetStandard, DataArreteStandard> trenchGraph = generatrice.GenerateGraphe();

        // Construction des Tilemaps � partir du graphe
        Utils.GrapheToScene(tilemaps,tiles, gameObjects ,trenchGraph, seed);

        // Joueur
        if (player != null)
        {
            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
            // R�initialisation de la vie
            playerHealth.SetCurrentHealth();
            // Abonnement � l'�v�nement de mort
            playerHealth.OnDeath += OnPlayerDeath;
            // Placement de la Camera
            playerCamera.GetComponent<CameraController>().FocusOnPlayer();
        }
        else
        {
            Debug.LogError("Player not found! Make sure the player GameObject has the tag 'Player'.");
        }

        // Ennemis
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemy in enemies)
        {
            activeEnemies.Add(enemy);
            enemy.GetComponent<Health>().OnDestroyed += OnEnemyDestroyed;
        }

        // On lance le jeu
        isPlaying = true;
    }
}