// Author : Camille ANSEL
// Creation : 06-2024

using System;
using System.Text;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using CamBib.Fonctions;
using CamBib.Class;

namespace CamBibUnity
{
    public static class TilemapUtils
    {
        /// <summary>
        /// Efface toutes les tiles de chaque Tilemap.
        /// </summary>
        public static void ClearTilemaps(Tilemap[] tilemaps)
        {
            foreach (Tilemap tilemap in tilemaps)
            {
                tilemap.ClearAllTiles();
            }
        }

        /// <summary>
        /// Imprime un motif dans les Tilemaps, permettant ainsi de poser le décor du jeu.
        /// Le type Blueprint inclut la position et la nature des tiles à placer.
        /// La méthode d'ajout peut être choisie.
        /// </summary>
        /// <param name="tilesToAdd">
        /// Le Blueprint dont les valeurs renvoient aux tiles de mêmes indices dans le vecteur tiles. 
        /// Si une valeur vaut -1, aucune tile n'est placée.
        /// </param>
        /// <param name="toTilemaps">
        /// Le Blueprint dont les valeurs indiquent dans quelle tilemap ajouter la tile correspondante.
        /// La valeur renvoie à la tilemap du même indice dans le vecteur tilemaps.
        /// Si une valeur vaut -1, aucune tile n'est placée.
        /// </param>
        /// <param name="offset">
        /// Décalage optionnel appliqué à la position des Blueprints.
        /// Si non spécifié, le décalage est nul.
        /// </param>
        /// <param name="insertionBehavior">
        /// Spécifie la méthode d'ajout des tiles. 
        /// "replace" pour remplacer les tiles existantes, "min" pour n'ajouter une tile que si son indice est inférieur à l'indice de la tile actuelle.
        /// Par défaut, la valeur est "replace".
        /// </param>
        /// <exception cref="ArgumentException">
        /// Lancée si un Blueprint est vide ou transparent, ou si les Blueprints n'ont pas les mêmes dimensions ou positions.
        /// </exception>
        public static void InsertToTilemaps(Tilemap[] tilemaps, TileBase[] tiles, Blueprint toTilemaps, Blueprint tilesToAdd, PaireInt offset = null, string insertionBehavior = "replace")
        {
            Debug.Log("insert to tilemap running");
            // Erreur
            if (toTilemaps.IsEmpty() || tilesToAdd.IsEmpty() || toTilemaps.IsTransparent() || tilesToAdd.IsTransparent())
            {
                throw new ArgumentException("Aucun Blueprint ne doit être transparent ou vide", "tilesToAdd et toTilemaps");
            }
            if (toTilemaps.Width() != tilesToAdd.Width() || toTilemaps.Height() != tilesToAdd.Height() || toTilemaps.Position() != tilesToAdd.Position())
            {
                throw new ArgumentException("Les Blueprints doivent avoir les mêmes valeurs de width height et position", "tilesToAdd et toTilemaps");
            }

            // decalage
            if (offset != null) // Cela modifie le blueprint donné. la transformation est inversée à la toute fin de l'execution de cette fonction
            {
                toTilemaps.SetPosition(PaireInt.Somme(toTilemaps.Position(), offset));
                tilesToAdd.SetPosition(PaireInt.Somme(tilesToAdd.Position(), offset));
            }

            // On regroupe les valeurs en commun
            PaireInt position = toTilemaps.Position();
            int width = toTilemaps.Width();
            int height = toTilemaps.Height();

            // Parcours des valeurs des Blueprints
            for (int i = position.X(); i < position.X() + width; i++)
            {
                for (int j = position.Y(); j < position.Y() + height; j++)
                {
                    // Verifie si il y a une tile a placer
                    int tileIndex = tilesToAdd[i, j];
                    int tilemapIndex = toTilemaps[i, j];
                    if (tilemapIndex == -1 || tileIndex == -1) { continue; }

                    // Récupère la tilemap, la tile et la position à laquelle la placer dans la tilemap
                    Tilemap tilemap = tilemaps[tilemapIndex];
                    TileBase tile = tiles[tileIndex];
                    Vector3Int tilePosition = new Vector3Int(i, j, 0);

                    // InsetionBehavior : min
                    if (insertionBehavior == "min")
                    {
                        // Rerifie qu'il n'y a pas de tilemap remplie en dessous
                        bool nextInsertion = false;
                        int currentTileIndex;
                        int t = 0;
                        while (t < tilemapIndex && !nextInsertion)
                        {
                            currentTileIndex = Array.IndexOf(tiles, tilemaps[t].GetTile(tilePosition));
                            if (currentTileIndex != -1) { nextInsertion = true; }
                            t++;
                        }
                        currentTileIndex = Array.IndexOf(tiles, tilemaps[tilemapIndex].GetTile(tilePosition));
                        if (nextInsertion) { continue; }

                        // Verifie qu'il n'y a pas deja une tile avec un ordre de priorite superieur
                        if (currentTileIndex != -1 && currentTileIndex < tileIndex) { continue; }

                        // Retire tiles presentes au dessus
                        t = tilemapIndex + 1;
                        while (t < tilemaps.Length)
                        {
                            tilemaps[t].SetTile(tilePosition, null);
                            t++;
                        }
                    }

                    // Place la tile dans la tilemap correspondante
                    tilemap.SetTile(tilePosition, tile);
                }
            }

            // Annulation du decalage
            if (offset != null)
            {
                toTilemaps.SetPosition(PaireInt.Soustraction(toTilemaps.Position(), offset));
                tilesToAdd.SetPosition(PaireInt.Soustraction(tilesToAdd.Position(), offset));
            }
        }
        
