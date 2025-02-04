# 🎯 Roguelike avec Génération Procédurale

🚀 Un jeu de tir en 2D où les niveaux sont **générés de manière procédurale** en utilisant la **théorie des graphes**.

---

## 🌍 Génération Procédurale - Principe

Notre système de génération de niveaux repose sur **trois étapes** :

### **1️⃣ Génération d'un graphe**
Un **graphe** est d'abord créé pour structurer le niveau. En utilisant la **théorie des graphes**, on garantit que le joueur **n'a pas à repasser deux fois par le même couloir** pour explorer toute la carte. Pour cela, le graphe doit être *eulérien* et *planaire* (sinon les arrêtes se croisent).

🔹 Actuellement, notre implémentation génère un graphe *linéaire*.


📌 **Prochaine amélioration :** Générer des graphes plus novateurs : des *cycles*, des graphes *semi-eulériens* et *eulériens* (non linéaires)

### **2️⃣ Tracé des couloirs et des salles**
On remplace ensuite chaque arrête du graphe par un **couloir** et chaque noeud par une **salle**.
Nous avons travaillé sur un tracé plus naturel et fluide des couloirs, en nous inspirant de la **synthèse additive** :

 🔹 On modélise une droite entre les **salles** à lier puis on lui ajoute une sucession de perturbations dont l'amplitude maximale diminue à chaque itération.
 
 🔹 Les perturbations sont des courbes sinusoïdales de fréquent divisée par 2 à chaque itération. 
 
 🔹 Attribue à chaque perturbation un modificateur d'amplitude aléatoire dans l'intervalle [1;-1] qui est facteur de l'amplitude maximale de la perturbation. C'est cela qui rend aléatoire le tracé.

📌 **Prochaine amélioration :** Générer des **salles plus complexes** (circulaires, rectangulaires, formes plus aléatoires) pour diversifier les environnements.  

### **3️⃣ Placement des entités dans le niveau**
Une fois la structure du niveau définie, nous ajoutons dans les **salles** les **entitées** :

📌 **Prochaine amélioration :** Positionner les **entités et bonus de manière plus réaliste**, en tenant compte de la disposition des salles.  

---

## 🧰 Mechaniques du jeu
### Les armes
Les armes ont 5 caractéristiques : cadence, portée, mode de tir (auto ou semi-auto), vitesse du projectile et dégats.
Le **Revolver** est l'arme, par défault, équipée sur le joueur. La **Mitraillette**, le **Fusil semi-automatique** et le **Fusil à verrou** peuvent être récupérés dans une **caisse d'arme**

### Les caisses
Lorsque vous marchez sur une **caisse d'arme**, vous échangez votre arme avec celle qu'elle contient. La **caisse de soin** vous restaurera 1 **point de vie** si vous en avez besoin.
Il y a une caisse de chaque exemplaire par niveau .

### Victoire 🏁
Vous avez terminé un niveau lorsque tous les ennemis ont été tués. Un nouveau niveau se recharge alors.

---

## 🕹️ Essayer le jeu sans installer Unity

Vous pouvez tester le jeu sans avoir besoin d'installer Unity !  

### **1️⃣ Télécharger la version compilée**
- Le dossier `build/` contient la version compilée du jeu (`a3112b4`).  

### **2️⃣ Lancer le jeu**
- Ouvrir le fichier **`My project.exe`**.  
- Le jeu démarre immédiatement, sans installation requise !  

---

## 🎮 Résumé des commandes du jeu

| Action          | Touche par défaut |
|----------------|-----------------|
| **Se déplacer** | `Z / Q / S / D` |
| **Viser**      | `Shift gauche` |
| **Tirer**      | `Clic gauche` |
| **Quitter** | `Échap` |
| **Activer un cheat code** | `\` |
| **Regénérer le niveau** | `G + E + N` |

Pour utiliser un cheat code, il faut presser la touche **Activer un cheat code** en plus de celles du cheat code.
Je n'ai pas réussi à regénérer le niveau lors de mes dernier tests (dans l'éditeur comme dans la version compilée)

---

## Problèmes connus

❌ **Le niveau ne se recharge pas dans le build final ?**  
**Problème :** Lorsqu'on tue le dernier ennemi, le niveau ne se recharge pas. 


---

# 📫 Suggestions et signalements de bugs
Si vous avez des idées d'amélioration ou que vous trouvez un bug n'hésitez pas à m'en faire part 😉.

# Licence
Ce projet est sous licence MIT. Vous êtes libre de l'utiliser et de le modifier, mais veuillez créditer l'auteur original.
