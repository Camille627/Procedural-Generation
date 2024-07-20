using UnityEngine;
using System.Collections.Generic;

public class HealthUI : MonoBehaviour
{
    public GameObject heartPrefab; // Prefab de l'icône de cœur
    private List<GameObject> hearts = new List<GameObject>(); // Liste des cœurs actifs
    private float heartWidth; // Largeur d'un cœur
    private float spacing; // Espacement entre les cœurs

    private void OnEnable()
    {
        // Calculer la largeur d'un cœur en utilisant le SpriteRenderer du prefab
        SpriteRenderer heartRenderer = heartPrefab.GetComponent<SpriteRenderer>();
        heartWidth = heartRenderer.bounds.size.x;
        spacing = heartWidth / 4;

        // S'abonner à l'événement de changement de vie
        PlayerHealth.OnHealthChanged += UpdateHearts;
    }

    private void OnDisable()
    {
        // Se désabonner de l'événement de changement de vie
        PlayerHealth.OnHealthChanged -= UpdateHearts;
    }

    private void UpdateHearts(int currentHealth)
    {
        // Détruire tous les cœurs actuels
        foreach (GameObject heart in hearts)
        {
            Destroy(heart);
        }
        hearts.Clear();

        // Créer de nouveaux cœurs en fonction de la santé actuelle
        for (int i = 0; i < currentHealth; i++)
        {
            GameObject heart = Instantiate(heartPrefab, transform);
            heart.transform.localPosition = new Vector3(i * (heartWidth + spacing), 0, 0);
            hearts.Add(heart);
        }
    }
}