        /// <summary>
        /// Insère des tiles dans plusieurs tilemaps en fonction des blueprints fournis.
        /// </summary>
        /// <param name="tilemaps">Tableau des tilemaps dans lesquelles insérer les tiles.</param>
        /// <param name="tiles">Tableau des tiles disponibles pour l'insertion.</param>
        /// <param name="blueprints">Liste des blueprints définissant où et quelles tiles insérer.</param>
        public static void InsertToTilemaps(Tilemap[] tilemaps, TileBase[] tiles, List<Blueprint> blueprints)
        {
            // Parcours des valeurs des Blueprints
            for (int tilemapIndex = 0; tilemapIndex < blueprints.Count; tilemapIndex++)
            {
                Blueprint blueprint = blueprints[tilemapIndex];

                for (int i = blueprint.Position().X(); i < blueprint.Position().X() + blueprint.Width(); i++)
                {
                    for (int j = blueprint.Position().Y(); j < blueprint.Position().Y() + blueprint.Height(); j++)
                    {
                        // Verifie si il y a une tile a placer
                        int tileIndex = blueprint[i, j];
                        if (tileIndex == -1) { continue; }

                        // Récupère la tilemap, la tile et la position à laquelle la placer dans la tilemap
                        Tilemap tilemap = tilemaps[tilemapIndex];
                        TileBase tile = tiles[tileIndex];
                        Vector3Int tilePosition = new Vector3Int(i, j, 0);
                        
                        // Insertion Bottom
                        if (tilemapIndex == 0)
                        {
                            // Retire tiles presentes dans la tilemap "Limits"
                            tilemaps[1].SetTile(tilePosition, null);
                        }

                        // Insertion Limits
                        if (tilemapIndex == 1)
                        {
                            // Verifie qu'il n'y a pas de tile en dessous
                            if (tilemaps[0].GetTile(tilePosition) != null) { continue; }
                        }

                        // Place la tile dans la tilemap correspondante
                        tilemap.SetTile(tilePosition, tile);
                    }
                }
            }

        }
    }

    public static class PrefabUtils
    {
        /// <summary>
        /// Instancie des préfabs à des positions souhaitées.
        /// </summary>
        /// <param name="prefabs">Un tableau de tuples associant chacun une position au prefab que l'on souhaite instancier.</param>
        /// <param name="grid">Si ce paramètre est renseigné, la grille est utilisée pour récupérer la bonne position.</param>
        /// <returns>Un tableau contenant les références des objets instanciés (dans l'ordre du tableau de tuples).</returns>
        public static GameObject[] InstanciePrefabs(Tuple<GameObject, Paire<int>>[] prefabs, Grid grid = null, float[] offset = null)
        {
            // Offset
            Vector3 vectOffset;
            if (offset != null)
            {
                if(offset.Length != 3) { throw new ArgumentException("Le tableau représente les coordonnées d'un vecteur3. Il doit avoir 3 valeurs","offset"); }
                vectOffset = new Vector3(offset[0], offset[1], offset[2]);
            }
            else
            {
                vectOffset = Vector3.zero;
            }

            List<GameObject> objets = new List<GameObject>();

            foreach (var element in prefabs)
            {
                Vector3 position;
                if (grid != null)
                {
                    // Convertir la position de la cellule en position dans la scène en utilisant la grille
                    position = grid.CellToWorld(new Vector3Int(element.Item2.X(), element.Item2.Y(), 0));
                }
                else
                {
                    // Utiliser directement la position en Vector3 si aucune grille n'est fournie
                    position = new Vector3(element.Item2.X(), element.Item2.Y(), 0);
                }

                // Instancier le prefab à la position calculée avec une rotation par défaut
                GameObject gameObject = UnityEngine.Object.Instantiate(element.Item1, position + vectOffset, Quaternion.identity);
                objets.Add(gameObject);
            }

            // Retourner le tableau des objets instanciés
            return objets.ToArray();
        }
    }

