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
    // Assets à instancier
    [SerializeField] private Tilemap[] tilemaps; // Tableau des Tilemaps utilisées pour le niveau.
    [SerializeField] private TileBase[] tiles; // Tableau des Tiles utilisées pour le niveau.
    [SerializeField] private MaDict<string, GameObject> gameObjects; // Les GameObjects utilisés pour le niveau
    [SerializeField] public WeaponData[] weapons; // Armes disponibles


    public GameObject player; // Référence du joueur
    public Camera playerCamera; // Référence de la camera
    public GameObject enemyPrefab; // Préfabriqué pour les ennemis.

    private int seed = (int)DateTime.Now.Ticks; // Seed pour le jeu
    private Random random;
    private static bool isPlaying = false; // indique si le jeu est en cours
    private static List<GameObject> activeEnemies = new List<GameObject>(); // Liste des ennemis actifs dans le niveau.

    public static event Action OnLevelEnded; // Evénement déclenché lorsque le niveau est terminé

    private void Start()
    {
        random = new Random(seed);
        player = GameObject.FindGameObjectWithTag("Player"); ; // Référence du joueur

        // Initialisation et génération du niveau
        SetLevel();
    }

    /// <summary>
    /// OnApplicationQuit est une méthode appelée par Unity lorsque l'application ou l'éditeur est en train de se fermer.
    /// </summary>
    /// <remarks>
    /// Cette méthode est utile pour effectuer des actions de nettoyage ou de sauvegarde avant que l'application ne soit complètement fermée.
    /// Notez que cette méthode est appelée lorsque l'application est en train de se quitter, ce qui inclut l'arrêt du mode Play dans l'éditeur Unity.
    /// </remarks>
    void OnApplicationQuit()
    {
        isPlaying = false;
    }

    /// <summary>
    /// Méthode appelée lorsqu'un ennemi est détruit.
    /// Met à jour la liste des ennemis actifs et vérifie si le niveau est complété.
    /// </summary>
    /// <param name="enemy">Le GameObject de l'ennemi détruit.</param>
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
    /// Vérifie si le niveau est complété (s'il ne reste plus d'ennemis).
    /// Si oui, régénère un nouveau niveau.
    /// </summary>
    private void CheckLevelCompletion()
    {
        if (activeEnemies.Count == 0)
        {
            LevelCompleted();
        }
    }

    /// <summary>
    /// Action à réaliser lorsque le niveau est complété.
    /// Actuellement, cela génère un nouveau niveau.
    /// </summary>
    private void LevelCompleted()
    {
        Debug.Log("Level Completed!");
        StartCoroutine(EndLevel());
    }

    /// <summary>
    /// Action à réaliser lorsque le joueur à échoué (il n'a plus de vie)
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
    /// Génère le niveau en construisant la Tilemap et en positionnant le joueur et les ennemis.
    /// </summary>
    public void SetLevel()
    {
        Debug.Log(" Generating level...");

        // Génération de la direction moyenne
        DirectionPrecedente generatrice = new DirectionPrecedente(random.Next());
        generatrice.SetDirectionsDiscretes(IntUtils.Directions2D(2)); // adapté pour se déplacer au clavier
        generatrice.SetDeltaIndexMax(2);
        // Génération du graphe de la tranchée
        Graphe<DataSommetStandard, DataArreteStandard> trenchGraph = generatrice.GenerateGraphe();

        // Construction des Tilemaps à partir du graphe
        Utils.GrapheToScene(tilemaps,tiles, gameObjects ,trenchGraph, seed);

        // Joueur
        if (player != null)
        {
            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
            // Réinitialisation de la vie
            playerHealth.SetCurrentHealth();
            // Abonnement à l'évènement de mort
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