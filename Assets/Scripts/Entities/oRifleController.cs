// Author : Camille ANSEL

using UnityEngine;

public class RifleController : MonoBehaviour
{
    public GameObject player;
    public Camera playerCamera;
    public SpriteRenderer srPlayer;
    public SpriteRenderer spriteRenderer;
    public GameObject bulletPrefab;

    private Vector2 bulletDirection;
    public float bulletStart;
    public int bulletSpeed;


    void Update()
    {
        FaceMouse();

        if (Input.GetKeyDown(KeyCode.Space))
        {
            Fire();
        }
    }

    /// <summary>
    /// Oriente dans la direction de la souris
    /// </summary>
    void FaceMouse()
    {
        Vector3 mousePosition = playerCamera.ScreenToWorldPoint(Input.mousePosition);

        Vector2 direction = new Vector2(
            mousePosition.x - transform.position.x, 
            mousePosition.y - transform.position.y);

        transform.right = direction;

        Flip(direction.x, direction.y);
    }

    /// <summary>
    /// Choisit le sens d'affichage de l'arme et du player en fonction de la direction dans laquelle pointe la souris
    /// </summary>
    /// <param name="_x"></param>
    /// <param name="_y"></param>
    void Flip(float _x, float _y)
    {
        if (_x > 0.1f)
        {
            spriteRenderer.flipY = false;
            srPlayer.flipX = false;
        }
        else if (_x < -0.1f)
        {
            spriteRenderer.flipY = true;
            srPlayer.flipX = true;
        }

        //faire passer l'arme derrière le joueur quand il tire vers le haut.

 //       if (_y > 0.1f)
 //
 //     {
 //         spriteRenderer.sortingOrder = 9;
 //     }
 //     else if (_y < -0.1f)
 //     {
 //         spriteRenderer.sortingOrder = 12;
 //     }
    }

    /// <summary>
    /// Tire
    /// </summary>
    void Fire()
    {
        Vector3 mousePosition = playerCamera.ScreenToWorldPoint(Input.mousePosition);

        bulletDirection = new Vector3( mousePosition.x - transform.position.x, mousePosition.y - transform.position.y, 0);
    

        // Création de la balle à partir du prefab "Bullet"
        var bullet = (GameObject)Instantiate(
            bulletPrefab,
            new Vector3(transform.position.x + bulletDirection.normalized.x * bulletStart, transform.position.y + bulletDirection.normalized.y * bulletStart, 0),
            transform.rotation);

        // Ajout de vélocité à la balle
        bullet.GetComponent<Rigidbody2D>().velocity = bulletDirection.normalized * bulletSpeed;

        // Desruction de la balle après 2 secondes
        Destroy(bullet, 5.0f);
    }
}