    public static class Raycast2DUtils
    {
        public static RaycastHit2D Entre2Positions(Vector3 positionA, Vector3 positionB, LayerMask layerMask)
        {
            Vector3 direction = positionB - positionA; 
            float distance = direction.magnitude;
            return Physics2D.Raycast(positionA, direction, distance, layerMask);
        }
    }

    /// <summary>
    /// Classe qui reproduit le comportement d'un dictionnaire tout en étant affiché dans unity
    /// !!! Elle n'est pas codé corretement (champs privés, constructeur, setters, getter, ...)
    /// </summary>
    /// <typeparam name="TClef"></typeparam>
    /// <typeparam name="TValeur"></typeparam>
    [Serializable]
    public class MaDict<TClef, TValeur> 
        where TClef : IEquatable<TClef>
    {
        // Classe paire clef-valeur
        [Serializable]
        public class DictAssociation
        {
            [SerializeField] private TClef clef;
            [SerializeField] private TValeur valeur;

            // build
            public DictAssociation()
            {
                clef = default;
                valeur = default;
            }
            public DictAssociation(DictAssociation model)
            {
                clef = model.clef;
                valeur = model.valeur;
            }
            public DictAssociation(TClef clef, TValeur valeur)
            {
                this.clef = clef;
                this.valeur = valeur;
            }

            // get
            public TClef Clef() { return clef; }
            public TValeur Valeur() { return valeur; }

            // Set
            public void SetClef(TClef clef) { this.clef = clef; }
            public void SetValeur(TValeur valeur) { this.valeur = valeur; }

            // ToString
            public override string ToString()
            {
                StringBuilder sb = new StringBuilder();
                sb.Append($"association: {{{this.clef}, ");

                if (valeur != null)
                {
                    sb.AppendLine($"{this.valeur}}}");
                }
                else
                {
                    sb.AppendLine(" null}");
                }

                return sb.ToString();
            }
        }

        [SerializeField] private DictAssociation[] objets;
        [SerializeField] private bool clefsUniques;

        // build
        public MaDict(bool clefsUniques = false)
        {
            objets = new DictAssociation[0];
            this.clefsUniques = clefsUniques;
        }
        public MaDict(MaDict<TClef,TValeur> model)
        {
            objets = new DictAssociation[model.objets.Length];
            for (int i = 0; i < model.objets.Length; i++)
            {
                objets[i] = new DictAssociation(model.objets[i]);
            }
            clefsUniques = model.clefsUniques;
        }

        // get
        public bool ClefsUniques() { return clefsUniques; }
        public int Length() { return objets.Length; }
        public TValeur Valeur(TClef clef) // Retourne la première occurence
        {
            foreach (DictAssociation objet in objets)
            {
                if (EqualityComparer<TClef>.Default.Equals(objet.Clef(), clef)) { return objet.Valeur(); }
            }
            return default;
        }
        public TClef[] Clefs()
        {
            TClef[] clefs = new TClef[objets.Length];
            for (int k = 0; k < objets.Length; k++) { clefs[k] = objets[k].Clef(); }
            return clefs;
        }
        public TClef[] DifferentesClefs()
        {
            List<TClef> differentesClefs = new List<TClef>();
            for (int k = 0; k < objets.Length; k++) 
            {
                TClef clef = objets[k].Clef();
                if (!differentesClefs.Contains(clef)) { differentesClefs.Add(clef); } 
            }
            return differentesClefs.ToArray();
        }
        public TValeur[] Valeurs()
        {
            TValeur[] valeurs = new TValeur[objets.Length];
            for (int k = 0; k < objets.Length; k++) { valeurs[k] = objets[k].Valeur(); }
            return valeurs;
        }

        // Set
        /// <summary>
        /// Set la valeur associée à une clef.
        /// Attention, cela ne set que la première valeur
        /// </summary>
        /// <param name="clef">La clef recherchée</param>
        /// <param name="valeur">La nouvelle valeur à associer à cette clef</param>
        public void SetValeur(TClef clef, TValeur valeur)
        {
            foreach (DictAssociation objet in objets)
            {
                if (clef.Equals(objet.Clef()))
                {
                    objet.SetValeur(valeur);
                    return;
                }
            }
            return;
        }

        // ToString
        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine($"MaDict<{typeof(TClef)}, {typeof(TValeur)}> ; Longueur:{objets.Length}");
            foreach (DictAssociation objet in objets)
            {
                builder.AppendLine('|' + objet.ToString());
            }

            return builder.ToString();
        }

