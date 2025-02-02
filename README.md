# 🎯 [Nom du Projet] - Jeu de Tir 2D avec Génération Procédurale

🚀 Un jeu de tir en 2D où les niveaux sont **générés de manière procédurale** en utilisant la **théorie des graphes**, des **oscillations sinusoïdales** pour les couloirs, et une **répartition intelligente des entités**.  
L'objectif est de créer des environnements variés et immersifs, inspirés de **techniques issues de la synthèse musicale**.  

---

## 🌍 Génération Procédurale - Principe

Notre système de génération de niveaux repose sur **trois étapes clés** :

### **1️⃣ Génération d'un graphe**
🔹 Un **graphe** est d'abord créé pour structurer les connexions entre différentes zones du niveau.  
🔹 **Avantage :** En utilisant la **théorie des graphes**, on garantit que le joueur **n'a pas à repasser deux fois par le même couloir** pour explorer toute la carte.  
🔹 **Actuellement, notre implémentation génère un graphe linéaire**.

📌 **Prochaine amélioration :** Générer des graphes plus novateurs avec des **cycles, des structures semi-eulériennes et eulériennes**, pour offrir plus de variété et de choix au joueur.  

### **2️⃣ Tracé aléatoire des couloirs avec oscillations sinusoïdales**
Nous avons travaillé sur un tracé **plus naturel et fluide des couloirs**, en nous inspirant de techniques issues de la **synthèse sonore en musique** :

📌 **Prochaine amélioration :** Générer des **salles plus complexes** (circulaires, rectangulaires, formes plus aléatoires) pour diversifier les environnements.  

### **3️⃣ Placement des entités dans le niveau**
Une fois la structure du niveau définie, nous plaçons dynamiquement les **éléments interactifs** :

📌 **Prochaine amélioration :** Positionner les **entités et bonus de manière plus réaliste**, en tenant compte de la disposition des salles et du gameplay.  

---

## 🕹️ Essayer le jeu sans installer Unity

🎮 Vous pouvez tester le jeu sans avoir besoin d'installer Unity !  

### **1️⃣ Télécharger la version compilée**
- Le dossier `build/` contient la version compilée du jeu (`a3112b4`).  

### **2️⃣ Lancer le jeu**
- Ouvrir le fichier **`My project.exe`**.  
- Le jeu démarre immédiatement, sans installation requise !  

---

## 🕹️ Résumé des commandes du jeu

| Action          | Touche par défaut |
|----------------|-----------------|
| **Se déplacer** | `Z / Q / S / D` (ou `W / A / S / D` en QWERTY) |
| **Viser**      | `Shift gauche` |
| **Tirer**      | `Clic gauche` |
| **Changer d'arme** | `E` |
| **Ramasser un objet** | `F` |
| **Recharger**  | `R` |
| **Pause/Menu** | `Échap` |
| **Activer un cheat code** | `\` |
| **Régénérer le niveau** | `G + E + N` |

---

## ❓ Problèmes connus

❌ **Le niveau ne se recharge pas dans le build final ?**  
**Problème :** Lorsqu'on tue le dernier ennemi, le niveau ne se recharge pas.  
**Cause possible :** La coroutine `EndLevel()` est peut-être interrompue ou `SetLevel()` ne se déclenche pas correctement.  

### **✅ Solution temporaire**
Ajoutez un **rechargement manuel de la scène** dans `EndLevel()` :

```csharp
using UnityEngine.SceneManagement;

public void EndLevel()
{
    Debug.Log("EndLevel() started.");
    OnLevelEnded?.Invoke();

    if (player != null)
    {
        player.GetComponent<PlayerHealth>().OnDeath -= OnPlayerDeath;
    }

    isPlaying = false;
    TilemapUtils.ClearTilemaps(tilemaps);
    activeEnemies.Clear();

    Debug.Log("Reloading scene...");
    SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
}
