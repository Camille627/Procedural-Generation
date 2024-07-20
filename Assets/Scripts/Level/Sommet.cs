using System;
using Random = System.Random;
using System.Text;
using CamBib.Class;
using CamBib.Fonctions;
using CamBibUnity;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace SommetSimple
{
    public interface SimpleProcedure<DataSommet>
    {
        public abstract Graphe<DataSommet> GenerateGraphe();
    }

    // Classe contenant les information des sommets
    public class DataSommetSimple
    {
        // Variables
        private PaireInt position;
        private int taille;

        // Build
        public DataSommetSimple()
        {
            position = new PaireInt();
            taille = -1;
        }
        public DataSommetSimple(DataSommetSimple model)
        {
            position = model.position;
            taille = model.taille;
        }
        public DataSommetSimple(PaireInt position, int taille)
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

    // Classes Generatrices
    public class DirectionPrecedente : SimpleProcedure<DataSommetSimple>
    {
        // Key variables
        private Random random;
        private int[][] directionsDiscretes; // ensemble des vecteurs des directions

        private int ordreGraphe; // nombre de sommets
        private int deltaIndexNextDirectionMax; // plus ce parametre est grand plus c'est sinueux

        private TabRepartition<int> distancesInterSommets;
        private TabRepartition<int> tailleSommet;

        // build
        public DirectionPrecedente()
        {
            random = new Random();
            directionsDiscretes = IntUtils.Directions2D(2);
            ordreGraphe = 8;
            deltaIndexNextDirectionMax = 3;
            tailleSommet = new TabRepartition<int>(IntUtils.Sequence(5, 9), new double[] { 0.15, 0.5, 0.90, 1 });
            distancesInterSommets = new TabRepartition<int>(IntUtils.Sequence(10, 20), "centreeArithmetique");
        }
        public DirectionPrecedente(DirectionPrecedente model)
        {
            random = model.random;
            directionsDiscretes = model.directionsDiscretes;
            ordreGraphe = model.ordreGraphe;
            deltaIndexNextDirectionMax = model.deltaIndexNextDirectionMax;
            tailleSommet = model.tailleSommet;
            distancesInterSommets = model.distancesInterSommets;
        }
        public DirectionPrecedente(Random random, int[][] directionsDiscretes, int ordreGraphe, int deltaIndexNextDirectionMax, TabRepartition<int> tailleSommet, TabRepartition<int> distancesInterSommets)
        {
            this.random = random;
            this.directionsDiscretes = directionsDiscretes;
            this.ordreGraphe = ordreGraphe;
            this.deltaIndexNextDirectionMax = deltaIndexNextDirectionMax;
            this.tailleSommet = tailleSommet;
            this.distancesInterSommets = distancesInterSommets;
        }

        // set

        // Generateur
        public Graphe<DataSommetSimple> GenerateGraphe()
        {
            // Initialisation
            Graphe<DataSommetSimple> graphe = new Graphe<DataSommetSimple>();

            // Direction initiale
            int indexDirection = random.Next(0, MapGenerator.tableauDirections.Length);

            // Loi de répartition de l'ecart avec l'indice de la direction suivante
            int[] deltaIndexs = IntUtils.Sequence(-deltaIndexNextDirectionMax, deltaIndexNextDirectionMax + 1);
            TabRepartition<int> deltaIndexNextDirection = new TabRepartition<int>(deltaIndexs, "centreeArithmetique");
            // Sommet initial
            graphe.AjouteSommet(new DataSommetSimple(
                new PaireInt(0, 0),
                tailleSommet.Observation()));

            // Ajoute les sommets les uns à la suite des autres
            for (int i = 1; i < ordreGraphe; i++)
            {
                // index nouvelle direction
                indexDirection += deltaIndexNextDirection.Observation();
                // direction et distance
                Paire<double> dir = new PaireInt(TableauUtils.Valeur(directionsDiscretes, indexDirection)).Unitaire();
                int dist = distancesInterSommets.Observation();
                // Calcule la position
                PaireInt nextPosition = PaireInt.Somme(
                    graphe.DonneeSommet(i - 1).Position(),
                    new PaireInt(NumUtils.ArrondiEntier(dist * dir.X()), NumUtils.ArrondiEntier(dist * dir.Y())));
                // Ajoute le sommet
                graphe.AjouteSommet(new DataSommetSimple(nextPosition, tailleSommet.Observation()));
                // Cree une arrete entre le nouveau sommet et le precedent
                graphe.CreeArrete(i - 1, i);
            }

            return graphe;
        }
    }

    public class DirectionMoyenne : SimpleProcedure<DataSommetSimple>
    {
        // Key variables
        private System.Random random;
        private int[][] directionsDiscretes; // ensemble des vecteurs des directions
        private int indexDirectionMoyenne; // la direction dans laquelle on se dirige
        private int deltaIndexMax; // plus ce nombre est élevé plus le tracé est sinueux;
        private int ordreGraphe; // nombre de sommets

        private TabRepartition<int> distancesInterSommets;
        private TabRepartition<int> tailleSommet;

        // build
        public DirectionMoyenne()
        {
            random = new Random();
            deltaIndexMax = 2;
            directionsDiscretes = IntUtils.Directions2D(deltaIndexMax);
            indexDirectionMoyenne = random.Next(0, directionsDiscretes.Length);
            ordreGraphe = 8;
            tailleSommet = new TabRepartition<int>(IntUtils.Sequence(5, 9), new double[] { 0.15, 0.5, 0.90, 1 });
            distancesInterSommets = new TabRepartition<int>(IntUtils.Sequence(10, 20), "centreeArithmetique");
        }
        public DirectionMoyenne(DirectionMoyenne model)
        {
            random = model.random;
            directionsDiscretes = model.directionsDiscretes;
            ordreGraphe = model.ordreGraphe;
            indexDirectionMoyenne = model.indexDirectionMoyenne;
            tailleSommet = model.tailleSommet;
            distancesInterSommets = model.distancesInterSommets;
        }
        public DirectionMoyenne(Random random, int[][] directionsDiscretes, int ordreGraphe, int indexDirectionMoyenne, TabRepartition<int> tailleSommet, TabRepartition<int> distancesInterSommets)
        {
            this.random = random;
            this.directionsDiscretes = directionsDiscretes;
            this.ordreGraphe = ordreGraphe;
            this.indexDirectionMoyenne = indexDirectionMoyenne;
            this.tailleSommet = tailleSommet;
            this.distancesInterSommets = distancesInterSommets;
        }

        // get
        public Random Random() { return random; }
        public int[][] DirectionsDiscretes() { return directionsDiscretes; }
        public int IndexDirectionMoyenne() { return indexDirectionMoyenne; }
        public int DeltaIndexMax() { return deltaIndexMax; }
        public int OrdreGraphe() { return ordreGraphe; }
        public TabRepartition<int> DistancesInterSommets() { return distancesInterSommets; }
        public TabRepartition<int> TailleSommet() { return tailleSommet; }

        // set
        public void SetRandom(Random value) { random = value; }
        public void SetDirectionsDiscretes(int[][] value) { directionsDiscretes = value; }
        public void SetIndexDirectionMoyenne(int value) { indexDirectionMoyenne = value; }
        public void SetDeltaIndexMax(int value) { deltaIndexMax = value; }
        public void SetOrdreGraphe(int value) { ordreGraphe = value; }
        public void SetDistancesInterSommets(TabRepartition<int> value) { distancesInterSommets = value; }
        public void SetTailleSommet(TabRepartition<int> value) { tailleSommet = value; }

        // Generateur
        public Graphe<DataSommetSimple> GenerateGraphe()
        {
            // Initialisation
            Graphe<DataSommetSimple> graphe = new Graphe<DataSommetSimple>();

            // Loi de répartition des directions
            int premierIndex = indexDirectionMoyenne - deltaIndexMax;
            int dernierIndex = indexDirectionMoyenne + deltaIndexMax;
            int[][] dirs = TableauUtils.SousTableau(directionsDiscretes, premierIndex, dernierIndex + 1);
            TabRepartition<int[]> direction = new TabRepartition<int[]>(dirs, "centreeArithmetique");

            // Loi de répartition des distances inter-sommets
            int[] tableauDistances = IntUtils.Sequence(10, 20);
            TabRepartition<int> distance = new TabRepartition<int>(tableauDistances, "centreeArithmetique");

            // Sommet initial
            graphe.AjouteSommet(new DataSommetSimple(
                new PaireInt(0, 0),
                tailleSommet.Observation()));

            // Ajoute les sommets les uns à la suite des autres
            for (int i = 1; i < ordreGraphe; i++)
            {
                // direction et distance
                Paire<double> dir = new PaireInt(direction.Observation()).Unitaire();
                int dist = distance.Observation();
                // Calcule la position
                PaireInt nextPosition = PaireInt.Somme(
                    graphe.DonneeSommet(i - 1).Position(),
                    new PaireInt(NumUtils.ArrondiEntier(dist * dir.X()), NumUtils.ArrondiEntier(dist * dir.Y())));
                // Ajoute le sommet
                graphe.AjouteSommet(new DataSommetSimple(nextPosition, tailleSommet.Observation()));
                // Cree une arrete entre le nouveau sommet et le precedent
                graphe.CreeArrete(i - 1, i);
            }

            return graphe;
        }
    }

    public class Circulaire : SimpleProcedure<DataSommetSimple>
    {
        // Key variables
        private Random random;
        private int[][] directionsDiscretes; // ensemble des vecteurs des directions

        int indexDirectionMoyenne;
        private int ordreGraphe; // nombre de sommets

        private TabRepartition<int> distanceSommetCentre;
        private TabRepartition<int> tailleSommet;

        // build
        public Circulaire()
        {
            random = new Random();
            directionsDiscretes = IntUtils.Directions2D(2);
            indexDirectionMoyenne = random.Next(0, directionsDiscretes.Length);
            ordreGraphe = 8;
            tailleSommet = new TabRepartition<int>(IntUtils.Sequence(5, 9), new double[] { 0.15, 0.5, 0.90, 1 });
            distanceSommetCentre = new TabRepartition<int>(IntUtils.Sequence(20, 10, -1), "geometrique");
        }
        public Circulaire(Circulaire model)
        {
            random = model.random;
            directionsDiscretes = model.directionsDiscretes;
            ordreGraphe = model.ordreGraphe;
            indexDirectionMoyenne = model.indexDirectionMoyenne;
            tailleSommet = model.tailleSommet;
            distanceSommetCentre = model.distanceSommetCentre;
        }
        public Circulaire(Random random, int[][] directionsDiscretes, int ordreGraphe, int indexDirectionMoyenne, TabRepartition<int> tailleSommet, TabRepartition<int> distancesInterSommets)
        {
            this.random = random;
            this.directionsDiscretes = directionsDiscretes;
            this.ordreGraphe = ordreGraphe;
            this.indexDirectionMoyenne = indexDirectionMoyenne;
            this.tailleSommet = tailleSommet;
            this.distanceSommetCentre = distancesInterSommets;
        }

        // set

        // Generateur
        public Graphe<DataSommetSimple> GenerateGraphe()
        {
            // Initialisation
            Graphe<DataSommetSimple> graphe = new Graphe<DataSommetSimple>();

            // Choix des directions
            bool[] directions = ordreGraphe > directionsDiscretes.Length ? BoolUtils.GetArrayOfTrue(directionsDiscretes.Length) : StatUtils.RandomKparmisN(ordreGraphe, directionsDiscretes.Length);

            // Ajout des sommets
            for (int indexDir = 0; indexDir < directionsDiscretes.Length; indexDir++)
            {
                // Verifie si il y a un sommet dans cette direction
                if (!directions[indexDir]) continue;

                // Position du sommet
                Paire<double> dir = new PaireInt(directionsDiscretes[indexDir]).Unitaire();
                int distCentre = distanceSommetCentre.Observation();
                PaireInt pos = Paire<double>.Multiplication(dir, (double)distCentre);

                // Ajout sommet
                graphe.AjouteSommet(new DataSommetSimple(pos, tailleSommet.Observation()));

                // Creer une arrete
                int indiceSommet = graphe.Ordre() - 1;
                if (indiceSommet > 0) { graphe.CreeArrete(indiceSommet - 1, indiceSommet); }
            }

            // Creation d'arrete entre le premier et le dernier sommet
            graphe.CreeArrete(0, graphe.Ordre() - 1);

            return graphe;
        }
    }

    // Blueprints, Tilemaps
    public static class Utils
    {
        /// <summary>
        /// Cree le Blueprint d'une chambre.
        /// Le blueprint est un carre (de valeurs 0) de cote "taille" avec une bordure intérieure (de valeurs 1)
        /// La position fait que le carre est centré sur le repere.
        /// </summary>
        /// <param name="taille">La taille souhaitée pour cette chambre</param>
        /// <returns>Le Blueprint representant la chambre</returns>
        public static Blueprint BlueprintChambre(DataSommetSimple sommet)
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

        /// <summary>
        /// Cree le Blueprint d'un couloir.
        /// Le couloir se tient entre deux sommets
        /// </summary>
        /// <param name="sommetA">A</param>
        /// <param name="sommetB">B</param>
        /// <returns></returns>
        public static Blueprint BlueprintCouloir(DataSommetSimple sommetA, DataSommetSimple sommetB)
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
        /// La largeur minimum est de 4, son maximum est 7, et elle est inférieur à la taille du plus petit sommet adjecent.
        /// </summary>
        /// <param name="tailleSommet1">La taille du premier sommet adjacent.</param>
        /// <param name="tailleSommet2">La taille du deuxième sommet adjacent.</param>
        /// <returns>La largeur du couloir.</returns>
        public static int LargeurCouloir(int tailleSommet1, int tailleSommet2)
        {
            int LargeurCouloir = IntUtils.Min(new int[] { tailleSommet1, tailleSommet2, 7 }) - 2;
            LargeurCouloir = IntUtils.Max(new int[] { LargeurCouloir, 4 });
            return LargeurCouloir;
        }

        // Gestion des tilemaps


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
        public static void GrapheToTilemaps(Tilemap[] tilemaps, TileBase[] tiles,Graphe<DataSommetSimple> graphe)
        {
            // Parcours de chaque sommet du graphe pour générer les chambres
            for (int indexSommet = 0; indexSommet < graphe.Ordre(); indexSommet++)
            {
                // Blueprint de la chambre
                Blueprint chambre = BlueprintChambre(graphe.DonneeSommet(indexSommet));
                // Ajout aux tilemaps
                TilemapUtils.InsertToTilemaps(tilemaps, tiles, chambre, chambre, insertionBehavior: "min");
            }


            // Parcours de chaque arrete du graphe pour générer les couloirs
            foreach (PaireInt arrete in graphe.Arretes())
            {
                // Blueprint du couloir
                Blueprint couloir = BlueprintCouloir(graphe.DonneeSommet(arrete[0]), graphe.DonneeSommet(arrete[1]));
                // Ajout sur le Blueprint global
                TilemapUtils.InsertToTilemaps(tilemaps, tiles, couloir, couloir, insertionBehavior: "min");
            }

        }
    }
}