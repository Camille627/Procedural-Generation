using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRenderer : MonoBehaviour
{
    [SerializeField] private Camera playerCamera;
    private SpriteRenderer srPlayer;

    public Camera PlayerCamera() { return playerCamera; }

    private void Start()
    {
        srPlayer = GetComponent<SpriteRenderer>();
        // Camera
        playerCamera = Camera.main;
        playerCamera.GetComponent<CameraController>().SetPlayer(gameObject);
        playerCamera.GetComponent<CameraController>().FocusOnPlayer();
    }

    private void Update()
    {
        FaceMouse();
    }

    /// <summary>
    /// Oriente dans la direction de la souris
    /// </summary>
    private void FaceMouse()
    {
        Vector3 direction = playerCamera.ScreenToWorldPoint(Input.mousePosition) - transform.position;
        Flip(direction);
    }

    /// <summary>
    /// Choisit le sens d'affichage de l'arme et du player en fonction de la direction dans laquelle pointe la souris
    /// </summary>
    /// <param name="direction">Vecteur 2D de la direction dans laquelle se diriger</param>
    private void Flip(Vector3 direction)
    {
        if (direction.x > 0.1f)
        {
            srPlayer.flipX = false;
        }
        else if (direction.y < -0.1f)
        {
            srPlayer.flipX = true;
        }
    }
}
