using System;
using System.Text;
using CamBib.Class;
using CamBib.Fonctions;

using UnityEngine;
using UnityEngine.Tilemaps;

public class MapGenerator : MonoBehaviour
{
    // Declaration
    public Tilemap[] tilemaps;
    public TileBase[] tiles;

    // Variables de generation
    public static TabRepartition<int> nbSalles = new TabRepartition<int>(IntUtils.Sequence(7,20), "arithmetique" );
    public static TabRepartition<int> tailleSalle= new TabRepartition<int>(IntUtils.Sequence(5, 9), new double[] { 0.15, 0.5, 0.90, 1 });
    public static int[][] tableauDirections = IntUtils.Directions2D(2); // directions possibles discretisees
    public static int deltaDeltaIndexMax;
    public static TabRepartition<int> distancesInterSommets = new TabRepartition<int>(IntUtils.Sequence(10, 20), "centreeArithmetique");


    void Start()
    {
        // Génération procédurale de la tranchée
        Graphe<DataSommet> grapheTranchee = GenerateTranchee();

        // Mise en place dans les tilemaps
        GrapheToTilemaps(grapheTranchee);
    }


    //// Generateur du graphe de la Tranchee

    /// <summary>
    /// Represente un sommet du graphe de la tranchee et contient les informations necessaires à sa construction
    /// </summary>
    public class DataSommet
    {
        // Variables
        private PaireInt position;
        private int taille;

        // Build
        public DataSommet()
        {
            position = new PaireInt();
            taille = -1;
        }
        public DataSommet(DataSommet model)
        {
            position = model.position;
            taille = model.taille;
        }
        public DataSommet(PaireInt position, int taille)
        {
            this.position = position;
            this.taille = taille;
        }

        // Get
        public PaireInt Position() { return position; }
        public int Taille() { return taille; }

        // Set
        public void SetPosition(PaireInt position) { this.position = position; }
        public void SetTaille(int taille) { this.taille = taille; }