        // Indicateur
        public bool Contient(TClef clef)
        {
            for (int k = 0; k < objets.Length; k++)
            {
                if (clef.Equals(objets[k].Clef())) { return true; }
            }
            return false;
        }

        /// <summary>
        /// Vérifie si les clefs d'un dictionnaire sont uniques indépendamment du paramètre ClefsUniques
        /// </summary>
        /// <returns>true si les associations du dictionnaires on toutes des clefs differentes</returns>
        public bool IsClefsUniques()
        {
            List<TClef> verifiees = new List<TClef>();
            for (int k = 0; k < objets.Length; k++) 
            { 
                if (verifiees.Contains(objets[k].Clef())) { return false; }
                else { verifiees.Add(objets[k].Clef()); }
            }
            return true;
        }

        public void Ajoute(DictAssociation objet)
        {
            // Erreur ClefsUniques
            if(clefsUniques && Contient(objet.Clef())) { throw new Exception("La clefs est déjà associée à une valeur"); }

            DictAssociation[] nouveauxObjets = new DictAssociation[objets.Length + 1];
            for (int i = 0; i < objets.Length; i++) { nouveauxObjets[i] = objets[i]; }
            nouveauxObjets[^1] = objet;
            objets = nouveauxObjets;
        }
        public void Ajoute(TClef clef, TValeur valeur)
        {
            // Erreur ClefsUniques
            if (clefsUniques && Contient(clef)) { throw new Exception("La clefs est déjà associée à une valeur"); }

            DictAssociation[] nouveauxObjets = new DictAssociation[objets.Length + 1];
            for (int i = 0; i < objets.Length; i++) { nouveauxObjets[i] = objets[i]; }
            nouveauxObjets[^1] = new DictAssociation(clef, valeur);
            objets = nouveauxObjets;
        }

        /// <summary>
        /// Retire une paire clef/valeur.
        /// Attention, cela n'impacte que la première occurence
        /// </summary>
        /// <param name="clef">la clef de l'association clef/valeur que l'on veut retirer du dictionnaire</param>
        public void Retire(TClef clef)
        {
            int index = 0;
            while (index < objets.Length && !objets[index].Clef().Equals(clef)) { index++; }
            if (index < objets.Length)
            {
                objets = objets.Retire(index);
            }
            throw new ArgumentException("Aucune association du dictionnaire n'a cette clef","clef");
        }

        /// <summary>
        /// Retire et compte toutes les associations ayant une clef précise du dictionnaire
        /// </summary>
        /// <param name="clef">La clef dont on veut retirer toutes les associations</param>
        /// <returns>Le nombre d'associations retirées</returns>
        public int RetireTous(TClef clef)
        {
            List<int> indices = new List<int>();
            for (int i = 0; i < objets.Length; i++) { if (objets[i].Clef().Equals(clef)) { indices.Add(i); } }
            objets = objets.Retire(indices.ToArray());
            return indices.Count;
        }
        
        /// <summary>
        /// Récupérer une valeur correspondant à une clef en retirant son association du dictionnaire
        /// Cela prend en compte la première occurence rencontrée
        /// </summary>
        /// <param name="clef">La clef de l'association recherchée</param>
        /// <returns>La valeur retirée du dictionnaire</returns>
        public TValeur Pop(TClef clef)
        {
            TValeur association = Valeur(clef);
            Retire(clef);
            return association;
        }

        /// <summary>
        /// Compte le nombre d'association ayant une clef particulière
        /// </summary>
        /// <param name="clef">La clef</param>
        /// <returns>Le nombre d'associations ayant la clef passée en paramètre</returns>
        public int Compte(TClef clef)
        {
            int compte = 0;
            for (int i = 0; i < objets.Length; i++) { if (objets[i].Equals(clef)) { compte++; } }
            return compte;
        }

        // MaDict to Dictionnary
        public Dictionary<TClef,TValeur> ToDictionary()
        {
            Dictionary<TClef, TValeur> dictionnaire = new Dictionary<TClef, TValeur>();

            foreach (DictAssociation paire in objets)
            {
                dictionnaire.Add(paire.Clef(), paire.Valeur());
            }

            return dictionnaire;
        }
    }
}
