using UnityEngine;

public class CameraController : MonoBehaviour
{
    [System.Serializable]
    public class CameraSettings
    {
        public float normalTimeOffset = 0.5f;     // Temps de décalage pour le lissage de la position en mode normal
        public float aimTimeOffset = 0.1f;        // Temps de décalage pour le lissage de la position en mode visée
        public float normalMouseInfluence = 0.1f; // Influence de la position de la souris sur la caméra en mode normal
        public float aimMouseInfluence = 0.3f;    // Influence de la position de la souris sur la caméra en mode visée
        public Vector3 posOffset = new Vector3(0, 0, -10); // Décalage de position de la caméra
    }

    public GameObject player; // Référence au joueur
    public CameraSettings settings = new CameraSettings();

    private Vector3 velocity = Vector3.zero; // Vitesse actuelle du lissage
    private Camera controlledCamera; // Référence à la caméra
    private PlayerController playerController; // Référence au script qui gère le joueur

    void Start()
    {
        // Stocker la référence à la caméra
        controlledCamera = gameObject.GetComponent<Camera>(); 

        // Initialisation du GameObjet "Player"
        if (player != null)
        {
            Vector3 start = player.transform.position + settings.posOffset;
            transform.position = start;
            playerController = player.GetComponent<PlayerController>();
        }
    }

    void Update()
    {
        if (player != null)
        {
            // Déterminer les paramètres en fonction de l'état de visée
            float currentMouseInfluence = playerController.IsAiming() ? settings.aimMouseInfluence : settings.normalMouseInfluence;
            float currentTimeOffset = playerController.IsAiming() ? settings.aimTimeOffset : settings.normalTimeOffset;

            // Suis la position du joueur
            Vector3 targetPosition = player.transform.position + settings.posOffset;

            // Ajout de l'influence de la position de la souris
            Vector3 mousePosition = controlledCamera.ScreenToWorldPoint(Input.mousePosition);
            mousePosition.z = 0; // Assurer que le déplacement ne se fasse que sur le plan X-Y
            Vector3 directionToMouse = (mousePosition - player.transform.position) * currentMouseInfluence;

            targetPosition += directionToMouse;

            // Lissage de la position de la caméra
            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, currentTimeOffset);
        }
    }

    public void SetPlayer(GameObject player)
    {
        this.player = player;
        // récupère le playerController
        playerController = player.GetComponent<PlayerController>();
    }

    public void GoToPlayer()
    {
        transform.position = player.transform.position + settings.posOffset;
    }

    public void FocusOnPlayer()
    {
        // position du joueur
        Vector3 targetPosition = player.transform.position + settings.posOffset;

        // Lissage de la position de la caméra
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, 0.1f);
    }
}
