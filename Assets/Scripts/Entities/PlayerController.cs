using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [System.Serializable]
    public class PlayerSettings
    {
        public float moveSpeed = 5f;
        public float aimMoveSpeed = 2f; // Vitesse de déplacement en position de viser
    }

    public PlayerSettings settings = new PlayerSettings();
    public SpriteRenderer spriteRenderer;
    public Animator animator;
    public Rigidbody2D rb;

    private Vector2 velocity = Vector2.zero;
    private Vector2 move = Vector2.zero;
    private bool isAiming = false;

    void Update()
    {
        if (!Input.GetKey(OptionsManager.cheatKey)) // évite les interférences avec l'entrée d'un cheatCode
        {
            // handles the movement input
            HandleMovement();
        }
    }

    void FixedUpdate()
    {
        // moves the player
        MovePlayer();
    }

    /// <summary>
    /// Checks for key inputs based on the key bindings in OptionsManager and sets the move variable accordingly.
    /// </summary>
    void HandleMovement()
    {
        move = Vector2.zero; // Reset move vector to ensure new input is captured

        // Check for key inputs and set move value accordingly
        if (Input.GetKey(OptionsManager.upKey))
        {
            move.y = 1;
        }
        else if (Input.GetKey(OptionsManager.downKey))
        {
            move.y = -1;
        }

        if (Input.GetKey(OptionsManager.leftKey))
        {
            move.x = -1;
        }
        else if (Input.GetKey(OptionsManager.rightKey))
        {
            move.x = 1;
        }

        // Normalize the move vector to ensure consistent movement speed in all directions
        move = move.normalized;

        // Check for aiming
        isAiming = Input.GetKey(OptionsManager.aimModeKey);
    }

    void MovePlayer()
    {
        // Movement speed based on aiming state
        float currentMoveSpeed = isAiming ? settings.aimMoveSpeed : settings.moveSpeed;

        // Movement
        Vector2 targetVelocity = move * currentMoveSpeed * Time.fixedDeltaTime;
        rb.velocity = Vector2.SmoothDamp(rb.velocity, targetVelocity, ref velocity, 0.05f);

        // Animator (uncomment if you have animations set up)
        // float characterVelocity = Mathf.Abs(Mathf.Sqrt(rb.velocity.x * rb.velocity.x + rb.velocity.y * rb.velocity.y));
        // animator.SetFloat("Speed", characterVelocity);
    }

    public bool IsAiming()
    {
        return isAiming;
    }
}
