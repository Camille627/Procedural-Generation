using UnityEngine;

public class Bullet : MonoBehaviour
{

    public int damages;
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player") == false && collision.gameObject.CompareTag("PlayerBullet") == false)
        {
            // Récupération de l'objet touché
            var hit = collision.gameObject;
            var health = hit.GetComponent<Health>();
            if (health != null)
            {
                //Application des dégats
                health.TakeDamage(damages);
            }


            Destroy(gameObject);
        }



    }
}