        // To String
        public override String ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("taille: " + taille);
            builder.AppendLine(" ; position x: " + position[0] + " y: " + position[1]);
            return builder.ToString();
        }
    }

    /// <summary>
    /// Génère procéduralement le graphe représentant une Tranchée.
    /// </summary>
    /// <param name="procedure">La procédure de génération à utiliser. Peut être "direction moyenne" (par défaut)".</param>
    /// <returns>Le Graphe de la tranchée.</returns>
    /// <exception cref="ArgumentException">Lancée si le type de génération spécifié n'est pas implémenté.</exception>
    public static Graphe<DataSommet> GenerateTranchee(string procedure = "direction moyenne")
    {
        // Initialisation
        System.Random random = new System.Random();
        Graphe<DataSommet> grapheTranchee = new Graphe<DataSommet>();

        // Inittialise la taille de la tranchée
        int tailleTranchee = nbSalles.Observation();

        // Ajoute les sommets
        switch (procedure)
        {
            case "direction precedente":
                deltaDeltaIndexMax = 4; // plus ce parametre est grand plus c'est sinueux


                // Direction initiale
                int indexDirection = random.Next(0, MapGenerator.tableauDirections.Length);
                // Loi de répartition de l'ecart avec l'indice de la direction suivante
                int[] deltaIndexs = IntUtils.Sequence(-deltaDeltaIndexMax, deltaDeltaIndexMax+1);
                TabRepartition<int> deltaIndexNextDirection = new TabRepartition<int>(deltaIndexs, "centreeArithmetique");
                // Sommet initial
                grapheTranchee.AjouteSommet(new DataSommet(
                    new PaireInt(0, 0),
                    tailleSalle.Observation()));

                // Ajoute les sommets les uns à la suite des autres
                for (int i = 1; i < tailleTranchee; i++)
                {
                    // index nouvelle direction
                    indexDirection += deltaIndexNextDirection.Observation();
                    // direction et distance
                    Paire<double> dir = new PaireInt(TableauUtils.Valeur(tableauDirections,indexDirection)).Unitaire();
                    int dist = distancesInterSommets.Observation();
                    // Calcule la position
                    PaireInt nextPosition = PaireInt.Somme(
                        grapheTranchee.DonneeSommet(i - 1).Position(),
                        new PaireInt(NumUtils.ArrondiEntier(dist * dir.X()), NumUtils.ArrondiEntier(dist * dir.Y())));
                    // Ajoute le sommet
                    grapheTranchee.AjouteSommet(new DataSommet(nextPosition, tailleSalle.Observation()));
                    // Cree une arrete entre le nouveau sommet et le precedent
                    grapheTranchee.CreeArrete(i - 1, i);
                }
                break;

            case "direction moyenne":
                // Direction moyenne
                int indexDirectionMoy = random.Next(0, tableauDirections.Length);

                // Loi de répartition des directions
                int deltaIndexMax = 2;
                int premierIndex = indexDirectionMoy - deltaIndexMax;
                int dernierIndex = indexDirectionMoy + deltaIndexMax;
                int[][] dirs = TableauUtils.SousTableau(tableauDirections,premierIndex,dernierIndex+1);
                TabRepartition<int[]> direction = new TabRepartition<int[]>(dirs, "centreeArithmetique");

                // Loi de répartition des distances inter-sommets
                int[] tableauDistances = IntUtils.Sequence(10,20);
                TabRepartition<int> distance = new TabRepartition<int>(tableauDistances, "centreeArithmetique");
                
                // Sommet initial
                grapheTranchee.AjouteSommet(new DataSommet(
                    new PaireInt(0, 0),
                    tailleSalle.Observation()));

                // Ajoute les sommets les uns à la suite des autres
                for (int i = 1; i < tailleTranchee; i++)
                {
                    // direction et distance
                    Paire<double> dir = new PaireInt(direction.Observation()).Unitaire(); 
                    int dist = distance.Observation();
                    // Calcule la position
                    PaireInt nextPosition = PaireInt.Somme( 
                        grapheTranchee.DonneeSommet(i-1).Position(),
                        new PaireInt( NumUtils.ArrondiEntier(dist*dir.X()), NumUtils.ArrondiEntier(dist * dir.Y())));
                    // Ajoute le sommet
                    grapheTranchee.AjouteSommet(new DataSommet(nextPosition, tailleSalle.Observation()));
                    // Cree une arrete entre le nouveau sommet et le precedent
                    grapheTranchee.CreeArrete(i - 1, i);
                }
                break;

            case "circulaire":
                // Choix des directions
                bool[] directions = tailleTranchee > tableauDirections.Length ? BoolUtils.GetArrayOfTrue(tableauDirections.Length) : StatUtils.RandomKparmisN(tailleTranchee, tableauDirections.Length);

                // Loi de répartition des distances sommets-centre
                int[] tabDistancesSommetsCentre = IntUtils.Sequence(20, 10, -1);
                TabRepartition<int> distanceSommetCentre = new TabRepartition<int>(tabDistancesSommetsCentre, "geometrique");

                // Ajout des sommets
                for (int indexDir = 0; indexDir < tableauDirections.Length; indexDir++)
                {
                    // Verifie si il y a un sommet dans cette direction
                    if (!directions[indexDir]) continue;

                    // Position du sommet
                    Paire<double> dir = new PaireInt(tableauDirections[indexDir]).Unitaire();
                    int distCentre = distanceSommetCentre.Observation();
                    PaireInt pos = Paire<double>.Multiplication(dir, (double)distCentre);

                    // Ajout sommet
                    grapheTranchee.AjouteSommet(new DataSommet(pos, tailleSalle.Observation()));

                    // Creer une arrete
                    int indiceSommet = grapheTranchee.Ordre() - 1;
                    if (indiceSommet > 0) { grapheTranchee.CreeArrete(indiceSommet - 1, indiceSommet); }
                }

                // Creation d'arrete entre le premier et le dernier sommet
                grapheTranchee.CreeArrete(0, grapheTranchee.Ordre() - 1);
                break;

            default:
                // Erreur
                Debug.Log($"Fonction: GenerateTranchee - type: {procedure}");
                throw new ArgumentException("Ce type de génération n'est pas implémentée", "type"); 
        } 


        return grapheTranchee;
    }


    ///// Systemes d'ajout dans la tilemap

    //// Fonctions communes

    /// <summary>
    /// Cree le Blueprint d'une chambre.
    /// Le blueprint est un carre (de valeurs 0) de cote "taille" avec une bordure intérieure (de valeurs 1)
    /// La position fait que le carre est centré sur le repere.
    /// </summary>
    /// <param name="taille">La taille souhaitée pour cette chambre</param>
    /// <returns>Le Blueprint representant la chambre</returns>
    public static Blueprint BlueprintChambre(DataSommet sommet)
    {
        // Donnees du Sommet
        PaireInt positionSommet = sommet.Position();
        int taille = sommet.Taille();
        // Matrice
        Blueprint chambre = new Blueprint(taille); // sol
        chambre.InsertToBlueprint(Blueprint.Box(taille, value: 1), insertionBehavior: "replace"); // ajout de murs
        // Position
        chambre.SetPosition(PaireInt.Somme(positionSommet, new PaireInt(-taille / 2, -taille / 2))); // On centre la chambre sur sa position

        return chambre;
    }

    public static Blueprint BlueprintCouloir(DataSommet sommetA, DataSommet sommetB)
    {
        // Largeur du couloir
        int largeur = LargeurCouloir(sommetA.Taille(), sommetB.Taille());

        //// motif elementaire du couloir 
        // sol
        Blueprint couloirElem = new Blueprint(largeur);
        // murs
        couloirElem.InsertToBlueprint(Blueprint.Box(largeur, value: 1));
        // Centrage dans le repere
        couloirElem.SetPosition(new PaireInt(-largeur / 2, -largeur / 2));

        // Dimensions du couloir
        PaireInt dimensionCouloir = PaireInt.Soustraction(sommetB.Position(), sommetA.Position());
        // Création du Blueprint du couloir
        Blueprint couloir = Blueprint.Line(dimensionCouloir, couloirElem);
        // Position
        couloir.SetPosition(PaireInt.Somme(couloir.Position(), sommetA.Position()));

        return couloir;
    }

    /// <summary>
    /// Calcule la largeur du couloir, y compris les bords en fonction des tailles des sommets adjacents.
    /// La largeur minimum est de 3, son maximum est 7, et elle est inférieur à la taille du plus petit sommet adjecent.
    /// </summary>
    /// <param name="tailleSommet1">La taille du premier sommet adjacent.</param>
    /// <param name="tailleSommet2">La taille du deuxième sommet adjacent.</param>
    /// <returns>La largeur du couloir.</returns>
    public static int LargeurCouloir(int tailleSommet1, int tailleSommet2)
    {
        int largeurTranchee = IntUtils.Min(new int[] { tailleSommet1, tailleSommet2, 7 }) - 2;
        largeurTranchee = IntUtils.Max(new int[] { largeurTranchee, 3});
        return largeurTranchee;
    }


    //// Blueprint unique (obsolete)
    /* Exemple
    // Génération procédurale de la tranchée
    Graphe<DataTranchee> grapheTranchee = GenerateTranchee();
    Debug.Log(grapheTranchee.ToString());
      
    // Conversion en Blueprint
    Blueprint bpTilemapTranchee = GrapheToBlueprint(grapheTranchee);
    Blueprint bpTilesTranchee = new Blueprint(bpTilemapTranchee);
        
    // Mise en place des tilemaps
    Terraform(bpTilemapTranchee, bpTilesTranchee);
     */
    /// <summary>
    /// Convertit un graphe représentant des tranchées en un blueprint représentant des salles et des couloirs.
    /// </summary>
    /// <param name="graphe">Le graphe représentant les tranchées.</param>
    /// <returns>Le blueprint représentant les salles et les couloirs générés à partir du graphe.</returns>
    /// <remarks>
    /// Cette fonction génère un blueprint à partir d'un graphe représentant des tranchées. Chaque sommet du graphe est converti en une salle carrée, et chaque arête du graphe est convertie en un couloir reliant les salles correspondantes.
    /// La taille des salles carrées et la largeur des couloirs sont des paramètres configurables.
    /// </remarks>
    public static Blueprint GrapheToBlueprint(Graphe<DataSommet> graphe)
    {
        // Intialisation
        Blueprint blueprint = new Blueprint(); // Blueprint du graphe

        // Parcours de chaque sommet du graphe pour générer les chambres
        for (int indexSommet = 0 ; indexSommet < graphe.Ordre(); indexSommet++)
        {
            // Blueprint de la chambre
            Blueprint chambre = BlueprintChambre(graphe.DonneeSommet(indexSommet));
            // Ajout sur le Blueprint global
            blueprint.InsertToBlueprint(chambre, insertionBehavior:"min", resize:true);
        }

        
        // Parcours de chaque arrete du graphe pour générer les couloirs
        foreach (PaireInt arrete in graphe.Arretes())
        {
            // Blueprint du couloir
            Blueprint couloir = BlueprintCouloir(graphe.DonneeSommet(arrete[0]), graphe.DonneeSommet(arrete[1]));
            // Ajout sur le Blueprint global
            blueprint.InsertToBlueprint(couloir, insertionBehavior:"min", resize:true);
        }
        
        return blueprint;
    }

    /// <summary>
    /// Imprime un motif dans les Tilemaps. Permet de poser le décor du jeu.
    /// Le type Blueprint inclu la position et la nature des tiles à placer.
    /// </summary>
    /// <param name="tilesToAdd">Le Blueprint dont les valeurs renvoient aux tiles de mêmes indices dans le vecteur tile (si une valeur vaut -1, aucune tile n'est placée).</param>
    /// <param name="toTilemaps">Le Blueprint dont les valeurs indiquent dans quelle tilemap ajouter la tile correspondante. La valeur renvoie à la tilemap du même indice dans le vecteur Tilemaps (si une valeur vaut -1, aucune tile n'est placée).</param>
    /// Exemples
    /*
        Blueprint bpa = new Blueprint(6, 5);
        Terraform(bpa, bpa, new Vector3Int(-5,5));
    
        Blueprint bpb = new Blueprint(4, 5, 0);
        Blueprint box = Blueprint.Box(4, 5, 1);
        Blueprint bpd = Blueprint.Line(new PaireInt(-20, -15), bpb);
        Terraform(bpd, bpd);

        Blueprint bpc = Blueprint.Line(new Vector2Int(29,47), 4);
        Terraform(bpc, bpc);

        Blueprint bpa = new Blueprint(6, 5);
        Blueprint bpb = new Blueprint(4,value:1,position: new PaireInt(-10,-8));
        bpa.InsertToBlueprint(bpb,resize:true);
        Terraform(bpa, bpa);
    */
    private void Terraform(Blueprint toTilemaps, Blueprint tilesToAdd)
    {
        // Erreur
        if (toTilemaps.Width() != tilesToAdd.Width() || toTilemaps.Height() != tilesToAdd.Height() || toTilemaps.Position() != tilesToAdd.Position())
        {
            throw new ArgumentException("Les Blueprints doivent avoir les mêmes valeurs de width height et position", "tilesToAdd et toTilemaps");
        }
        if (toTilemaps.IsEmpty() || tilesToAdd.IsEmpty() || toTilemaps.IsTransparent() || tilesToAdd.IsTransparent())
        {
            throw new ArgumentException("Aucun Blueprint ne doit être transparent ou vide", "tilesToAdd et toTilemaps");
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
                int tileIndex = tilesToAdd[i, j];
                int tilemapIndex = toTilemaps[i, j];
                // Vérifie s'il y a une tile à placer
                if (tilemapIndex == -1 || tileIndex == -1)
                {
                    continue; // Passe à l'itération suivante si aucune tile ne doit être placée
                }

                // Récupère la tilemap, la tile et la position à laquelle la placer dans la tilemap
                Tilemap tilemap = tilemaps[tilemapIndex];
                TileBase tile = tiles[tileIndex];
                Vector3Int tilePosition = new Vector3Int(i, j, 0);

                // Place la tile dans la tilemap
                tilemap.SetTile(tilePosition, tile);
            }
        }
    }


    //// Piece par piece
    /// <summary>
    /// Converts a graphe into tilemaps by generating rooms from the vertices and corridors from the edges.
    /// </summary>
    /// <param name="graphe">The graph containing the trench data.</param>
    /// <remarks>
    /// This method processes each vertex in the graph to generate rooms and each edge to generate corridors, adding them to the tilemaps.
    /// </remarks>
    /// <example>
    /// <code>
    /// Graphe<DataTranchee> grapheTranchee = GenerateTranchee();
    /// // Assume graphe is initialized and populated with data
    /// GrapheToTilemaps(grapheTranchee);
    /// </code>
    /// </example>
    private void GrapheToTilemaps(Graphe<DataSommet> graphe)
    {
        // Parcours de chaque sommet du graphe pour générer les chambres
        for (int indexSommet = 0; indexSommet < graphe.Ordre(); indexSommet++)
        {
            // Blueprint de la chambre
            Blueprint chambre = BlueprintChambre(graphe.DonneeSommet(indexSommet));
            // Ajout aux tilemaps
            InsertToTilemaps(chambre, chambre, insertionBehavior: "min");
        }

        
        // Parcours de chaque arrete du graphe pour générer les couloirs
        foreach (PaireInt arrete in graphe.Arretes())
        {
            // Blueprint du couloir
            Blueprint couloir = BlueprintCouloir(graphe.DonneeSommet(arrete[0]), graphe.DonneeSommet(arrete[1]));
            // Ajout sur le Blueprint global
            InsertToTilemaps(couloir, couloir, insertionBehavior: "min");
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
    private void InsertToTilemaps(Blueprint toTilemaps, Blueprint tilesToAdd, PaireInt offset = null, string insertionBehavior = "replace")
    {
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
                    while(t < tilemapIndex && !nextInsertion)
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
                    while (t<tilemaps.Length)
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
}

