using UnityEngine;
using System.Collections.Generic;

public class HealthUI : MonoBehaviour
{
    public GameObject heartPrefab; // Prefab de l'ic�ne de c�ur
    private List<GameObject> hearts = new List<GameObject>(); // Liste des c�urs actifs
    private float heartWidth; // Largeur d'un c�ur
    private float spacing; // Espacement entre les c�urs

    private void OnEnable()
    {
        // Calculer la largeur d'un c�ur en utilisant le SpriteRenderer du prefab
        SpriteRenderer heartRenderer = heartPrefab.GetComponent<SpriteRenderer>();
        heartWidth = heartRenderer.bounds.size.x;
        spacing = heartWidth / 4;

        // S'abonner � l'�v�nement de changement de vie
        PlayerHealth.OnHealthChanged += UpdateHearts;
    }

    private void OnDisable()
    {
        // Se d�sabonner de l'�v�nement de changement de vie
        PlayerHealth.OnHealthChanged -= UpdateHearts;
    }

    private void UpdateHearts(int currentHealth)
    {
        // D�truire tous les c�urs actuels
        foreach (GameObject heart in hearts)
        {
            Destroy(heart);
        }
        hearts.Clear();

        // Cr�er de nouveaux c�urs en fonction de la sant� actuelle
        for (int i = 0; i < currentHealth; i++)
        {
            GameObject heart = Instantiate(heartPrefab, transform);
            heart.transform.localPosition = new Vector3(i * (heartWidth + spacing), 0, 0);
            hearts.Add(heart);
        }
    }
}
