using System;
using System.Text;
using System.Collections.Generic;
using Random = System.Random;
using CamBib.Class;
using CamBib.Fonctions;
using CamBibUnity;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace SommetArrete
{
    public interface Procedure<DataSommet, DataArrete> where DataSommet : DataSommetAbstraite
    {
        public abstract Graphe<DataSommet, DataArrete> GenerateGraphe();
    }

    // Classe contenant les information des sommets
    public class DataSommetStandard : DataSommetAbstraite
    {
        // Variables
        private int seed;
        private PaireInt position;
        private MaDict<string, Paire<int>> contenu; // {"GameObject","Position"}
        private int taille;
        private List<Blueprint> blueprints;

        // Build
        public DataSommetStandard()
        {
            seed = 0;
            position = new PaireInt();
            contenu = new MaDict<string, Paire<int>>();
            taille = -1;
            blueprints = null;
        }
        public DataSommetStandard(DataSommetStandard model)
        {
            seed = model.seed;
            position = new PaireInt(model.position);
            contenu = new MaDict<string, Paire<int>>(model.contenu);
            taille = model.taille;
            // Copie profonde pour blueprints
            if (model.blueprints != null)
            {
                foreach (Blueprint blueprint in model.blueprints)
                {
                    blueprints.Add(new Blueprint(blueprint)); 
                }
            } else { blueprints = null; }
            
        }
        public DataSommetStandard(int seed, PaireInt position, MaDict<string, Paire<int>> contenu, int taille)
        {
            this.seed = seed;
            this.position = position;
            this.contenu = contenu;
            this.taille = taille;
            blueprints = null;
        }

        // Get
        public int Seed() { return seed; }
        public PaireInt Position() { return position; }
        public MaDict<string, Paire<int>> Contenu() { return contenu; }
        public int Taille() { return taille; }
        public List<Blueprint> Blueprints() { return blueprints; }

        // Set
        public void SetSeed(int valeur) { seed = valeur; }
        public void SetPosition(PaireInt paire) { position = paire; }
        public void SetContenu(MaDict<string, Paire<int>> valeurs) { contenu = valeurs; }
        public void AddObjet(string tag, PaireInt position) { contenu.Ajoute(tag, position); }
        public void SetTaille(int value) { taille = value; }
        public void SetBlueprints(List<Blueprint> valeurs) { blueprints = valeurs; }

        // To String
        public override String ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine($"seed:{seed} taille:{taille}");
            builder.AppendLine("position x: " + position[0] + " y: " + position[1]);
            builder.AppendLine("Contenu: " + contenu);
            builder.AppendLine(blueprints[0].ToString());
            return builder.ToString();
        }
    }

    // Classe contenant les informations des arretes
    public class DataArreteStandard
    {
        // Variables
        private int seed;
        private DataSommetStandard sommetA;
        private DataSommetStandard sommetB;
        private int largeur;
        private Paire<int>[] points;

        // Build
        public DataArreteStandard()
        {
            seed = 0;
            sommetA = null;
            sommetB = null;
            largeur = -1;
            points = null;
        }
        public DataArreteStandard(DataArreteStandard model)
        {
            seed = model.seed;
            sommetA = model.sommetA;
            sommetB = model.sommetB;
            largeur = model.largeur;
            points = model.points;
        }
        public DataArreteStandard(int seed, DataSommetStandard sommetA, DataSommetStandard sommetB, int largeur, Paire<int>[] points)
        {
            this.seed = seed;
            this.sommetA = sommetA;
            this.sommetB = sommetB;
            this.largeur = largeur;
            this.points = points;
        }

        // Get
        public int Seed() { return seed; }
        public DataSommetStandard SommetA() { return sommetA; }
        public DataSommetStandard SommetB() { return sommetB; }
        public DataSommetStandard[] Sommets() { return new DataSommetStandard[] { sommetA, sommetB }; }
        public int Largeur() { return largeur; }
        public Paire<int>[] Points() { return points; }

        // Set
        public void SetSeed(int valeur) { seed = valeur; }
        public void SetSommetA(DataSommetStandard sommet) { sommetA = sommet; }
        public void SetSommetB(DataSommetStandard sommet) { sommetB = sommet; }
        public void SetSommets(DataSommetStandard[] sommets) { sommetA = sommets[0]; sommetB = sommets[1]; }
        public void SetLargeur(int value) { largeur = value; }

        // To String
        public override String ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine($"sommetA: {sommetA}");
            builder.AppendLine($"sommetB: {sommetB}");
            builder.AppendLine($"seed: {seed}; largeur: {largeur}");
            return builder.ToString();
        }
    }

    // Classes Generatrices
    public class DirectionPrecedente : Procedure<DataSommetStandard, DataArreteStandard>
    {
        // Key variables
        private Random random;
        private int[][] directionsDiscretes; // ensemble des vecteurs des directions

        private int ordreGraphe; // nombre de sommets
        private int deltaIndexNextDirectionMax; // plus ce parametre est grand plus c'est sinueux

        private TabRepartition<int> distancesInterSommets;
        private TabRepartition<int> tailleSommet;

        // build
        public DirectionPrecedente(int seed = 0)
        {
            random = new Random(seed);
            directionsDiscretes = IntUtils.Directions2D(2);
            ordreGraphe = 8;
            deltaIndexNextDirectionMax = 3;
            tailleSommet = new TabRepartition<int>(IntUtils.Sequence(5, 9), new double[] { 0.15, 0.5, 0.90, 1 }, random);
            distancesInterSommets = new TabRepartition<int>(IntUtils.Sequence(10, 25), "geometrique", random);
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

        // get

        // set
        public void SetRandom(Random random) { this.random = random; }
        public void SetRandom(int seed) { random = new Random(seed); }
        public void SetDirectionsDiscretes(int[][] directions) { directionsDiscretes = directions; }
        public void SetDeltaIndexMax(int delta) { deltaIndexNextDirectionMax = delta; }

        // Generateur
        public Graphe<DataSommetStandard, DataArreteStandard> GenerateGraphe()
        {
            //// Initialisation
            
            Graphe<DataSommetStandard, DataArreteStandard> graphe = new Graphe<DataSommetStandard, DataArreteStandard>();
            // Direction initiale
            int indexDirection = random.Next(0, directionsDiscretes.Length); 
            // Loi de répartition de l'écart avec l'indice de la direction suivante
            int[] deltaIndexs = IntUtils.Sequence(-deltaIndexNextDirectionMax, deltaIndexNextDirectionMax + 1);
            TabRepartition<int> deltaIndexNextDirection = new TabRepartition<int>(deltaIndexs, "geometrique", random);

            // Sommet initial
            PaireInt position = new PaireInt(0,0);
            MaDict<string, Paire<int>> contenu = new MaDict<string, Paire<int>>();
            contenu.Ajoute("Player", position);
            int taille = tailleSommet.Observation();
            DataSommetStandard donneeSommet = new DataSommetStandard(random.Next(), position, contenu, taille) ;
            donneeSommet.SetBlueprints(Utils.BlueprintSommet(donneeSommet));
            graphe.AjouteSommet(donneeSommet);

            // Ajoute les sommets les uns à la suite des autres et les arrètes
            for (int i = 1; i < ordreGraphe; i++)
            {
                int seedSommet = random.Next();
                // index nouvelle direction
                indexDirection += deltaIndexNextDirection.Observation();
                // direction et distance
                Paire<double> dir = new PaireInt(TableauUtils.Valeur(directionsDiscretes, indexDirection)).Unitaire();
                int dist = distancesInterSommets.Observation();
                // Calcule la position
                PaireInt nextPosition = PaireInt.Somme(
                    graphe.DonneeSommet(i - 1).Position(),
                    new PaireInt(NumUtils.ArrondiEntier(dist * dir.X()), NumUtils.ArrondiEntier(dist * dir.Y())));
                // cree les donnees
                contenu = new MaDict<string, Paire<int>>();
                contenu.Ajoute("Enemy", null);
                donneeSommet = new DataSommetStandard(seedSommet, nextPosition, contenu, tailleSommet.Observation());
                donneeSommet.SetBlueprints(Utils.BlueprintSommet(donneeSommet));

                graphe.AjouteSommet(donneeSommet);

                // Cree une arrete entre le nouveau sommet et le precedent
                // cree les donnees
                int seedArrete = random.Next(); 
                DataSommetStandard sommetA = donneeSommet, sommetB = graphe.DonneeSommet(i - 1);
                int largeur = NumUtils.Max(IntUtils.Min(new int[] { sommetA.Taille(), sommetB.Taille(), 7 }) - 2, 2 );
                float courbure =  dist < 15 ? 0.3f : dist < 25 ? 2f : 5f;
                Paire<int>[] points = Utils.PointsArrete(seedArrete, sommetA, sommetB, courbure: courbure);
                DataArreteStandard donnee = new DataArreteStandard(seedArrete, sommetA, sommetB, largeur, points);
                // cree arrete
                graphe.CreeArrete(i - 1, i, donnee);
            }

            //// Layering des sommets
            for (int i = 0; i < graphe.Ordre(); i++)
            {
                Utils.LayerSommet(graphe, graphe.DonneeSommet(i));
            }

            // Ajoute le WeaponContainer dans un des sommets
            DataSommetStandard sommetChoisi = graphe.DonneeSommet(random.Next(0, graphe.Ordre()));
            sommetChoisi.AddObjet("Weapon", PaireInt.Somme(sommetChoisi.Position(), new PaireInt(-1, -1)));
            // Ajoute le Medikit dans un des sommets
            sommetChoisi = graphe.DonneeSommet(random.Next(0, graphe.Ordre()));
            sommetChoisi.AddObjet("Medikit", PaireInt.Somme(sommetChoisi.Position(), new PaireInt(-1, 1)));

            return graphe;
        }
    }

    // Blueprints, Tilemaps
    public static class Utils
    {
        /// <summary>
        /// Calcule les points d'une courbe reliant deux sommets (repérés par leurs positions)
        /// Ce sera un trait si on met le nombre de perturbations à 0.
        /// </summary>
        /// <param name="seed">La graine utilisée pour la génération de la courbe</param>
        /// <param name="sommetA">Le premier sommet</param>
        /// <param name="sommetB">Le second sommet</param>
        /// <param name="nombrePerturbations">Le nombre de sinusoidales que l'on va ajouter au trait initial pour le courber. Chaque perturbation à une prériode et une amplitude divisée par deux. Par default vaut 5</param>
        /// <param name="courbure">Facteur de chaque perturbation. Par defaut, vaut 1</param>
        /// <returns>Le tableau des positions successives reliant les deux points par une courbe</returns>
        public static Paire<int>[] PointsArrete(int seed, DataSommetStandard sommetA, DataSommetStandard sommetB, int nombrePerturbations = 5, float courbure = 1)
        {
            // Recuperation donnees
            Random random = new Random(seed);

            // Tracé général
            bool flip = false;
            if (Math.Abs(sommetB.Position()[0] - sommetA.Position()[0]) < Math.Abs(sommetB.Position()[1] - sommetA.Position()[1]))
            {
                // Echange les axes pour la construction
                PaireInt temp = new PaireInt(sommetB.Position());
                sommetB.Position()[0] = temp[1];
                sommetB.Position()[1] = temp[0];
                temp = new PaireInt(sommetA.Position());
                sommetA.Position()[0] = temp[1];
                sommetA.Position()[1] = temp[0];
                flip = true;
            }
            Paire<double>[] coordonnees = MathUtils.CoordonneesDiscretesLigne((Paire<double>)sommetA.Position(), (Paire<double>)sommetB.Position());
            double[] abscisses = PaireUtils.GetValeurs0(coordonnees);
            double[][] matriceOrdonnees = new double[1 + nombrePerturbations][]; matriceOrdonnees[0] = PaireUtils.GetValeurs1(coordonnees);
            for (int p = 0; p < nombrePerturbations; p++)
            {
                double periode = Paire<double>.DistanceEuclidienne(sommetA.Position(), sommetB.Position()) / Math.Pow(2, p - 1);
                int sgn = (random.Next(0, 2) * 2) - 1;
                double amplitude = sgn * courbure * Math.Pow(0.5, p);
                double[] perturbation = PaireUtils.GetValeurs1(MathUtils.CoordonneesDiscretesSinusoidale(0, sommetB.Position()[0] - sommetA.Position()[0], periode: periode));
                matriceOrdonnees[1 + p] = NumUtils.Multiplie(perturbation, sgn * amplitude);
            }
            double[] ordonnees = NumUtils.SommesColonnes(MatriceUtils.ConvertirJaggedEnRectangulaire(matriceOrdonnees));
            coordonnees = PaireUtils.Build(abscisses, ordonnees);
            Paire<int>[] points = DessinUtils.PointsCourbe(coordonnees);

            if (flip)
            {
                PaireInt temp = new PaireInt(sommetB.Position());
                sommetB.Position()[0] = temp[1];
                sommetB.Position()[1] = temp[0];
                temp = new PaireInt(sommetA.Position());
                sommetA.Position()[0] = temp[1];
                sommetA.Position()[1] = temp[0];
                PaireUtils.InverseValeurs(ref points);
            }

            return points;
        }

        /// <summary>
        /// Construit le blueprint d'un sommet d'un graphe en fonction de ses données
        /// </summary>
        /// <param name="donnee">Les donnees du sommet</param>
        /// <returns>Le blueprint du sommet</returns>
        public static List<Blueprint> BlueprintSommet(DataSommetStandard donnee)
        {
            List<Blueprint> blueprints = new List<Blueprint>();
            // Recuperation donnees
            PaireInt positionSommet = donnee.Position();
            int taille = donnee.Taille();// taille est la largeur interne de la salle
            // Bottom
            blueprints.Add(new Blueprint(taille, position: PaireInt.Somme(positionSommet, new PaireInt(-taille / 2, -taille / 2))));
            // Limits
            blueprints.Add(Blueprint.Box(taille+2, value: 1, position: PaireInt.Somme(positionSommet, new PaireInt(-(taille + 2) / 2, -(taille + 2) / 2))));
            // Props
            /*ajouter script pour le décor*/

            return blueprints;
        }

        /// <summary>
        /// Ajoute des détails dans la construction du sommet en fonction de ses données mais aussi des données des éléments voisins (ex : arrètes).
        /// Cette fonction peut placer des gameObjects et modifier les blueprints pour agencer l'espace. 
        /// </summary>
        /// <param name="donnee"></param>
        /// <returns></returns>
        public static void LayerSommet(Graphe<DataSommetStandard, DataArreteStandard> graphe, DataSommetStandard donnee)
        {
            Random random = new Random(donnee.Seed());
            
            //// Matrice de remplissabilité (donnant une valeur à chaque case du blueprint pour savoir si on peut le remplir)
            
            // Récupérations des tracés des arretes voisines
            List<Paire<int>[]> chemins = new List<Paire<int>[]>();
            DataArreteStandard[] donneeArretesAdjacentes = graphe.DonneeArretes(graphe.SommetIndex(donnee.SommetID()));
            Debug.Log("Sommet:" + graphe.SommetIndex(donnee.SommetID()));
            foreach (DataArreteStandard donneeArrete in donneeArretesAdjacentes) { chemins.Add(donneeArrete.Points()); }
            // Calcul des intersections des tracés des arrètes et du blueprint du sommet (Les entrées de la salle)
            Paire<int>[] entrees = new Paire<int>[chemins.Count];
            for ( int indexChemin = 0; indexChemin < chemins.Count; indexChemin++ )
            {
                Paire<int>[] chemin = chemins[indexChemin];
                double distanceExtremite0 = Paire<int>.DistanceEuclidienne(donnee.Position(), chemin[0]);
                double distanceExtremite1 = Paire<int>.DistanceEuclidienne(donnee.Position(), chemin[^1]);
                int index = distanceExtremite0 < distanceExtremite1 ? 0 : chemin.Length - 1;
                int increment = index == 0 ? 1 : -1;

                int x = chemin[index].X();
                int y = chemin[index].Y();
                while (donnee.Blueprints()[0].IsValidPosition(x, y) && (donnee.Blueprints()[0].Valeur(x,y) >= 0)) 
                { 
                    index += increment;
                    x = chemin[index].X();
                    y = chemin[index].Y();
                }

                entrees[indexChemin] = new Paire<int>(x,y); //Ajt la position trouvée
            }
            
            // Calcul de la matrice de remplissabilité
            Blueprint remplissabilite = new Blueprint(donnee.Blueprints()[0]);
            int valeurInobstruable = int.MinValue; // definit les cases sur lesquelles le joueur doit pouvoir passer
            int valeurPrefab = -2; // definit les cases sur lesquelles on place un prefab

            Paire<double> centroid;
            if (entrees.Length == 1)
            {
                centroid = (Paire<double>)entrees[0];
            } 
            else if (entrees.Length > 1)
            {
                // Etablissement des chemins inobstruables liants les entrées de la salle
                centroid = MathUtils.Centroide(Paire<int>.ConvertToArrayPaireDoubles(entrees));
                for (int i = 0; i < entrees.Length; i++)
                {
                    PaireInt dimensions = Paire<int>.Soustraction(entrees[i], centroid);
                    Blueprint cheminInobstruable = Blueprint.Courbe(dimensions, new Blueprint(1, 1, valeurInobstruable));
                    cheminInobstruable.SetPosition(PaireInt.Somme(cheminInobstruable.Position(), centroid));
                    remplissabilite.InsertToBlueprint(cheminInobstruable);
                }
            } 
            else 
            { throw new Exception("Il n'y à pas d'entrée dans cette salle. Celà veut dire qu'elle n'est pas connectée au reste du niveau."); }
            

            // Etablissement de la valeur de remplissabilité de chaque case du blueprint (distance euclidienne au centroid)
            int x0 = remplissabilite.Position()[0];
            int y0 = remplissabilite.Position()[1];
            for (int x = 0; x < remplissabilite.Width(); x++)
            {
                for (int y = 0; y < remplissabilite.Height(); y++)
                {
                    if (remplissabilite[x0+x,y0+y] != valeurInobstruable && remplissabilite[x0 + x, y0 + y] != -1)
                    {
                        remplissabilite[x0 + x, y0 + y] = (int)Paire<double>.DistanceEuclidienne(centroid, new Paire<double>(x0 + x, y0 + y)); // Il faut créer une matrice de double pour stocker les doubles ou permetre cela dans les blueprints
                    }
                }
            }

            Debug.Log("remplissabilite: " + remplissabilite);
            
            //// Macrice d'agencement (elle décrit la manière dont est agencée la salle)
            
            Blueprint agencement = new Blueprint(donnee.Blueprints()[0]);
            List<Paire<int>> positionsLibres;

            // Ajout des obstacles (valeur d'identification : 1)
            int max = NumUtils.Max(remplissabilite.Matrice());
            
            if(max > 0)
            {
                
                positionsLibres = MatriceUtils.Indices(remplissabilite.Matrice(), max);
                int nombreObstacles = random.Next((positionsLibres.Count/2)+1); // environ 1 pour 2 espaces libres
                
                for (int i = 0; i < nombreObstacles; i++)
                {
                    int positionIndex = random.Next(positionsLibres.Count);
                    agencement[PaireInt.Somme(agencement.Position(),positionsLibres[positionIndex])] = 1;
                    positionsLibres.RemoveAt(positionIndex);
                }
            }

            // Remplissage des cases de valeur moyenne avec "caisses"

            // Remplissage du chemin avec des "ennemis"

            positionsLibres = remplissabilite.IndicesMin();
            int nbEnnemis = donnee.Contenu().RetireTous("Enemy");
            Debug.Log("nbEnnemis:" + nbEnnemis);
            Debug.Log("contenu sans ennemis:" + donnee.Contenu());
            for (int i = 0; i < nbEnnemis; i++)
            {
                Paire<int> position = Paire<int>.Somme(agencement.Position(), positionsLibres[random.Next(positionsLibres.Count)]);
                Debug.Log("position:" + Paire<int>.Somme(position, donnee.Position()));
                agencement[position] = valeurPrefab;
                donnee.Contenu().Ajoute("Enemy", position);
            }
            Debug.Log("contenu :" + donnee.Contenu());
            Debug.Log("agencement: " + agencement);
            
            return;
        }

        /// <summary>
        /// Construit le blueprint d'une arrete d'un graphe en fonction de ses données
        /// </summary>
        /// <param name="donnee">Les donnees de l'arrete</param>
        /// <returns>Le blueprint de l'arrete</returns>
        public static List<Blueprint> BlueprintArrete(DataArreteStandard donnee)
        {
            List<Blueprint> blueprints = new List<Blueprint>();
            Blueprint blueprint;
            Blueprint elementaire;


            // Recuperation donnees
            int largeur = donnee.Largeur();
            Paire<int>[] points = donnee.Points();

            // Bottom
            elementaire = new Blueprint(largeur);
            elementaire.SetPosition(new PaireInt(-largeur / 2, -largeur / 2));
            blueprint = new Blueprint();
            blueprint.InsertToBlueprint(elementaire, points);
            blueprints.Add(blueprint);
            // Limits
            largeur += 2;
            elementaire = Blueprint.Box(largeur, value: 1, borderWidth: 1);
            elementaire.SetPosition(new PaireInt(-(largeur) / 2, -(largeur) / 2));
            blueprint = new Blueprint();
            blueprint.InsertToBlueprint(elementaire, points);
            blueprints.Add(blueprint);
            // Props
            /*script pour les bonus, décors et points de spawn des entitées*/



                return blueprints;
        }

        /// <summary>
        /// Converti le graphe en un niveau dans la scene sur Unity
        /// </summary>
        /// <param name="tilemaps">Les tilemaps de la scène</param>
        /// <param name="tiles">Les tileBases disponibles pour remplir les tilemaps</param>
        /// <param name="objets">Les Prefabs des GameObjects à instanctier</param>
        /// <param name="graphe">Le graphe contenant les informations pour remplir la scène</param>
        /// <param name="randomSeed">La graine de Random à utiliser pour la génération</param>
        public static void GrapheToScene(Grid grid, Tilemap[] tilemaps, TileBase[] tiles, MaDict<string, GameObject> objets, Graphe<DataSommetStandard, DataArreteStandard> graphe, int? randomSeed = null)
        {
            if (!randomSeed.HasValue) { randomSeed = (int)DateTime.Now.Ticks; }

            // Parcours de chaque arrete du graphe pour générer les couloirs
            foreach (DataArreteStandard donneeArrete in graphe.DonneeArretes())
            {
                // Blueprint du couloir
                List<Blueprint> blueprints = BlueprintArrete(donneeArrete);
                // Ajout sur le Blueprint global
                TilemapUtils.InsertToTilemaps(tilemaps, tiles, blueprints);
            }
            
            // Parcours de chaque sommet du graphe pour générer les chambres
            for (int indexSommet = 0; indexSommet < graphe.Ordre(); indexSommet++)
            {
                DataSommetStandard donnee = graphe.DonneeSommet(indexSommet);
                // Ajout des prefabs
                List<Tuple<GameObject, Paire<int>>> contenuConverti = new List<Tuple<GameObject, Paire<int>>>();

                foreach (string clef in donnee.Contenu().Clefs())
                { 
                    Paire<int> position = donnee.Contenu().Valeur(clef);

                    if (clef == "Player")
                    {
                        GameObject player = GameObject.FindGameObjectWithTag("Player");
                        player.transform.position = new Vector3Int(position.X(), position.Y(), 0);
                        continue;
                    }
                    else
                    {
                        var tuple = Tuple.Create(objets.Valeur(clef), position);
                        contenuConverti.Add(tuple);
                    }

                }

                PrefabUtils.InstanciePrefabs(contenuConverti.ToArray(), grid);

                // Construction des tilemaps
                TilemapUtils.InsertToTilemaps(tilemaps, tiles, donnee.Blueprints());
            }
            
        }
    }

}
