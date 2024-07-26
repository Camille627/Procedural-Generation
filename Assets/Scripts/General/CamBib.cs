// Author : Camille ANSEL
// Creation : 05-2024
// Last Update : 07-2024

using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using Random = System.Random;

using UnityEngine; // Pour debug uniquement

using CamBib.Fonctions;
using CamBib.Class;


/// <summary>
/// Toutes les Fonctions et Classes générales de Camille ANSEL.
/// Ne pas utiliser "using Unity.engine;" hormis pour la commande "Debug.Log();" (qui est très pratique ;))
/// </summary>
namespace CamBib
{
    // Fonctions pour tout faire
    namespace Fonctions
    {
        /// <summary>
        /// Pour faire des statistiques et des calculs de probabilités
        /// </summary>
        public static class StatUtils
        {
            /// <summary>
            /// Génère une répartition de valeurs selon une méthode spécifiée. 
            /// La somme des valeurs du vecteur retourné vaut 1.
            /// </summary>
            /// <param name="nombreValeurs">Le nombre de valeurs à générer.</param>
            /// <param name="methode">Le type de répartition à utiliser. Peut être "uniforme", "geometrique", "arithmetique", ou "centreeArithmetique". La valeur par défaut est "uniforme".</param>
            /// <param name="parametre">Le paramètre supplémentaire pour certaines répartitions. Utilisé uniquement pour le type "geometrique". Doit être compris entre 0 et 1. La valeur par défaut est 0.</param>
            /// <returns>Un tableau de doubles représentant la répartition des valeurs.</returns>
            /// <exception cref="ArgumentException">Levée si le paramètre pour la répartition géométrique est hors des limites [0, 1].</exception>
            public static double[] Repartition(int nombreValeurs, string methode = "uniforme", float parametre = 0)
            {
                if (nombreValeurs <= 0)
                {
                    throw new ArgumentException("Le nombre de valeurs doit être supérieur à 0.", nameof(nombreValeurs));
                }

                double[] repartition = new double[nombreValeurs];
                System.Random random = new System.Random();

                switch (methode.ToLower())
                {
                    case "uniforme":
                        for (int i = 0; i < nombreValeurs; i++)
                        {
                            repartition[i] = 1d / nombreValeurs;
                        }
                        break;

                    case "geometrique":
                        if (parametre < 0 || parametre > 1)
                        {
                            throw new ArgumentException("Pour une répartition géométrique, le paramètre doit être compris entre 0 et 1.", nameof(parametre));
                        }
                        double reste = 1;
                        for (int i = 0; i < nombreValeurs; i++)
                        {
                            repartition[i] = reste * parametre;
                            reste *= (1 - parametre);
                        }
                        for (int i = 0; i < nombreValeurs; i++)
                        {
                            repartition[i] += reste / nombreValeurs;
                        }
                        break;

                    case "arithmetique":
                        double somme = 0;
                        for (int i = 0; i < nombreValeurs; i++)
                        {
                            repartition[i] = i + 1;
                            somme += i + 1;
                        }
                        for (int i = 0; i < nombreValeurs; i++)
                        {
                            repartition[i] /= somme;
                        }
                        break;

                    case "centreearithmetique":
                        somme = 0;
                        int milieu = nombreValeurs / 2;
                        // Première moitié du tablea
                        for (int i = 0; i < milieu; i++)
                        {
                            repartition[i] = i + 1;
                            somme += i + 1;
                        }
                        // Seconde moitié du tableau
                        for (int i = milieu; i < nombreValeurs; i++)
                        {
                            repartition[i] = repartition[nombreValeurs - 1 - i];
                            somme += repartition[nombreValeurs - 1 - i];
                        }
                        // Cas impair
                        if (nombreValeurs % 2 == 1)
                        {
                            somme -= repartition[milieu];
                        }
                        // Approche la somme des valeurs de 1
                        for (int i = 0; i < nombreValeurs; i++)
                        {
                            repartition[i] = NumUtils.Arrondi(repartition[i] / somme, 8);
                        }
                        break;

                    default:
                        throw new ArgumentException($"Méthode de répartition '{methode}' inconnue.", nameof(methode));
                }

                // Ajouter le reste à une valeur aléatoire
                int indexAleatoire = random.Next(nombreValeurs);
                repartition[indexAleatoire] += 1d - NumUtils.Somme(repartition);

                return repartition;
            }

            /// <summary>
            /// Normalise un tableau d'entiers de sorte que la somme de ses valeurs soit égale à 1.
            /// </summary>
            /// <param name="parts">Tableau d'entiers à normaliser.</param>
            /// <returns>Tableau de doubles où la somme des éléments est égale à 1.</returns>
            public static double[] Repartition(int[] parts)
            {
                // Calcul de la somme des valeurs du tableau
                int somme = 0;
                for (int i = 0; i < parts.Length; i++) { somme += parts[i]; }

                // Normalisation des valeurs du tableau
                double[] result = new double[parts.Length];
                double total = 0;
                for (int i = 0; i < parts.Length; i++) { result[i] = (double)parts[i] / somme; total += result[i]; }

                // Ajustement du reste pour assurer que la somme est exactement 1
                double difference = 1 - total;
                if (parts.Length > 0 && difference != 0)
                {
                    result[0] += difference;
                }

                return result;
            }

            /// <summary>
            /// Generates a random sample of boolean values with a specified number of true values.
            /// </summary>
            /// <param name="populationSize">The total number of elements in the population.</param>
            /// <param name="sampleSize">The number of true values to generate.</param>
            /// <param name="seed">An optional seed value for the random number generator. If not provided, a random seed is used.</param>
            /// <returns>An array of boolean values representing the random sample.</returns>
            /// <exception cref="ArgumentException">Thrown when the sample size is greater than the population size.</exception>
            public static bool[] RandomKparmisN(int sampleSize, int populationSize, int? seed = null)
            {
                if (sampleSize > populationSize)
                {
                    throw new ArgumentException("Sample size cannot be greater than population size.");
                }

                bool[] result = new bool[populationSize];
                System.Random rand = seed.HasValue ? new System.Random(seed.Value) : new System.Random();
                int trueCount = 0;

                while (trueCount < sampleSize)
                {
                    int index = rand.Next(populationSize);

                    if (!result[index])
                    {
                        result[index] = true;
                        trueCount++;
                    }
                }

                return result;
            }
        }

        /// <summary>
        /// Usages spécifiques mais variés pour des calculs mathématiques
        /// </summary>
        public static class MathUtils
        {
            /// <summary>
            /// Compte le nombre d'entiers entre deux nombres décimaux.
            /// </summary>
            /// <param name="start">Le nombre décimal de départ.</param>
            /// <param name="end">Le nombre décimal de fin.</param>
            /// <returns>Le nombre d'entiers entre start et end, inclusivement.</returns>
            public static int CompteEntiersEntre(double start, double end)
            {
                // Start est le plus petit des deux nombres
                if (start > end)
                {
                    double temp = end;
                    end = start;
                    start = temp;
                }

                // Arrondir le nombre de départ à l'entier supérieur
                int lowerBound = (int)Math.Ceiling(start);

                // Arrondir le nombre de fin à l'entier inférieur
                int upperBound = (int)Math.Floor(end);

                // Calculer le nombre d'entiers entre les deux bornes
                int count = upperBound - lowerBound + 1;
                // Si le résultat est négatif, cela signifie qu'il n'y a pas d'entiers entre les deux nombres
                if (count < 0)
                {
                    count = 0;
                }

                return count;
            }

            
            /// <summary>
            /// Calcule les coordonnees des points abscisses entières d'une ligne tracée entre deux points.
            /// </summary>
            /// <param name="start">le premier point</param>
            /// <param name="end">le second point</param>
            /// <returns>le tableau des coordonnées des points de la ligne entre les points A et B</returns>
            public static Paire<double>[] CoordonneesDiscretesLigne(Paire<double> start, Paire<double> end)
            {
                // Cas abscisses egales (retourne l'ordonnee la plus grande)
                if (start.X() == end.X())
                {
                    Paire<double> coordonnee = start.Y() > end.Y() ? start : end;
                    return new Paire<double>[] { new Paire<double>(coordonnee) }; // Copie par sureté
                }

                // Choisir le point de départ avec l'abscisse la plus petite
                if (start.X() > end.X())
                {
                    Paire<double> temp = end;
                    end = start;
                    start = temp;
                }

                // Initialisation
                int longueur = CompteEntiersEntre(start.X(), end.X());
                Paire<double>[] coordonnees = new Paire<double>[longueur];

                Paire<double> direction = Paire<double>.Soustraction(end, start);

                ////  Remplissage du tableau
                int x0 = (int)Math.Ceiling(start.X());
                for (int i = 0; i < longueur; i++)
                {
                    int x = x0 + i;
                    Paire<double> coordonnee = new Paire<double>();
                    coordonnee[0] = x;
                    coordonnee[1] = start.Y() + (x - start.X()) * direction.Y() / direction.X(); // calcul de l'ordonnee sur la droite suivant les coordonnees choisies
                    coordonnees[i] = coordonnee;
                }
                return coordonnees;
            }

            /// <summary>
            /// Calcule les ordonnéea des points d'abscisses entières d'un sinus entre deux abscisses.
            /// </summary>
            /// <param name="x1">la première abscisse</param>
            /// <param name="x2">la seconde abscisse</param>
            /// <returns>le tableau des coordonnées des points de la courbe sinusoidale entre les abscisses</returns>
            public static Paire<double>[] CoordonneesDiscretesSinusoidale(double x1, double x2, double? frequence = null, double periode = 2*Math.PI, double dephasage = 0)
            {
                // Cas abscisses egales (retourne l'ordonnee la plus grande)
                if (x1 == x2)
                {
                    return new Paire<double>[] { new Paire<double>(x1, Math.Sin(x1)) };
                }

                // Choisir le point de départ avec l'abscisse la plus petite
                if (x1 > x2)
                {
                    double temp = x2;
                    x2 = x1;
                    x1 = temp;
                }

                
                // Initialisation
                int longueur = CompteEntiersEntre(x1, x2);
                Paire<double>[] coordonnees = new Paire<double>[longueur];

                double freq = frequence.HasValue ? frequence.Value : 2 * Math.PI / periode;
                int x0 = (int)Math.Ceiling(x1);

                ////  Remplissage du tableau
                for (int i = 0; i < longueur; i++)
                {
                    int x = x0 + i;
                    Paire<double> coordonnee = new Paire<double>();
                    coordonnee[0] = x;
                    coordonnee[1] = Math.Sin(freq * x + dephasage);
                    coordonnees[i] = coordonnee;
                }

                return coordonnees;
            }

            /// <summary>
            /// Calcule la somme d'une suite finie de sinusoïdes à un abscisse en tenant compte de différents paramètres.
            /// Renseigner "random" permet d'obtenir des courbes aléatoires.
            /// </summary>
            /// <param name="x">L'abscissez à laquelle calculer la somme.</param>
            /// <param name="nombreSinusoides">Le nombre de sinusoïdes dont on va calculer la somme.</param>
            /// <param name="facteurAttenuation">Le facteur d'atténuation/amplification des sinusoïdes.</param>
            /// <param name="periodeInitiale">La période totale utilisée pour calculer les périodes (en divisant par deux pour chaque sinusoïde).</param>
            /// <param name="randomSeed">Si elle est renseigné, le signe de l'amplitude de chaque sinusoïde sera déterminé aléatoirement</param>
            /// <returns>La valeur calculée.</returns>
            public static double SommeSinusoidales(int x, int nombreSinusoides, int periodeInitiale, float facteurAttenuation = 1, int? randomSeed = null)
            {
                Random random = randomSeed.HasValue ? new Random() : new Random(randomSeed.Value);

                double somme = 0;

                for (int i = 0; i < nombreSinusoides; i++)
                {
                    double periode = periodeInitiale / Math.Pow(2, i);
                    double frequence = (2 * Math.PI / periode);
                    double amplitude = Math.Pow(facteurAttenuation, i);

                    if (randomSeed.HasValue) { int sgn = (random.Next(0, 2) * 2) - 1; amplitude *= sgn; }

                    somme += amplitude * Math.Sin(frequence * x);
                }

                return somme;
            }

            /// <summary>
            /// Calcule le centroïde d'un nuage de points
            /// </summary>
            /// <param name="points">le nuage de points sous forme d'un tableau de paires</param>
            /// <returns>la paire contenant les coordonnées du centroïde</returns>
            public static Paire<double> Centroide(Paire<double>[] points)
            {
                Paire<double> centroid = new Paire<double>();

                double[] X = PaireUtils.GetValeurs0(points); 
                double[] Y = PaireUtils.GetValeurs1(points);
                int N = points.Length;

                centroid[0] = NumUtils.Somme(X) / N;
                centroid[1] = NumUtils.Somme(Y) / N;

                return centroid;
            }
        }

        /// <summary>
        /// Concernent les types int, float, double et decimal et leurs structures dérivées
        /// </summary>
        public static class NumUtils
        {
            /// <summary>
            /// Vérifie si le type spécifié est numérique (int, float, double ou decimal).
            /// Attention, toutes les fonctions n'ont pas encore été adaptées à tous les types
            /// </summary>
            /// <param name="type">Le type à vérifier.</param>
            /// <returns>True si le type est numérique ; sinon, False.</returns>
            public static bool IsNumerique<DataType>()
            {
                return typeof(DataType) == typeof(decimal) ||
                       typeof(DataType) == typeof(double) ||
                       typeof(DataType) == typeof(float) ||
                       typeof(DataType) == typeof(int);
            }

            /// <summary>
            /// Renvoie la plus grande des deux valeurs spécifiées.
            /// </summary>
            /// <typeparam name="T">Le type des valeurs comparées. Ce type doit implémenter IComparable<T>.</typeparam>
            /// <param name="a">La première des deux valeurs à comparer.</param>
            /// <param name="b">La seconde des deux valeurs à comparer.</param>
            /// <returns>La valeur maximale entre <paramref name="a"/> et <paramref name="b"/>.</returns>
            /// <exception cref="ArgumentNullException">Si l'un des arguments est null et que le type <typeparamref name="T"/> est une référence.</exception>
            public static T Max<T>(T a, T b) where T : IComparable<T>
            {
                if (a == null || b == null)
                {
                    throw new ArgumentNullException(a == null ? nameof(a) : nameof(b), "Les arguments ne peuvent pas être null.");
                }

                return a.CompareTo(b) > 0 ? a : b;
            }

            /// <summary>
            /// Renvoie la plus petite des deux valeurs spécifiées.
            /// </summary>
            /// <typeparam name="T">Le type des valeurs comparées. Ce type doit implémenter IComparable<T>.</typeparam>
            /// <param name="a">La première des deux valeurs à comparer.</param>
            /// <param name="b">La seconde des deux valeurs à comparer.</param>
            /// <returns>La valeur minimale entre <paramref name="a"/> et <paramref name="b"/>.</returns>
            /// <exception cref="ArgumentNullException">Si l'un des arguments est null et que le type <typeparamref name="T"/> est une référence.</exception>
            public static T Min<T>(T a, T b) where T : IComparable<T>
            {
                if (a == null || b == null)
                {
                    throw new ArgumentNullException(a == null ? nameof(a) : nameof(b), "Les arguments ne peuvent pas être null.");
                }
                return a.CompareTo(b) < 0 ? a : b;
            }

            /// <summary>
            /// Donne le signe du nombre passé en paramètre.
            /// </summary>
            /// <typeparam name="T">Le type des valeurs comparées. Ce type doit implémenter IComparable<T>.</typeparam>
            /// <param name="num">La valeur dont on cherche le signe.</param>
            /// <returns>Retourne 1 si <paramref name="num"/> est supérieur ou égal à 0, -1 sinon.</returns>
            /// <exception cref="ArgumentNullException">Si l'un des arguments est null et que le type <typeparamref name="T"/> est une référence.</exception>
            public static int Signe<T>(T num) where T : IComparable<T>
            {
                // Erreur
                if (num == null)
                {
                    throw new ArgumentNullException(nameof(num), "L'argument ne peut pas être null.");
                }
                // Initialise le zero
                T zero = default(T);
                if (zero == null)
                {
                    zero = (T)Convert.ChangeType(0, typeof(T));
                }

                return num.CompareTo(zero) >= 0 ? 1 : -1;
            }

            /// <summary>
            /// Arrondit un nombre à l'entier le plus proche.
            /// </summary>
            /// <typeparam name="T">Le type de nombre à arrondir. Doit être un type numérique.</typeparam>
            /// <param name="num">Le nombre à arrondir.</param>
            /// <returns>L'entier le plus proche de la valeur donnée.</returns>
            /// <exception cref="ArgumentNullException">Levée si l'argument <paramref name="num"/> est null.</exception>
            public static int ArrondiEntier<T>(T num) where T : IComparable<T>
            {
                // Erreur
                if (num == null)
                {
                    throw new ArgumentNullException(nameof(num), "L'argument ne peut pas être null.");
                }
                // Floor
                int floor = Convert.ToInt32(num);

                return Convert.ToDouble(num).CompareTo(floor + 0.5d) >= 0 ? floor + 1 : floor;
            }

            /// <summary>
            /// Arrondit un nombre à virgule flottante au nombre de chiffres significatifs spécifié.
            /// </summary>
            /// <param name="valeur">Le nombre à arrondir.</param>
            /// <param name="chiffresSignificatifs">Le nombre de chiffres significatifs pour l'arrondi.</param>
            /// <returns>Le nombre arrondi au nombre de chiffres significatifs spécifié.</returns>
            /// <exception cref="ArgumentException">Levée si le nombre de chiffres significatifs est négatif.</exception>
            public static float Arrondi(float valeur, int chiffresSignificatifs)
            {
                if (chiffresSignificatifs < 0)
                {
                    throw new ArgumentException("Le nombre de chiffres significatifs doit être positif.", nameof(chiffresSignificatifs));
                }

                // Calcule le coefficient multiplicateur pour le nombre de chiffres significatifs
                float coefficient = Mathf.Pow(10f, chiffresSignificatifs);

                // Multiplie la valeur par le coefficient pour déplacer la virgule
                float valeurMultipliee = valeur * coefficient;

                // Arrondit à l'entier le plus proche
                float valeurArrondie = ArrondiEntier(valeurMultipliee);

                // Divise par le coefficient pour ramener la virgule à sa position d'origine
                return valeurArrondie / coefficient;
            }
            public static double Arrondi(double valeur, int chiffresSignificatifs)
            {
                if (chiffresSignificatifs < 0)
                {
                    throw new ArgumentException("Le nombre de chiffres significatifs doit être positif.", nameof(chiffresSignificatifs));
                }

                // Calcule le coefficient multiplicateur pour le nombre de chiffres significatifs
                double coefficient = Math.Pow(10, chiffresSignificatifs);

                // Multiplie la valeur par le coefficient pour déplacer la virgule
                double valeurMultipliee = valeur * coefficient;

                // Arrondit à l'entier le plus proche
                double valeurArrondie = ArrondiEntier(valeurMultipliee);

                // Divise par le coefficient pour ramener la virgule à sa position d'origine
                return valeurArrondie / coefficient;
            }

            /// <summary>
            /// Calculates the distance (absolute difference) between two numbers of the same numeric type.
            /// </summary>
            /// <typeparam name="T">The numeric type of the parameters (int, double, float, etc.).</typeparam>
            /// <param name="a">The first number.</param>
            /// <param name="b">The second number.</param>
            /// <returns>The absolute difference between the two numbers as a double.</returns>
            public static double Distance<T>(T a, T b) where T : IComparable, IConvertible
            {
                double da = Convert.ToDouble(a);
                double db = Convert.ToDouble(b);
                return Math.Abs(da - db);
            }

            // Avec les Tableaux

            /// <summary>
            /// Calcule la somme des valeurs d'un tableau.
            /// </summary>
            /// <param name="tableau">Le tableau d'entrée.</param>
            /// <returns>La somme des valeurs du tableau.</returns>
            public static int Somme(int[] tableau)
            {
                int somme = 0;
                for (int i = 0; i < tableau.Length; i++)
                {
                    somme += tableau[i];
                }
                return somme;
            }
            public static float Somme(float[] tableau)
            {
                float somme = 0;
                for (int i = 0; i < tableau.Length; i++)
                {
                    somme += tableau[i];
                }
                return somme;
            }
            public static double Somme(double[] tableau)
            {
                double somme = 0;
                for (int i = 0; i < tableau.Length; i++)
                {
                    somme += tableau[i];
                }
                return somme;
            }
            /// <summary>
            /// Additionne deux vecteurs terme à terme.
            /// </summary>
            /// <param name="tableau1">Le premier vecteur d'entrée.</param>
            /// <param name="tableau2">Le deuxième vecteur d'entrée.</param>
            /// <returns>Un nouveau vecteur où chaque élément est la somme des éléments correspondants des deux vecteurs d'entrée.</returns>
            public static int[] Additionne(int[] tableau1, int[] tableau2)
            {
                if (tableau1.Length != tableau2.Length)
                {
                    throw new ArgumentException("Les vecteurs doivent avoir la même longueur");
                }

                int[] resultat = new int[tableau1.Length];
                for (int i = 0; i < tableau1.Length; i++)
                {
                    resultat[i] = tableau1[i] + tableau2[i];
                }
                return resultat;
            }
            public static float[] Additionne(float[] vecteur1, float[] vecteur2)
            {
                if (vecteur1.Length != vecteur2.Length)
                {
                    throw new ArgumentException("Les vecteurs doivent avoir la même longueur");
                }

                float[] resultat = new float[vecteur1.Length];
                for (int i = 0; i < vecteur1.Length; i++)
                {
                    resultat[i] = vecteur1[i] + vecteur2[i];
                }
                return resultat;
            }
            public static double[] Additionne(double[] vecteur1, double[] vecteur2)
            {
                if (vecteur1.Length != vecteur2.Length)
                {
                    throw new ArgumentException("Les vecteurs doivent avoir la même longueur");
                }

                double[] resultat = new double[vecteur1.Length];
                for (int i = 0; i < vecteur1.Length; i++)
                {
                    resultat[i] = vecteur1[i] + vecteur2[i];
                }
                return resultat;
            }
            /// <summary>
            /// Additionne un scalaire à chaque élément d'un tableau.
            /// </summary>
            /// <param name="tableau">Le tableau d'entrée.</param>
            /// <param name="scalaire">Le scalaire à ajouter à chaque élément du tableau.</param>
            /// <returns>Un nouveau tableau où chaque élément du tableau d'entrée a été additionné au scalaire.</returns>
            public static int[] Additionne(int[] tableau, int scalaire)
            {
                int[] resultat = new int[tableau.Length];
                for (int i = 0; i < tableau.Length; i++)
                {
                    resultat[i] = tableau[i] + scalaire;
                }
                return resultat;
            }
            public static float[] Additionne(float[] tableau, float scalaire)
            {
                float[] resultat = new float[tableau.Length];
                for (int i = 0; i < tableau.Length; i++)
                {
                    resultat[i] = tableau[i] + scalaire;
                }
                return resultat;
            }
            public static double[] Additionne(double[] tableau, double scalaire)
            {
                double[] resultat = new double[tableau.Length];
                for (int i = 0; i < tableau.Length; i++)
                {
                    resultat[i] = tableau[i] + scalaire;
                }
                return resultat;
            }

            /// <summary>
            /// Soustrait chaque élément du deuxième tableau du premier tableau.
            /// </summary>
            /// <param name="tableau1">Le premier tableau d'entrée.</param>
            /// <param name="tableau2">Le deuxième tableau d'entrée.</param>
            /// <returns>Un nouveau tableau où chaque élément est la différence des éléments correspondants des deux tableaux d'entrée.</returns>
            public static int[] Soustrait(int[] tableau1, int[] tableau2)
            {
                if (tableau1.Length != tableau2.Length)
                {
                    throw new ArgumentException("Les tableaux doivent avoir la même longueur");
                }

                int[] resultat = new int[tableau1.Length];
                for (int i = 0; i < tableau1.Length; i++)
                {
                    resultat[i] = tableau1[i] - tableau2[i];
                }
                return resultat;
            }
            public static float[] Soustrait(float[] tableau1, float[] tableau2)
            {
                if (tableau1.Length != tableau2.Length)
                {
                    throw new ArgumentException("Les tableaux doivent avoir la même longueur");
                }

                float[] resultat = new float[tableau1.Length];
                for (int i = 0; i < tableau1.Length; i++)
                {
                    resultat[i] = tableau1[i] - tableau2[i];
                }
                return resultat;
            }
            public static double[] Soustrait(double[] tableau1, double[] tableau2)
            {
                if (tableau1.Length != tableau2.Length)
                {
                    throw new ArgumentException("Les tableaux doivent avoir la même longueur");
                }

                double[] resultat = new double[tableau1.Length];
                for (int i = 0; i < tableau1.Length; i++)
                {
                    resultat[i] = tableau1[i] - tableau2[i];
                }
                return resultat;
            }
            /// <summary>
            /// Soustrait un scalaire à chaque élément d'un tableau.
            /// </summary>
            /// <param name="tableau">Le tableau d'entrée.</param>
            /// <param name="scalaire">Le scalaire à soustraire à chaque élément du tableau.</param>
            /// <returns>Un nouveau tableau où chaque élément du tableau d'entrée a été soustrait du scalaire.</returns>
            public static int[] Soustrait(int[] tableau, int scalaire)
            {
                int[] resultat = new int[tableau.Length];
                for (int i = 0; i < tableau.Length; i++)
                {
                    resultat[i] = tableau[i] - scalaire;
                }
                return resultat;
            }
            public static float[] Soustrait(float[] tableau, float scalaire)
            {
                float[] resultat = new float[tableau.Length];
                for (int i = 0; i < tableau.Length; i++)
                {
                    resultat[i] = tableau[i] - scalaire;
                }
                return resultat;
            }
            public static double[] Soustrait(double[] tableau, double scalaire)
            {
                double[] resultat = new double[tableau.Length];
                for (int i = 0; i < tableau.Length; i++)
                {
                    resultat[i] = tableau[i] - scalaire;
                }
                return resultat;
            }

            /// <summary>
            /// Multiplie les termes d'un tableau par un scalaire
            /// </summary>
            /// <returns></returns>
            public static double[] Multiplie(double[] tableau, double scalaire)
            {
                return tableau.Select(terme => terme * scalaire).ToArray();
            }

            /// <summary>
            /// Calcule les sommes cumulées des éléments d'un tableau.
            /// </summary>
            /// <param name="tableau">Le tableau d'entrée contenant les éléments à cumuler.</param>
            /// <returns>Un tableau contenant les sommes cumulées des éléments.</returns>
            public static int[] Cumule(int[] tableau)
            {
                int[] cumul = new int[tableau.Length];

                int termeI;
                for (int i = 0; i < tableau.Length; i++)
                {
                    termeI = 0;
                    for (int j = 0; j <= i; j++)
                    {
                        termeI += tableau[j];
                    }
                    cumul[i] = termeI;
                }

                return cumul;
            }
            public static float[] Cumule(float[] tableau)
            {
                float[] cumul = new float[tableau.Length];

                float termeI;
                for (int i = 0; i < tableau.Length; i++)
                {
                    termeI = 0;
                    for (int j = 0; j <= i; j++)
                    {
                        termeI += tableau[j];
                    }
                    cumul[i] = termeI;
                }

                return cumul;
            }
            public static double[] Cumule(double[] tableau)
            {
                double[] cumul = new double[tableau.Length];

                double termeI;
                for (int i = 0; i < tableau.Length; i++)
                {
                    termeI = 0;
                    for (int j = 0; j <= i; j++)
                    {
                        termeI += tableau[j];
                    }
                    cumul[i] = termeI;
                }

                return cumul;
            }

            // Avec les Matrices Bidimensionnelles
            /// <summary>
            /// Calcule les sommes des colonnes
            /// </summary>
            /// <param name="matrice">La matrice dont on veut calculer les sommes des termes de chaque colonne</param>
            /// <returns>Un tableau des sommes de chaque colonne</returns>
            public static double[] SommesColonnes(double[,] matrice)
            {
                double[] sommesColonnes = new double[matrice.GetLength(1)];
                for (int j = 0; j < matrice.GetLength(1); j++)
                {
                    double sommeColonne = 0;
                    for (int i = 0; i < matrice.GetLength(0); i++)
                    {
                        sommeColonne += matrice[i,j];
                    }
                    sommesColonnes[j] = sommeColonne;
                }
                return sommesColonnes;
            }
            
            /// <summary>
            /// Trouve l'élément maximal dans une matrice bidimensionnelle.
            /// </summary>
            /// <typeparam name="T">Le type des éléments dans la matrice. Doit implémenter <see cref="IComparable{T}"/>.</typeparam>
            /// <param name="matrice">La matrice bidimensionnelle dont on veut trouver l'élément maximal.</param>
            /// <returns>L'élément maximal de la matrice.</returns>
            /// <exception cref="ArgumentException">Lancé si la matrice est nulle ou vide.</exception>
            /// <example>
            /// Voici un exemple d'utilisation de la méthode <c>Max</c> :
            ///
            /// <code>
            /// int[,] intMatrix = {
            ///     { 1, 2, 3 },
            ///     { 4, 5, 6 },
            ///     { 7, 8, 9 }
            /// };
            ///
            /// string[,] stringMatrix = {
            ///     { "apple", "orange", "banana" },
            ///     { "cherry", "mango", "grape" }
            /// };
            ///
            /// Console.WriteLine("Max value in intMatrix: " + Max(intMatrix)); // Output: 9
            /// Console.WriteLine("Max value in stringMatrix: " + Max(stringMatrix)); // Output: orange
            /// </code>
            /// </example>
            public static T Max<T>(this T[,] matrice) where T : IComparable<T>
            {
                if (matrice == null || matrice.Length == 0)
                {
                    throw new ArgumentException("La matrice ne doit pas être vide.");
                }

                T max = matrice[0, 0];
                foreach (T element in matrice)
                {
                    if (element.CompareTo(max) > 0) // cas où element > max
                    {
                        max = element;
                    }
                }
                return max;
            }

            /// <summary>
            /// Trouve l'élément minimal dans une matrice bidimensionnelle.
            /// </summary>
            /// <typeparam name="T">Le type des éléments dans la matrice. Doit implémenter <see cref="IComparable{T}"/>.</typeparam>
            /// <param name="matrice">La matrice bidimensionnelle dont on veut trouver l'élément minimal.</param>
            /// <returns>L'élément minimal de la matrice.</returns>
            /// <exception cref="ArgumentException">Lancé si la matrice est nulle ou vide.</exception>
            /// <example>
            /// Voici un exemple d'utilisation de la méthode <c>Min</c> :
            ///
            /// <code>
            /// int[,] intMatrix = {
            ///     { 1, 2, 3 },
            ///     { 4, 5, 6 },
            ///     { 7, 8, 9 }
            /// };
            ///
            /// string[,] stringMatrix = {
            ///     { "apple", "orange", "banana" },
            ///     { "cherry", "mango", "grape" }
            /// };
            ///
            /// Console.WriteLine("Min value in intMatrix: " + Min(intMatrix)); // Output: 1
            /// Console.WriteLine("Min value in stringMatrix: " + Min(stringMatrix)); // Output: apple
            /// </code>
            /// </example>
            public static T Min<T>(this T[,] matrice) where T : IComparable<T>
            {
                if (matrice == null || matrice.Length == 0)
                {
                    throw new ArgumentException("La matrice ne doit pas être vide.");
                }

                T min = matrice[0, 0];
                foreach (T element in matrice)
                {
                    if (element.CompareTo(min) < 0)
                    {
                        min = element;
                    }
                }
                return min;
            }

            /// <summary>
            /// Donne les indices des cellules contenant la valeur maximale de la matrice
            /// </summary>
            /// <typeparam name="T">Le type des éléments dans la matrice. Doit implémenter <see cref="IComparable{T}"/>.</typeparam>
            /// <param name="matrice">La matrice</param>
            /// <returns>Liste d'est paires (ligne, colonne) des indices des cellules contenant la valeur maximale de la matrice</returns>
            public static List<Paire<int>> IndicesMax<T>(this T[,] matrice) where T : IComparable<T>, IEquatable<T> { return MatriceUtils.Indices(matrice, Max(matrice));}

            /// <summary>
            /// Donne les indices des cellules contenant la valeur minimale de la matrice
            /// </summary>
            /// <typeparam name="T">Le type des éléments dans la matrice. Doit implémenter <see cref="IComparable{T}"/>.</typeparam>
            /// <param name="matrice">La matrice</param>
            /// <returns>Liste d'est paires (ligne, colonne) des indices des cellules contenant la valeur minimale de la matrice</returns>
            public static List<Paire<int>> IndicesMin<T>(this T[,] matrice) where T : IComparable<T>, IEquatable<T> { return MatriceUtils.Indices(matrice, Min(matrice)); }
        }

        /// <summary>
        /// Travailler avec les int (et leurs tableaux)
        /// </summary>
        public static class IntUtils
        {
            /// <summary>
            /// Trouve la valeur maximale dans un tableau d'entiers.
            /// </summary>
            /// <param name="tableau">Le tableau d'entiers dans lequel chercher la valeur maximale.</param>
            /// <returns>La valeur maximale dans le tableau.</returns>
            public static int Max(int[] tableau)
            {
                int max = int.MinValue;
                foreach (int i in tableau) { max = max < i ? i : max; }

                return max;
            }

            /// <summary>
            /// Trouve la valeur minimale dans un tableau d'entiers.
            /// </summary>
            /// <param name="tableau">Le tableau d'entiers dans lequel chercher la valeur minimale.</param>
            /// <returns>La valeur minimale dans le tableau.</returns>
            public static int Min(int[] tableau)
            {
                int min = int.MaxValue;
                foreach (int i in tableau) { min = min > i ? i : min; }

                return min;
            }

            /// <summary>
            /// Crée et renvoie une sequence d'entiers
            /// </summary>
            /// <param name="start">La première valeur du vecteur</param>
            /// <param name="limit">La valeur limite du vecteur</param>
            /// <param name="step">Le pas entre deux valeurs. Vaut 1 par défaut</param>
            /// <returns>La sequence d'entiers entre start(inclu) et limit(exclu)</returns>
            public static int[] Sequence(int start, int? limit = null, int? step = 1, int? length = null)
            {
                // Erreur
                if (!limit.HasValue && !length.HasValue)
                {
                    throw new ArgumentException("Argument manquant. Renseignez soit limit, soit length, soit les deux", "limit and length");
                }

                if (limit.HasValue && !(Math.Sign(limit.Value - start) == Math.Sign(step.Value)))
                {
                    throw new ArgumentException("Signe incorrect. Impossible de construire la sequence.", "step");
                }

                int[] maSequence;
                if (length.HasValue)
                {

                    maSequence = new int[length.Value];
                }
                else
                {
                    int longueurSequence = 1 + ((Math.Abs(limit.Value - start) - 1) / Math.Abs(step.Value));
                    maSequence = new int[longueurSequence];
                }

                int i = 0;
                int next = start;
                bool cont;
                do
                {
                    maSequence[i] = next;
                    i++;
                    next = maSequence[i - 1] + step.Value;

                    if (length.HasValue && i < length.Value) { cont = true; }
                    else if (limit.HasValue && Math.Sign(step.Value) > 0 && next < limit.Value) { cont = true; }
                    else if (limit.HasValue && Math.Sign(step.Value) < 0 && next > limit.Value) { cont = true; }
                    else { cont = false; }

                } while (cont);
                return maSequence;
            }

            /// <summary>
            /// Calcule la norme Euclidienne d'un vecteur d'entiers
            /// </summary>
            /// <param name="vecteur">Le vecteur dont on souaite calculer la norme</param>
            /// <returns>La valeur de la norme euclidienne du vecteur.</returns>
            public static double NormeEucli(int[] vecteur)
            {
                double norme = 0;
                for (int k = 0; k < vecteur.Length; k++)
                {
                    norme += vecteur[k] * vecteur[k];
                }
                norme = Math.Sqrt(norme);

                return norme;
            }

            /// <summary>
            /// Calcule le vecteur unitaire correspondant au vecteur d'entiers spécifié.
            /// </summary>
            /// <param name="vector">Le vecteur d'entiers.</param>
            /// <returns>Le vecteur unitaire correspondant.</returns>
            public static double[] Unitaire(int[] vecteur)
            {
                // Initialisation
                double[] unitaire = new double[vecteur.Length];
                double longueur = NormeEucli(vecteur);
                // Calcul des valeurs du vecteur unitaire
                for (int k = 0; k < vecteur.Length; k++)
                {
                    unitaire[k] = vecteur[k] / longueur;
                }
                return unitaire;
            }

            /// <summary>
            /// Génère un tableau de directions basé sur une dimension maximale donnée.
            /// </summary>
            /// <param name="dimMax">La dimension maximale pour déterminer les directions. Il s'agit de la distance maximale à laquelle les directions seront calculées par rapport à l'origine. Ce paramètre doit être un entier positif.</param>
            /// <returns>Un tableau 2D d'entiers représentant les coordonnées des directions. La taille de ce tableau est 8 * dimMax, chaque élément étant un tableau de deux entiers représentant les coordonnées (x, y).</returns>
            public static int[][] Directions2D(int dimMax)
            {
                int[][] directions = new int[8 * dimMax][];
                int i = 0;
                for (int x = 1 - dimMax; x <= dimMax; x++) { directions[i] = new int[] { x, dimMax }; i++; }
                for (int y = dimMax - 1; y >= -dimMax; y--) { directions[i] = new int[] { dimMax, y }; i++; }
                for (int x = dimMax - 1; x >= -dimMax; x--) { directions[i] = new int[] { x, -dimMax }; i++; }
                for (int y = 1 - dimMax; y <= dimMax; y++) { directions[i] = new int[] { -dimMax, y }; i++; }
                return directions;
            }
        }

        /// <summary>
        /// Travailler avec les booléens (et leurs tableaux)
        /// </summary>
        public static class BoolUtils
        {
            /// <summary>
            /// Generates a one-dimensional array of boolean values where all elements are set to true.
            /// </summary>
            /// <param name="size">The size of the array.</param>
            /// <returns>An array of boolean values with all elements set to true.</returns>
            public static bool[] GetArrayOfTrue(int size)
            {
                bool[] result = new bool[size];

                for (int i = 0; i < size; i++)
                {
                    result[i] = true;
                }

                return result;
            }
            /// <summary>
            /// Generates a two-dimensional matrix of boolean values where all elements are set to true.
            /// </summary>
            /// <param name="rows">The number of rows in the matrix.</param>
            /// <param name="columns">The number of columns in the matrix.</param>
            /// <returns>A two-dimensional matrix of boolean values with all elements set to true.</returns>
            public static bool[,] GetArrayOfTrue(int rows, int columns)
            {
                bool[,] result = new bool[rows, columns];

                for (int i = 0; i < rows; i++)
                {
                    for (int j = 0; j < columns; j++)
                    {
                        result[i, j] = true;
                    }
                }

                return result;
            }

            /// <summary>
            /// Generates a one-dimensional array of boolean values where all elements are set to false.
            /// </summary>
            /// <param name="size">The size of the array.</param>
            /// <returns>An array of boolean values with all elements set to false.</returns>
            public static bool[] GetArrayOfFalse(int size)
            {
                bool[] result = new bool[size];

                for (int i = 0; i < size; i++)
                {
                    result[i] = false;
                }

                return result;
            }
            /// <summary>
            /// Generates a two-dimensional matrix of boolean values where all elements are set to false.
            /// </summary>
            /// <param name="rows">The number of rows in the matrix.</param>
            /// <param name="columns">The number of columns in the matrix.</param>
            /// <returns>A two-dimensional matrix of boolean values with all elements set to false.</returns>
            public static bool[,] GetArrayOfFalse(int rows, int columns)
            {
                bool[,] result = new bool[rows, columns];

                for (int i = 0; i < rows; i++)
                {
                    for (int j = 0; j < columns; j++)
                    {
                        result[i, j] = false;
                    }
                }

                return result;
            }
        }

        /// <summary>
        /// Pour les structures dérivées des "Paires"
        /// </summary>
        public static class PaireUtils
        {
            public static string ToString<DataType>(Paire<DataType>[] paires) where DataType : struct
            {
                if (paires == null)
                {
                    return "null";
                }

                string[] elements = new string[paires.Length];
                for (int i = 0; i < paires.Length; i++)
                {
                    elements[i] = paires[i].ToString();
                }

                return "[" + string.Join(", ", elements) + "]";
            }

            /// <summary>
            /// Creer un tableau de paire à partir de deux tableau de valeurs
            /// </summary>
            /// <typeparam name="DataType">Le type des tableau</typeparam>
            /// <param name="valeurs0">Le tableau des valeurs0 des Paires</param>
            /// <param name="valeurs1">Le tableau des valeurs1 des Paires</param>
            /// <returns>Un tableau de paires contenant les valeurs 0 et 1 correspondantes</returns>
            public static Paire<DataType>[] Build<DataType>(DataType[] valeurs0, DataType[] valeurs1) where DataType : struct
            {
                if (valeurs0.Length != valeurs1.Length) { throw new Exception("Les tableau doivents êtres de même taille"); }

                Paire<DataType>[] paires = new Paire<DataType>[valeurs0.Length];
                for (int i = 0; i < valeurs0.Length; i++)
                {
                    paires[i] = new Paire<DataType>(valeurs0[i], valeurs1[i]);
                }

                return paires;
            }

            /// <summary>
            /// Recupérer toutes les valeurs 0 d'un tableau de paires
            /// </summary>
            /// <typeparam name="DataType">Le type de données des paires</typeparam>
            /// <param name="paires">Le tableau de paires</param>
            /// <returns>Le tableau contenant toutes les valeurs 0 des Paires</returns>
            public static DataType[] GetValeurs0<DataType>(Paire<DataType>[] paires) where DataType : struct
            {
                return paires.Select(p => p[0]).ToArray();
            }

            /// <summary>
            /// Recupérer toutes les valeurs 1 d'un tableau de paires
            /// </summary>
            /// <typeparam name="DataType">Le type de données des paires</typeparam>
            /// <param name="paires">Le tableau de paires</param>
            /// <returns>Le tableau contenant toutes les valeurs 0 des Paires</returns>
            public static DataType[] GetValeurs1<DataType>(Paire<DataType>[] paires) where DataType : struct
            {
                return paires.Select(p => p[1]).ToArray();
            }

            /// <summary>
            /// Echange les valeurs 0 et 1 de chaque Paire d'un tableau de paires
            /// </summary>
            /// <param name="paires">Le tableau dont on souhaite inverser les valeurs</param>
            public static void InverseValeurs<DataType>(ref Paire<DataType>[] paires) where DataType : struct
            {
                foreach (Paire<DataType> paire in paires)
                {
                    Paire<DataType> temp = new Paire<DataType>(paire);
                    paire[0] = temp[1];
                    paire[1] = temp[0];
                }
            }
        }

        /// <summary>
        /// Travailler sur des Tableau (aussi appelés array) de tous types
        /// </summary>
        public static class TableauUtils
        {
            /// <summary>
            /// Représente un tableau par un texte.
            /// </summary>
            /// <typeparam name="T">Le type de données du tableau</typeparam>
            /// <param name="matrice">Le tableau</param>
            /// <returns>String[] représentant le tableau</returns>
            public static string MaToString<T>(this T[] tableau)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine($"Array {typeof(T)} - length: {tableau.Length}");

                if (tableau != null)
                {
                    sb.AppendLine("{");

                    for (int i = 0; i < tableau.Length - 1; i++)
                    {
                        sb.AppendLine(tableau[i] + ", ");
                    }
                    sb.AppendLine(tableau[^1] + "}");
                }
                else
                {
                    sb.AppendLine("Array: null");
                }

                return sb.ToString();
            }

            /// <summary>
            /// Vérifie si une donnée appartient à un tableau
            /// </summary>
            /// <typeparam name="T">Le type de donnée avec lequel on travaille</typeparam>
            /// <param name="tableau">Le tableau de données dans lequel on va chercher</param>
            /// <param name="donnee">La donnée recherchée</param>
            /// <returns>true si la donnée est trouvée dans le tableau, false sinon</returns>
            public static bool Contient<T>(this T[] tableau, T valeur)
            {
                int i = 0;
                while (i < tableau.Length && !tableau[i].Equals(valeur)) { i++; }
                return i != tableau.Length && tableau[i].Equals(valeur);
            }

            /// <summary>
            /// Renvoie l'élément du tableau à l'index spécifié, en utilisant un index modulo la longueur du tableau.
            /// </summary>
            /// <typeparam name="T">Le type des éléments du tableau.</typeparam>
            /// <param name="tableau">Le tableau à partir duquel récupérer l'élément.</param>
            /// <param name="index">L'index de l'élément à récupérer. Si l'index est hors des limites du tableau, il est modulo avec la longueur du tableau.</param>
            /// <returns>L'élément du tableau à l'index spécifié.</returns>
            /// <exception cref="ArgumentNullException">Si le tableau est null.</exception>
            public static T Valeur<T>(this T[] tableau, int index)
            {
                while (index < 0) { index += tableau.Length; }
                return tableau[index % tableau.Length];
            }

            /// <summary>
            /// Copier un tableau en enlevant un élément
            /// </summary>
            /// <typeparam name="T">Le type de données du tableau</typeparam>
            /// <param name="tableau">Le tableau </param>
            /// <param name="index">L'index de l'élément à retirer du tableau</param>
            /// <returns>Un tableau contenant les mêmes éléments mais sans celui que l'on à retiré</returns>
            public static T[] Retire<T>(this T[] tableau, int index)
            {
                while (index < 0) { index += tableau.Length; }
                index = index % tableau.Length;

                return tableau.SousTableau(0, index).Concatene(tableau.SousTableau(index + 1, tableau.Length));
            }
            /// <summary>
            /// Copie un tableau en enlevant une valeur. Attention, il enlève la première occurence de cette valeur
            /// </summary>
            /// <typeparam name="T">Le type de données du tableau</typeparam>
            /// <param name="tableau">Le tableau </param>
            /// <param name="valeur">La valeur à retirer du tableau</param>
            /// <returns>Un tableau contenant les mêmes éléments mais sans celui que l'on à retiré</returns>
            public static T[] Retire<T>(this T[] tableau, T valeur)
            {
                int index = 0;
                while (index < tableau.Length && !tableau[index].Equals(valeur)) { index++; }
                if (index < tableau.Length)
                {
                    return tableau.Retire(index);
                }
                throw new Exception("La valeur n'est pas dans le tableau");
            }
            public static T[] Retire<T>(this T[] tableau, int[] indices)
            {
                // Tri des indices pour optimisation (facultatif)
                Array.Sort(indices);

                // Créez un nouveau tableau pour contenir les éléments restants
                T[] nouvTableau = new T[tableau.Length - indices.Length];
                int j = 0;
                int indicesIndex = 0;

                for (int i = 0; i < tableau.Length; i++)
                {
                    // Si l'indice actuel n'est pas dans la liste des indices à retirer
                    if (indicesIndex < indices.Length && i == indices[indicesIndex])
                    {
                        indicesIndex++;
                    }
                    else
                    {
                        nouvTableau[j] = tableau[i];
                        j++;
                    }
                }
                return nouvTableau;
            }

            /// <summary>
        /// Renvoie un sous-tableau contenant les éléments du tableau original compris entre les indices spécifiés,
        /// en utilisant des indices modulo la longueur du tableau.
        /// </summary>
        /// <typeparam name="T">Le type des éléments du tableau.</typeparam>
        /// <param name="tableau">Le tableau à partir duquel créer le sous-tableau.</param>
        /// <param name="indexPremier">L'index de début (inclus) du sous-tableau. Si l'index dépasse la longueur du tableau, il est modulo avec la longueur du tableau.</param>
        /// <param name="indexLimite">L'index de fin (exclus) du sous-tableau. Si l'index dépasse la longueur du tableau, il est modulo avec la longueur du tableau.</param>
        /// <returns>Un sous-tableau contenant les éléments du tableau original compris entre les indices spécifiés.</returns>
        /// <exception cref="ArgumentNullException">Si le tableau est null.</exception>
            public static T[] SousTableau<T>(this T[] tableau, int indexPremier, int indexLimite)
            {

                T[] sousTableau = new T[indexLimite - indexPremier];
                for (int i = indexPremier; i < indexLimite; i++)
                {
                    sousTableau[i - indexPremier] = Valeur(tableau, i);
                }
                return sousTableau;
            }
            
            public static T[] Redimensionne<T>(T[] tableau, int taille)
            {
                T[] nouveau = new T[taille];
                int i = 0;
                while (i < tableau.Length && i < taille)
                {
                    nouveau[i] = tableau[i];
                }
                return tableau;
            }

            /// <summary>
            /// Concatène deux tableaux en un seul tableau.
            /// </summary>
            /// <typeparam name="T">Le type des éléments des tableaux.</typeparam>
            /// <param name="tableau1">Le premier tableau à concaténer.</param>
            /// <param name="tableau2">Le deuxième tableau à concaténer.</param>
            /// <returns>Le tableau résultant contenant les éléments des deux tableaux d'entrée.</returns>
            public static T[] Concatene<T>(this T[] tableau1, T[] tableau2)
            {
                // Calculer la longueur totale du tableau résultant
                int nouvelleLongueur = tableau1.Length + tableau2.Length;

                // Créer un nouveau tableau avec la longueur calculée
                T[] tableauConcatene = new T[nouvelleLongueur];

                // Copier les éléments du premier tableau
                Array.Copy(tableau1, tableauConcatene, tableau1.Length);

                // Copier les éléments du deuxième tableau à partir de la fin du premier tableau
                Array.Copy(tableau2, 0, tableauConcatene, tableau1.Length, tableau2.Length);

                // Retourner le tableau concaténé
                return tableauConcatene;
            }

            /// <summary>
            /// Ajouter une valeur à la fin d'un tableau.
            /// </summary>
            /// <typeparam name="T">Type de donnée du tableau</typeparam>
            /// <param name="tableau">Tableau auquel on ajoute la valeur</param>
            /// <param name="valeur">La valeur à ajouter</param>
            /// <returns>Le tableau avec la valeur à la fin</returns>
            public static T[] Concatene<T>(T[] tableau, T valeur)
            {
                // Création d'un tableau plus long
                T[] nouveauTableau = new T[tableau.Length + 1];
                // Remplissage avec les valeurs du tableau de base
                for (int i = 0; i < tableau.Length; i++)
                {
                    nouveauTableau[i] = tableau[i];
                }
                // Ajout de la nouvelle valeur
                nouveauTableau[tableau.Length] = valeur;

                return nouveauTableau;
            }

        }

        /// <summary>
        /// Travailler sur les matrices (ou tableau de tableaux) quelque soient leurs types
        /// </summary>
        public static class MatriceUtils
        {
            /// <summary>
            /// Crée une copie profonde d'une matrice bidimensionnelle.
            /// </summary>
            /// <typeparam name="T">Le type des éléments de la matrice.</typeparam>
            /// <param name="matriceSource">La matrice bidimensionnelle à copier. Ne peut pas être null.</param>
            /// <returns>Une nouvelle matrice bidimensionnelle qui est une copie profonde de la matrice source.</returns>
            /// <exception cref="ArgumentNullException">Levée si <paramref name="matriceSource"/> est null.</exception>
            public static T[,] CopieProfonde<T>(T[,] matriceSource)
            {
                if (matriceSource == null)
                {
                    throw new ArgumentNullException(nameof(matriceSource), "La matrice source ne peut pas être null");
                }

                int rows = matriceSource.GetLength(0);
                int cols = matriceSource.GetLength(1);
                T[,] copie = new T[rows, cols];

                for (int i = 0; i < rows; i++)
                {
                    for (int j = 0; j < cols; j++)
                    {
                        copie[i, j] = matriceSource[i, j];
                    }
                }

                return copie;
            }
            /// <summary>
            /// Représente une matrice par un texte.
            /// </summary>
            /// <typeparam name="T">Le type de données de la matrice</typeparam>
            /// <param name="matrice">La matrice</param>
            /// <returns>String[] représentant la matrice</returns>
            public static string MaToString<T>(T[,] matrice)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine($"Matrix {typeof(T)}");
                sb.AppendLine($"Size: {matrice.GetLength(0)}x{matrice.GetLength(1)}");

                if (matrice != null)
                {
                    sb.AppendLine("Matrix [");
                    for (int i = 0; i < matrice.GetLength(0) - 1; i++)
                    {
                        sb.Append("[");
                        for (int j = 0; j < matrice.GetLength(1) - 1; j++)
                        {
                            sb.Append(matrice[i, j] + ", ");
                        }
                        sb.Append(matrice[i, matrice.GetLength(1) - 1]+"]");
                        sb.AppendLine();
                    }
                    sb.AppendLine("]");
                }
                else
                {
                    sb.AppendLine("Matrix: null");
                }

                return sb.ToString();
            }
            public static string MaToString<T>(T[][] matrice)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine($"{typeof(T)} Array - length: {matrice.Length}");

                if (matrice != null)
                {
                    sb.AppendLine("Array:");
                    for (int i = 0; i < matrice.Length - 1; i++)
                    {
                        for (int j = 0; j < matrice[0].Length - 1; j++)
                        {
                            sb.Append(matrice[i][j] + ", ");
                        }
                        sb.Append(matrice[i][matrice[0].Length - 1]);
                        sb.AppendLine();
                    }
                }
                else
                {
                    sb.AppendLine("Matrix: null");
                }

                return sb.ToString();
            }

            /// <summary>
            /// Redimensionne une matrice en créant une nouvelle matrice de dimensions spécifiées 
            /// et en copiant les éléments de la matrice originale dans la nouvelle matrice à partir d'une position donnée.
            /// </summary>
            /// <typeparam name="T">Le type des éléments de la matrice.</typeparam>
            /// <param name="matrice">La matrice originale à redimensionner.</param>
            /// <param name="longueur">La nouvelle longueur (nombre de lignes) de la matrice.</param>
            /// <param name="largeur">La nouvelle largeur (nombre de colonnes) de la matrice.</param>
            /// <param name="posX">La position de départ sur l'axe des X dans la nouvelle matrice (par défaut 0).</param>
            /// <param name="posY">La position de départ sur l'axe des Y dans la nouvelle matrice (par défaut 0).</param>
            /// <returns>Une nouvelle matrice de type <typeparamref name="T"/> avec les dimensions spécifiées, contenant les éléments de la matrice originale.</returns>
            /// <remarks>
            /// Si la position de départ (posX, posY) fait en sorte que des éléments de la matrice originale débordent
            /// en dehors des dimensions de la nouvelle matrice, seuls les éléments pouvant être copiés dans les limites
            /// de la nouvelle matrice seront inclus.
            /// </remarks>
            public static T[,] Redimensionne<T>(T[,] matrice, int longueur, int largeur, int posX = 0, int posY = 0)
            {
                T[,] nouvelleMatrice = new T[longueur, largeur];

                for (int i = 0; i < matrice.GetLength(0); i++)
                {
                    for (int j = 0; j < matrice.GetLength(1); j++)
                    {
                        int newI = i + posX;
                        int newJ = j + posY;

                        if (newI >= 0 && newI < longueur && newJ >= 0 && newJ < largeur)
                        {
                            nouvelleMatrice[newI, newJ] = matrice[i, j];
                        }
                    }
                }

                return nouvelleMatrice;
            }

            /// <summary>
            /// Convertit une matrice jagged (tableau de tableaux) en une matrice rectangulaire (tableau à deux dimensions).
            /// </summary>
            /// <typeparam name="T">Le type des éléments de la matrice.</typeparam>
            /// <param name="matriceJagged">La matrice jagged à convertir.</param>
            /// <returns>Une matrice rectangulaire contenant les mêmes éléments que la matrice jagged.</returns>
            /// <exception cref="ArgumentException">Lancé si la matrice jagged est nulle, vide, ou si toutes les sous-matrices n'ont pas la même longueur.</exception>
            public static T[,] ConvertirJaggedEnRectangulaire<T>(T[][] matriceJagged)
            {
                if (matriceJagged == null || matriceJagged.Length == 0)
                    throw new ArgumentException("La matrice jagged ne peut pas être nulle ou vide.");

                int rows = matriceJagged.Length;
                int cols = matriceJagged[0].Length;

                T[,] matriceRectangulaire = new T[rows, cols];

                for (int i = 0; i < rows; i++)
                {
                    if (matriceJagged[i].Length != cols)
                        throw new ArgumentException("Toutes les sous-matrices de la matrice jagged doivent avoir la même longueur.");

                    for (int j = 0; j < cols; j++)
                    {
                        matriceRectangulaire[i, j] = matriceJagged[i][j];
                    }
                }

                return matriceRectangulaire;
            }
            /// <summary>
            /// Convertit une matrice rectangulaire (tableau à deux dimensions) en une matrice jagged (tableau de tableaux).
            /// </summary>
            /// <typeparam name="T">Le type des éléments de la matrice.</typeparam>
            /// <param name="matriceRectangulaire">La matrice rectangulaire à convertir.</param>
            /// <returns>Une matrice jagged contenant les mêmes éléments que la matrice rectangulaire.</returns>
            /// <exception cref="ArgumentException">Lancé si la matrice rectangulaire est nulle.</exception>
            public static T[][] ConvertirRectangulaireEnJagged<T>(T[,] matriceRectangulaire)
            {
                if (matriceRectangulaire == null)
                    throw new ArgumentException("La matrice rectangulaire ne peut pas être nulle.");

                int rows = matriceRectangulaire.GetLength(0);
                int cols = matriceRectangulaire.GetLength(1);

                T[][] matriceJagged = new T[rows][];

                for (int i = 0; i < rows; i++)
                {
                    matriceJagged[i] = new T[cols];
                    for (int j = 0; j < cols; j++)
                    {
                        matriceJagged[i][j] = matriceRectangulaire[i, j];
                    }
                }

                return matriceJagged;
            }

            /// <summary>
            /// Trouve les indices des cellules dans une matrice bidimensionnelle qui contiennent une valeur spécifique.
            /// </summary>
            /// <typeparam name="T">Le type des éléments dans la matrice. Doit implémenter <see cref="IEquatable{T}"/>.</typeparam>
            /// <param name="matrice">La matrice bidimensionnelle dans laquelle on recherche la valeur.</param>
            /// <param name="valeur">La valeur recherchée dans la matrice.</param>
            /// <returns>Une liste de tuples représentant les indices (ligne, colonne) des cellules contenant la valeur recherchée.</returns>
            /// <example>
            /// Voici un exemple d'utilisation de la méthode <c>FindIndicesOfValue</c> :
            ///
            /// <code>
            /// int[,] intMatrix = {
            ///     { 1, 2, 3 },
            ///     { 4, 5, 6 },
            ///     { 7, 8, 9 }
            /// };
            ///
            /// var indices = FindIndicesOfValue(intMatrix, 5);
            /// foreach (var index in indices)
            /// {
            ///     Console.WriteLine($"({index.Item1}, {index.Item2})"); // Output: (1, 1)
            /// }
            /// </code>
            /// </example>
            public static List<Paire<int>> Indices<T>(this T[,] matrice, T valeur) where T : IEquatable<T>
            {
                if (matrice == null)
                {
                    throw new ArgumentException("La matrice ne doit pas être nulle.");
                }

                List<Paire<int>> indices = new List<Paire<int>>();

                for (int i = 0; i < matrice.GetLength(0); i++)
                {
                    for (int j = 0; j < matrice.GetLength(1); j++)
                    {
                        if (matrice[i, j].Equals(valeur))
                        {
                            indices.Add(new Paire<int>(i, j));
                        }
                    }
                }

                return indices;
            }

            /// <summary>
            /// Récupérer les différentes valeurs contenues dans une matrice
            /// </summary>
            /// <typeparam name="T">le type de données contenu dans la matrice</typeparam>
            /// <param name="matrice">la matrice</param>
            /// <returns>un tableau contenant une fois chaque valeur différente apparaissant dans la matrice</returns>
            public static T[] Valeurs<T>(this T[,] matrice)
            {
                List<T> valeurs = new List<T>();
                foreach (T element in matrice)
                {
                    if (!valeurs.Contains(element))
                    {
                        valeurs.Add(element);
                    }
                }
                return valeurs.ToArray();
            }
            
            /// <summary>
            /// Indique si une matrice contient une valeur où non
            /// </summary>
            /// <typeparam name="T">Le type de données de la matrice</typeparam>
            /// <param name="matrice">La matrice</param>
            /// <param name="valeur">La valeur recherchée dans la matrice</param>
            /// <returns>true si la valeur recherchée est dans la matrice, false sinon</returns>
            public static bool Contient<T>(this T[,] matrice, T valeur) where T : IEquatable<T>
            {
                foreach (T element in matrice)
                {
                    if (valeur.Equals(element))
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        /// <summary>
        /// Fonctions qui vise à représanter, calculer des formes
        /// </summary>
        public static class DessinUtils
        {
            /// <summary>
            /// Calcule les points discrets le long d'une ligne tracée entre deux points donnés.
            /// </summary>
            /// <param name="start">Le premier point (paire de doubles).</param>
            /// <param name="end">Le second point (paire de doubles).</param>
            /// <returns>Un tableau des coordonnées des points de la ligne entre les points start et end (paires d'entiers).</returns>
            public static PaireInt[] PointsLigne(Paire<double> start, Paire<double> end)
            {
                PaireInt[] points;
                int y;

                //// I] Cas Xstart = Xend
                if (start.X() == end.X())
                {
                    int firstY = (int)start.Y();
                    int lastY = (int)Math.Ceiling(end.Y());
                    if (lastY < firstY) // firstY doit être inférieur à lastY
                    {
                        int temp = firstY; firstY = lastY; lastY = temp;
                    }

                    points = new PaireInt[lastY-firstY+1];
                    for (y = firstY; y <= lastY; y++)
                    {
                        points[y - firstY] = new PaireInt((int)start.X(), y);
                        Debug.Log("point:"+ points[y - firstY]);
                    }
                    return points;
                }

                //// II] Cas général

                // Calcule des coordonnées à interpoler
                Paire<double>[] coordonnees = MathUtils.CoordonneesDiscretesLigne(start, end);
                // Initialisation du nombre de points à calculer
                int nombrePoints = (int)(Math.Abs(coordonnees[^1].X() - coordonnees[0].X()) + Math.Abs(coordonnees[^1].Y() - coordonnees[0].Y()));
                points = new PaireInt[nombrePoints];
                int p = 0; // index du tableau 'points'

                // Calcul des points
                y = (int)Math.Floor(coordonnees[0].Y());
                int incrementY;
                for (int i = 0; i < coordonnees.Length - 1; i++)
                {
                    int x = (int)Math.Round(coordonnees[i].X());
                    double limY = coordonnees[i + 1].Y();
                    incrementY = y < limY ? 1 : -1;
                    while (incrementY*y <= incrementY*limY)
                    {
                        points[p] = new PaireInt(x, y); 
                        p++;
                        y+=incrementY;
                    }
                    y-=incrementY;
                }
                return points;
            }

            /// <summary>
            /// Calcule les points discrets le long d'une ligne tracée entre deux points donnés.
            /// </summary>
            /// <param name="start">Le premier point (paire de doubles).</param>
            /// <param name="end">Le second point (paire de doubles).</param>
            /// <returns>Un tableau des coordonnées des points de la ligne entre les points start et end (paires d'entiers).</returns>
            public static PaireInt[] PointsCourbe0(Paire<double>[] coordonneesCourbe)
            {
                // Initialisation du nombre de points à calculer
                int nombrePoints = 32;
                PaireInt[]  points = new PaireInt[nombrePoints];
                int p = 0; // index du tableau 'points'

                // Calcul des points
                int y = (int)Math.Floor(coordonneesCourbe[0].Y());
                int incrementY;
                for (int i = 0; i < coordonneesCourbe.Length - 1; i++)
                {
                    int x = (int)Math.Round(coordonneesCourbe[i].X());
                    double limY = coordonneesCourbe[i + 1].Y();
                    incrementY = y < limY ? 1 : -1;
                    while (incrementY * y <= incrementY * limY)
                    {
                        if (p == nombrePoints) { nombrePoints *= 2; points = TableauUtils.Redimensionne(points, nombrePoints); } // double la taille du tableau si il n'est pas assez grand
                        points[p] = new PaireInt(x, y);
                        p++;
                        y += incrementY;
                    }
                    y -= incrementY;
                }

                // Redimensionner le tableau à la taille réelle utilisée
                TableauUtils.Redimensionne(points, p);
                return points;
            }

            public static PaireInt[] PointsCourbe(Paire<double>[] coordonneesCourbe)
            {
                // Liste pour stocker les points calculés
                List<PaireInt> points = new List<PaireInt>();

                // Parcourt chaque segment de la courbe
                for (int i = 0; i < coordonneesCourbe.Length - 1; i++)
                {
                    double x1 = coordonneesCourbe[i].X();
                    double y1 = coordonneesCourbe[i].Y();
                    double x2 = coordonneesCourbe[i + 1].X();
                    double y2 = coordonneesCourbe[i + 1].Y();

                    // Ajoute le point de départ du segment
                    points.Add(new PaireInt((int)Math.Round(x1), (int)Math.Round(y1)));

                    // Calcul de la distance entre les points en X
                    int distanceX = (int)Math.Abs(x2 - x1);

                    // Interpolation linéaire entre les points
                    for (int j = 1; j <= distanceX; j++)
                    {
                        double t = j / (double)distanceX;
                        double x = x1 + t * (x2 - x1);
                        double y = y1 + t * (y2 - y1);
                        points.Add(new PaireInt((int)Math.Round(x), (int)Math.Round(y)));
                    }
                }

                // Ajoute le dernier point de la courbe
                Paire<double> dernierPoint = coordonneesCourbe[coordonneesCourbe.Length - 1];
                points.Add(new PaireInt((int)Math.Round(dernierPoint.X()), (int)Math.Round(dernierPoint.Y())));

                return points.ToArray();
            }

        }
    }

    namespace Class
    {
        /// <summary>
        /// Classe générique permettant de manipuler des paires de valeurs.
        /// </summary>
        /// <typeparam name="DataType">Le type de données des valeurs de la paire.</typeparam>
        public class Paire<DataType> : IEquatable<Paire<DataType>>
            where DataType : struct // Limite DataType aux types de valeur
        {
            // Variables
            protected DataType valeur0; // Utilisation de DataType? pour permettre la nullité
            protected DataType valeur1; // Utilisation de DataType? pour permettre la nullité

            // Build
            public Paire()
            {
                valeur0 = default;
                valeur1 = default;
            }
            public Paire(Paire<DataType> model)
            {
                // Pas de copie profonde !!!
                valeur0 = model.valeur0;
                valeur1 = model.valeur1;
            }
            public Paire(DataType valeur0, DataType valeur1)
            {
                this.valeur0 = valeur0;
                this.valeur1 = valeur1;
            }
            public Paire(DataType[] tableau2)
            {
                // Erreur
                if (tableau2.Length != 2) { throw new ArgumentException("Le tableau doit être de longeur 2", "tableau2"); }
                // Assignation des valeurs
                valeur0 = tableau2[0];
                valeur1 = tableau2[1];
            }

            // Indexeurs
            public DataType this[int index]
            {
                get
                {
                    if (index == 0)
                    {
                        return valeur0;
                    }
                    else if (index == 1)
                    {
                        return valeur1;
                    }
                    else
                    {
                        throw new ArgumentException("L'index doit être égal à 0 ou 1", "index");
                    }
                }
                set
                {
                    if (index == 0)
                    {
                        valeur0 = value;
                    }
                    else if (index == 1)
                    {
                        valeur1 = value;
                    }
                    else
                    {
                        throw new ArgumentException("L'index doit être égal à 0 ou 1", "index");
                    }
                }
            }

            // Get
            public DataType X() { return valeur0; }
            public DataType Y() { return valeur1; }
            public DataType[] Valeurs()
            {
                return new DataType[] { valeur0, valeur1 };
            }
            // Set
            public void SetValeurs(DataType[] tableau2)
            {
                // Erreur
                if (tableau2.Length != 2) { throw new ArgumentException("Le tableau doit être de longeur 2", "tableau2"); }

                valeur0 = tableau2[0];
                valeur1 = tableau2[1];
            }

            // ToString
            /// <summary>
            /// Met sous forme de chaine de caractère la paire
            /// </summary>
            /// <returns>La chaine de caractères mettant en évidence les données de la paire</returns>
            public override string ToString()
            {
                return $"Paire<{typeof(DataType)}> ( {valeur0}, {valeur1})";
            }

            // Conversion
            public static explicit operator Paire<double>(Paire<DataType> paire)
            {
                if (Fonctions.NumUtils.IsNumerique<DataType>())
                {
                    double value1 = Convert.ToDouble(paire[0]);
                    double value2 = Convert.ToDouble(paire[1]);
                    return new Paire<double>(value1, value2);
                }
                else
                {
                    throw new InvalidOperationException("Conversion from DataType to int is not supported.");
                }
            }

            public static Paire<double>[] ConvertToArrayPaireDoubles(Paire<DataType>[] paires)
            {
                return Array.ConvertAll(paires, pi => (Paire<double>)pi);
            }

            public static implicit operator PaireInt(Paire<DataType> paire)
            {
                if (NumUtils.IsNumerique<DataType>())
                {
                    int value1 = Convert.ToInt32(paire[0]);
                    int value2 = Convert.ToInt32(paire[1]);
                    return new PaireInt(value1, value2);
                }
                else
                {
                    throw new InvalidOperationException("Conversion from DataType to int is not supported.");
                }
            }

            //// Bool
            public bool Equals(Paire<DataType> paire) { return this[0].Equals(paire[0]) && this[1].Equals(paire[1]); }

            //// Autres Méthodes

            /// <summary>
            /// Calcule la norme euclidienne des valeurs d'une paire de numeriques (int, float, double ou decimal).
            /// </summary>
            /// <typeparam name="DataType">Le type de données de la paire (int, float, double ou decimal).</typeparam>
            /// <param name="paire">La paire de valeurs.</param>
            /// <returns>La norme euclidienne des valeurs de la paire.</returns>
            /// <exception cref="ArgumentException">Le type de données de la paire n'est pas int, float, double ou decimal.</exception>
            public double NormEucli()
            {
                if (!NumUtils.IsNumerique<DataType>())
                {
                    throw new ArgumentException("Le type doit être int, float double ou decimal", "paire");
                }

                double x = Convert.ToDouble(valeur0);
                double y = Convert.ToDouble(valeur1);
                return Math.Sqrt(x * x + y * y);
            }

            /// <summary>
            /// Calcule un(e) paire/vecteur unitaire à partir d'une paire. 
            /// La norme Euclidienne du vecteur retourné vaut donc 1.
            /// </summary>
            /// <returns>La paire correspondant au vecteut unitaire</returns>
            public Paire<double> Unitaire()
            {
                if (Fonctions.NumUtils.IsNumerique<DataType>())
                {
                    // Calcul du vecteur unitaire
                    double magnitude = this.NormEucli();
                    if (magnitude == 0) return new Paire<double>(0, 0); // Si la longueur du vecteur est nulle, renvoyer un vecteur nul
                    double valeur0d = Convert.ToDouble(valeur0);
                    double valeur1d = Convert.ToDouble(valeur1);
                    return new Paire<double>(valeur0d / magnitude, valeur1d / magnitude);
                }
                else
                {
                    throw new InvalidOperationException("La méthode Unitaire n'est pas prise en charge pour ce type de données.");
                }
            }

            //// Fonctions

            // Sommes
            /// <summary>
            /// Somme terme à terme deux paires
            /// </summary>
            /// <param name="paireA"></param>
            /// <param name="paireB"></param>
            /// <returns>Paire résultante de la somme des deux paires</returns>
            public static Paire<decimal> Somme(Paire<decimal> paireA, Paire<decimal> paireB)
            {
                return new Paire<decimal>(paireA[0] + paireB[0],
                                          paireA[1] + paireB[1]);
            }
            public static Paire<double> Somme(Paire<double> paireA, Paire<double> paireB)
            {
                return new Paire<double>(paireA[0] + paireB[0],
                                         paireA[1] + paireB[1]);
            }
            public static Paire<float> Somme(Paire<float> paireA, Paire<float> paireB)
            {
                return new Paire<float>(paireA[0] + paireB[0],
                                        paireA[1] + paireB[1]);
            }
            public static Paire<int> Somme(Paire<int> paireA, Paire<int> paireB)
            {
                return new Paire<int>(paireA[0] + paireB[0],
                                      paireA[1] + paireB[1]);
            }

            /// <summary>
            /// Somme les termes d'une paire
            /// </summary>
            /// <param name="paire"></param>
            /// <returns>La somme des termes de la paire</returns>
            public static decimal Somme(Paire<decimal> paire) { return paire[0] + paire[0]; }
            public static double Somme(Paire<double> paire) { return paire[0] + paire[0]; }
            public static float Somme(Paire<float> paire) { return paire[0] + paire[0]; }
            public static int Somme(Paire<int> paire) { return paire[0] + paire[0]; }

            // Soustraction
            /// <summary>
            /// Fait la soustraction terme à terme des valeurs des paires : (a1-b1,a2-b2)
            /// </summary>
            /// <param name="paireA">Paire de laquelle ont soustrait</param>
            /// <param name="paireB">Paire dont on soustrait les valeurs</param>
            /// <returns>La paire résultant de la soustraction</returns>
            public static Paire<decimal> Soustraction(Paire<decimal> paireA, Paire<decimal> paireB)
            {
                return new Paire<decimal>(paireA[0] - paireB[0],
                                          paireA[1] - paireB[1]);
            }
            public static Paire<double> Soustraction(Paire<double> paireA, Paire<double> paireB)
            {
                return new Paire<double>(paireA[0] - paireB[0],
                                          paireA[1] - paireB[1]);
            }
            public static Paire<float> Soustraction(Paire<float> paireA, Paire<float> paireB)
            {
                return new Paire<float>(paireA[0] - paireB[0],
                                          paireA[1] - paireB[1]);
            }
            public static Paire<int> Soustraction(Paire<int> paireA, Paire<int> paireB)
            {
                return new Paire<int>(paireA[0] - paireB[0],
                                      paireA[1] - paireB[1]);
            }

            // Multiplication 
            /// <summary>
            /// Multiplie une paire terme par terme par un sclaire de type int, float, double ou decimal
            /// </summary>
            /// <param name="paire">La paire dont on multiplie les termes</param>
            /// <param name="scalaire">Le scalaire multiplicateur</param>
            /// <returns>Une paire dont les valeurs sont celles de la paire passee en parametre multipliees par le sclaire</returns>
            public static Paire<decimal> Multiplication(Paire<decimal> paire, decimal scalaire)
            {
                Paire<decimal> nouv = new Paire<decimal>(
                    paire.X() * scalaire,
                    paire.Y() * scalaire);

                return nouv;
            }
            public static Paire<double> Multiplication(Paire<double> paire, double scalaire)
            {
                Paire<double> nouv = new Paire<double>(
                    paire.X() * scalaire,
                    paire.Y() * scalaire);

                return nouv;
            }
            public static Paire<float> Multiplication(Paire<float> paire, float scalaire)
            {
                Paire<float> nouv = new Paire<float>(
                    paire.X() * scalaire,
                    paire.Y() * scalaire);

                return nouv;
            }
            public static Paire<int> Multiplication(Paire<int> paire, int scalaire)
            {
                Paire<int> nouv = new Paire<int>(
                    paire.X() * scalaire,
                    paire.Y() * scalaire);

                return nouv;
            }

            /// <summary>
            /// Multiplie terme à terme deux paires de type int, float, double ou decimal
            /// </summary>
            /// <param name="paireA">La premiere paire a multiplier</param>
            /// <param name="paireB">La seconde paire a multiplier</param>
            /// <returns>Une paire issue de la multiplication terme à terme des deux paires passées en paramettre</returns>
            public static Paire<decimal> Multiplication(Paire<decimal> paireA, Paire<decimal> paireB)
            {
                Paire<decimal> nouv = new Paire<decimal>(
                    paireA.X() * paireB.X(),
                    paireA.Y() * paireB.Y());

                return nouv;
            }
            public static Paire<double> Multiplication(Paire<double> paireA, Paire<double> paireB)
            {
                Paire<double> nouv = new Paire<double>(
                    paireA.X() * paireB.X(),
                    paireA.Y() * paireB.Y());

                return nouv;
            }
            public static Paire<float> Multiplication(Paire<float> paireA, Paire<float> paireB)
            {
                Paire<float> nouv = new Paire<float>(
                    paireA.X() * paireB.X(),
                    paireA.Y() * paireB.Y());

                return nouv;
            }
            public static Paire<int> Multiplication(Paire<int> paireA, Paire<int> paireB)
            {
                Paire<int> nouv = new Paire<int>(
                    paireA.X() * paireB.X(),
                    paireA.Y() * paireB.Y());

                return nouv;
            }

            // Distance
            /// <summary>
            /// Calcule la distance euclidienne entre deux instances de Paire<DataType>.
            /// </summary>
            /// <typeparam name="DataType">Le type des données dans les paires, limité aux types de valeur qui sont comparables et convertibles.</typeparam>
            /// <param name="p1">La première paire de valeurs.</param>
            /// <param name="p2">La deuxième paire de valeurs.</param>
            /// <returns>La distance euclidienne entre les deux paires.</returns>
            /// <exception cref="InvalidOperationException">Lancé si l'une des valeurs dans les paires est nulle.</exception>
            public static double DistanceEuclidienne<NumericType>(Paire<NumericType> p1, Paire<NumericType> p2) where NumericType : struct, IComparable, IConvertible
            {
                // Convertit les valeurs en double pour effectuer les calculs
                double x1 = Convert.ToDouble(p1.X());
                double y1 = Convert.ToDouble(p1.Y());
                double x2 = Convert.ToDouble(p2.X());
                double y2 = Convert.ToDouble(p2.Y());
                //Debug.Log("x1:" + x1 + " x2:" + x2 + " y1:" + y1 + " y2:" + y2+" D="+ Math.Sqrt(Math.Pow(x2 - x1, 2) + Math.Pow(y2 - y1, 2)));
                // Calcule et retourne la distance euclidienne
                return Math.Sqrt(Math.Pow(x2 - x1, 2) + Math.Pow(y2 - y1, 2));
            }
        }
        /// <summary>
        /// Class qui permet de manipuler des paires d'entiers. Elle herite de la classe Paire
        /// </summary>
        public class PaireInt : Paire<int>
        {
            // Constructeurs
            public PaireInt() : base(0, 0) { }
            public PaireInt(int valeur0, int valeur1) : base(valeur0, valeur1) { }
            public PaireInt(int[] tableau2) : base(tableau2)
            {
                if (tableau2.Length != 2)
                {
                    throw new ArgumentException("Le tableau doit être de longueur 2", nameof(tableau2));
                }
            }
            public PaireInt(PaireInt model) : base(model) { }

            public static Paire<int>[] ConvertToPaireInt(PaireInt[] pairesInt)
            {
                return Array.ConvertAll(pairesInt, pi => (Paire<int>)pi);
            }
        }


        //// Graphes
        
        public abstract class SommetAbstrait
        {
            DataSommetAbstraite donnee;
        }
        public abstract class DataSommetAbstraite
        {
            private string sommetID;
            // Get
            public string SommetID() { return sommetID; }
            // Set
            public void SetSommetID(string sommetID)
            {
                this.sommetID = sommetID;
            }
        }

        /// <summary>
        /// Graphe non-oriente
        /// </summary>
        /// <typeparam name="DataSommet">Le type des donnees passées dans les sommets</typeparam>
        public class Graphe<DataSommet>
        {
            // Classe interne
            private class Sommet<DataType>
            {
                // Variables
                private static Graphe<DataSommet> graphe;
                private string id;
                private Sommet<DataType>[] adjacents;
                private int degres;
                private DataType donnee;

                // Builder
                public Sommet(DataType donnee = default)
                {
                    id = $"{graphe.ordre}";
                    adjacents = new Sommet<DataType>[0];
                    degres = 0;
                    this.donnee = donnee;
                }
                public Sommet(Sommet<DataType> model)
                {
                    id = $"{graphe.ordre}";
                    adjacents = model.adjacents;
                    degres = model.degres;
                    donnee = model.donnee;
                }

                // Getter
                public static Graphe<DataSommet> Graphe() { return graphe; }
                public string ID() { return id; }
                public int Degres() { return degres; }
                public Sommet<DataType>[] Adjacents() { return adjacents; }
                public DataType Donnee() { return donnee; }

                // Setter
                public static void SetGraphe(Graphe<DataSommet> newGraphe) { graphe = newGraphe; }
                public void SetAdjacents(Sommet<DataType>[] adjacents)
                {
                    if (adjacents == null) { throw new ArgumentException("Ne peut être nul. Vous pouvez initialiser une liste vide", "adjacents"); }
                    this.adjacents = adjacents;
                    degres = adjacents.Length;
                }
                public void SetDonnee(DataType donnee)
                {
                    this.donnee = donnee;
                }
            }

            // Variables
            private int ordre;
            private Sommet<DataSommet>[] sommets;

            // Constructeur
            public Graphe()
            {
                ordre = 0;
                sommets = new Sommet<DataSommet>[0];
                Sommet<DataSommet>.SetGraphe(this);
            }
            public Graphe(Graphe<DataSommet> model)
            {
                ordre = model.ordre;
                sommets = model.sommets;
                Sommet<DataSommet>.SetGraphe(this);
            }

            // Get
            public int Ordre() { return ordre; }
            /// <summary>
            /// Récupérer les données stockées dans un sommet
            /// </summary>
            /// <param name="indexSommet">Le sommet dont on veut récupérer les données</param>
            /// <returns>Les données du sommet</returns>
            public DataSommet DonneeSommet(int indexSommet)
            {
                // Erreur
                if (indexSommet < 0 || this.ordre <= indexSommet) // Verifie indexSommet
                {
                    throw new ArgumentException("L'indice doit être compris entre 0(inclu) et l'ordre du graphe(exclu) pour correspondre à un sommet.",
                                                "indexSommet");
                }

                return sommets[indexSommet].Donnee();
            }

            // Set
            /// <summary>
            /// Ajoute un sommet au graphe et incrémente son ordre
            /// </summary>
            public void AjouteSommet(DataSommet donnee = default)
            {
                // Creation du nouveau Sommet
                Sommet<DataSommet> nouveau = new Sommet<DataSommet>(donnee);
                // Creation du nouveau tableau de sommets
                Sommet<DataSommet>[] nouveaux = Fonctions.TableauUtils.Concatene(this.sommets, nouveau);
                // Assignation 
                this.sommets = nouveaux;
                this.ordre++;
            }

            public void SetDataSommet(int indexSommet, DataSommet donnee)
            {
                sommets[indexSommet].SetDonnee(donnee);
            }

            /// <summary>
            /// Crée une arrete entre deux sommets dans le graphe.
            /// Cela incrémente donc les degres des sommets et ajoute a chacun un sommet adjacent.
            /// </summary>
            /// <param name="indexSommetsAdjacents">Les sommets entre lesquels on cree l'arrete</param>
            public void CreeArrete(int indexSommet0, int indexSommet1)
            {
                // Erreur
                if (indexSommet0 < 0 || indexSommet0 > this.sommets.Length) // Verifie indexSommet0
                {
                    throw new ArgumentException("L'index doit correspondre à un sommet du graphe. Il doit être compris entre 0(inclu) et l'ordre du graphe(exclu)",
                                                "indexSommet0");
                }
                if (indexSommet1 < 0 || indexSommet1 > this.sommets.Length) // Verifie indexSommet1
                {
                    throw new ArgumentException("L'index doit correspondre à un sommet du graphe. Il doit être compris entre 0(inclu) et l'ordre du graphe(exclu)",
                                                "indexSommet1");
                }

                // Recupere les Sommets
                Sommet<DataSommet> adjacent0 = this.sommets[indexSommet0];
                Sommet<DataSommet> adjacent1 = this.sommets[indexSommet1];
                // Modifie le Sommet 0
                Sommet<DataSommet>[] newAjdacents0 = Fonctions.TableauUtils.Concatene(adjacent0.Adjacents(), adjacent1);
                adjacent0.SetAdjacents(newAjdacents0);
                // Modifie le Sommet 1
                Sommet<DataSommet>[] newAjdacents1 = Fonctions.TableauUtils.Concatene(adjacent1.Adjacents(), adjacent0);
                adjacent1.SetAdjacents(newAjdacents1);
            }

            // ToString
            public override string ToString()
            {
                StringBuilder builder = new StringBuilder();
                builder.AppendLine($"Graphe - Ordre: {ordre}, DataType: {typeof(DataSommet)}");
                for (int i = 0; i < ordre; i++)
                {
                    builder.Append($"Sommet {i}: ");
                    builder.AppendLine(this.DonneeSommet(i).ToString());
                }
                return builder.ToString();
            }

            //// Methodes

            /// <summary>
            /// Renvoie un tableau contenant des paires d'entiers représentant les sommets liés par une arête.
            /// </summary>
            /// <returns>Tableau de paires d'entiers représentant les arêtes du graphe.</returns>
            public PaireInt[] Arretes()
            {
                // Calculer le nombre total d'arêtes dans le graphe pour ensuite initialiser le tableau
                int totalArretes = 0;
                foreach (Sommet<DataSommet> sommet in sommets)
                {
                    totalArretes += sommet.Degres();
                }
                totalArretes /= 2;
                // Initialiser le tableau des arêtes
                PaireInt[] arretes = new PaireInt[totalArretes];
                int index = 0;

                // Parcours chaque paire de sommets
                for (int i = 0; i < ordre - 1; i++)
                {
                    for (int j = i; j < ordre; j++)
                    {
                        // Verifie que c'est une arrete
                        if (Fonctions.TableauUtils.Contient(sommets[i].Adjacents(), sommets[j]))
                        {
                            // Ajoute la paire de sommets comme une arête
                            arretes[index++] = new PaireInt(i, j);
                        }
                    }
                }

                return arretes;
            }
        }
        /// <summary>
        /// Graphe non-oriente
        /// </summary>
        /// <typeparam name="DataSommet">Le type des donnees passées dans les sommets</typeparam>
        /// <typeparam name="DataArrete">Le type des donnees passées dans les arretes</typeparam>
        public class Graphe<DataSommet, DataArrete> where DataSommet : DataSommetAbstraite
        {
            // Class interne Sommet
            private class Sommet : SommetAbstrait
            {
                // Variables
                private Graphe<DataSommet, DataArrete> graphe;
                private string id;
                private int[] adjacents;
                private int degre;
                private DataSommet donnee;

                // Builder
                public Sommet(Graphe<DataSommet, DataArrete> graphe, DataSommet donnee = default)
                {
                    this.graphe = graphe;
                    id = $"{graphe.ordre}";
                    adjacents = new int[0];
                    degre = 0;
                    this.donnee = donnee;
                }
                public Sommet(Sommet model)
                {
                    graphe = model.graphe;
                    id = $"{graphe.ordre}";
                    adjacents = new int[model.adjacents.Length]; for (int i = 0; i < model.adjacents.Length; i++) { adjacents[i] = model.adjacents[i]; }
                    degre = model.degre;
                    donnee = model.donnee;
                }

                // Getter
                public Graphe<DataSommet, DataArrete> Graphe() { return graphe; }
                public string ID() { return id; }
                public int Degres() { return degre; }
                public int[] Adjacents() { return adjacents; }
                public DataSommet Donnee() { return donnee; }

                // Setter
                public void SetGraphe(Graphe<DataSommet, DataArrete> newGraphe) { graphe = newGraphe; }
                public void SetAdjacents(int[] adjacents)
                {
                    if (adjacents == null) { throw new ArgumentException("Ne peut être nul. Vous pouvez initialiser une liste vide", "adjacents"); }
                    this.adjacents = adjacents;
                    degre = adjacents.Length;
                }
                public void SetDonnee(DataSommet donnee)
                {
                    this.donnee = donnee;
                }
            }
            // Class interne Arrete
            public class Arrete
            {
                // Variables
                private Graphe<DataSommet, DataArrete> graphe;
                private string id;
                private DataArrete donnee;

                // Builder
                public Arrete(Graphe<DataSommet, DataArrete> graphe, DataArrete donnee = default)
                {
                    this.graphe = graphe;
                    id = $"{graphe.ordre}";
                    this.donnee = donnee;
                }
                public Arrete(Arrete model)
                {
                    graphe = model.graphe;
                    id = $"{graphe.ordre}";
                    donnee = model.donnee;
                }

                // Getter
                public Graphe<DataSommet, DataArrete> Graphe() { return graphe; }
                public string ID() { return id; }
                public DataArrete Donnee() { return donnee; }

                // Setter
                public void SetGraphe(Graphe<DataSommet, DataArrete> newGraphe) { graphe = newGraphe; }
                public void SetDonnee(DataArrete donnee) { this.donnee = donnee; }
            }

            // Variables
            private int ordre;
            private Sommet[] sommets;
            private Arrete[,] arretes; // référence les arretes en fonction des sommets (matrice carrée)

            // build
            public Graphe()
            {
                ordre = 0;
                sommets = new Sommet[0];
                arretes = new Arrete[ordre, ordre];
            }
            public Graphe(Graphe<DataSommet, DataArrete> model) // Ne fait pas une copie profonde !!!
            {
                ordre = model.ordre;
                sommets = model.sommets;
                foreach (Sommet sommet in sommets) { sommet.SetGraphe(this); }
                arretes = model.arretes;
                foreach (Arrete arrete in arretes) { arrete.SetGraphe(this); }
            }

            // get
            public int Ordre() { return ordre; }

            /// <summary>
            /// Récupérer l'index d'un sommet dans le graphe à partir de son ID
            /// </summary>
            /// <param name="sommetID">L'identifiant du sommet</param>
            /// <returns>L'index du sommet dans le graphe</returns>
            public int SommetIndex(string sommetID)
            {
                int index = 0;
                while (sommets[index].ID() != sommetID) { index++; }
                return index;
            }
            /// <summary>
            /// Récupérer les données stockées dans un sommet
            /// </summary>
            /// <param name="indexSommet">Le sommet dont on veut récupérer les données</param>
            /// <returns>Les données du sommet</returns>
            public DataSommet DonneeSommet(int indexSommet)
            {
                // Erreur
                if (indexSommet < 0 || this.ordre <= indexSommet) // Verifie indexSommet
                {
                    throw new ArgumentException("L'indice doit être compris entre 0(inclu) et l'ordre du graphe(exclu) pour correspondre à un sommet.",
                                                "indexSommet");
                }

                return sommets[indexSommet].Donnee();
            }
            /// <summary>
            /// Renvoie un tableau contenant les donnees de toutes les arêtes du graphe.
            /// </summary>
            /// <returns>Tableau des donnees des arêtes du graphe.</returns>
            public DataArrete[] DonneeArretes()
            {
                // Calculer le nombre total d'arêtes dans le graphe pour ensuite initialiser le tableau
                int totalArretes = CompteLiaisons();
                // Initialiser le tableau des arêtes
                DataArrete[] tableauArretes = new DataArrete[totalArretes];
                int index = 0;

                for (int i = 0; i < ordre; i++)
                {
                    for (int j = 0; j <= i; j++)
                    {
                        // Verifie que c'est une arrete
                        if (arretes[i, j] != null)
                        {
                            // Ajoute la paire de sommets comme une arête
                            tableauArretes[index++] = arretes[i, j].Donnee();
                        }
                    }
                }

                return tableauArretes;
            }

            /// <summary>
            /// Renvoie le tableau des donnees des arretes liées à un sommet
            /// </summary>
            /// <param name="indexSommet">l'indice du sommet dans le graphe</param>
            /// <returns>Le tableau des donnees des arretes liees au sommet</returns>
            public DataArrete[] DonneeArretes(int indexSommet)
            {
                // Initialisation du tableau
                int longueur = 0;
                for (int i = 0; i< arretes.GetLength(0) ; i++)
                {
                    if (i < indexSommet)
                    {
                        if ( arretes[indexSommet, i] != null) { longueur++; }
                    } 
                    else if (i > indexSommet)
                    {
                        if ( arretes[i, indexSommet] != null) { longueur++; }
                    } else { continue; } // saute le cas i == sommet
                }
                DataArrete[] donneeArretes = new DataArrete[longueur];

                // Remplissage
                int d = 0;
                for (int i = 0; i < arretes.GetLength(0); i++)
                {
                    if (i < indexSommet)
                    {
                        if (arretes[indexSommet, i] != null) { donneeArretes[d] = arretes[indexSommet, i].Donnee(); d++; }
                    }
                    else if (i > indexSommet)
                    {
                        if (arretes[i, indexSommet] != null) { donneeArretes[d] = arretes[i, indexSommet].Donnee(); d++; }
                    }
                    else { continue; } // saute le cas i == sommet
                }
                return donneeArretes;
            }

            // set
            public void SetDataSommet(int indexSommet, DataSommet donnee) { sommets[indexSommet].SetDonnee(donnee); }
            public void SetDataArrete(int indexSommet0, int indexSommet1, DataArrete donnee) { arretes[indexSommet0, indexSommet1].SetDonnee(donnee); }

            /// <summary>
            /// Ajoute un sommet au graphe et incrémente son ordre.
            /// </summary>
            /// <param name="donnee">Les données associées au sommet.</param>
            public void AjouteSommet(DataSommet donnee = default)
            {
                // Creation du nouveau Sommet
                Sommet nouveau = new Sommet(this, donnee);
                // Liaison du sommet à ses donnees
                donnee.SetSommetID(nouveau.ID());
                // Creation du nouveau tableau de sommets
                Sommet[] nouveaux = TableauUtils.Concatene(this.sommets, nouveau);
                // Assignation 
                this.sommets = nouveaux;
                this.ordre++;
                // Redimmensionne arretes
                arretes = Fonctions.MatriceUtils.Redimensionne(arretes, ordre, ordre);
            }

            /// <summary>
            /// Crée une arrete et met a jour les sommets adjacents.
            /// Cela incremente le degre des sommets et leur ajoute l'indice du nouveau voisin.
            /// </summary>
            /// <param name="indexSommet0">Le premier sommet</param>
            /// <param name="indexSommet1">Le second sommet</param>
            public void CreeArrete(int indexSommet0, int indexSommet1, DataArrete donnee)
            {
                // Erreur
                if (indexSommet0 < 0 || indexSommet0 > this.sommets.Length) // Verifie indexSommet0
                {
                    throw new ArgumentException("L'index doit correspondre à un sommet du graphe. Il doit être compris entre 0(inclu) et l'ordre du graphe(exclu)", "indexSommet0");
                }
                if (indexSommet1 < 0 || indexSommet1 > this.sommets.Length) // Verifie indexSommet1
                {
                    throw new ArgumentException("L'index doit correspondre à un sommet du graphe. Il doit être compris entre 0(inclu) et l'ordre du graphe(exclu)", "indexSommet1");
                }

                // Non-oriente : on place toutes les arretes du même côté de la matrice
                if (indexSommet0 < indexSommet1)
                {
                    int temp = indexSommet0; indexSommet0 = indexSommet1; indexSommet1 = temp;
                }
                // Creation arrete
                arretes[indexSommet0, indexSommet1] = new Arrete(this, donnee);

                // Recupere les Sommets
                Sommet adjacent0 = this.sommets[indexSommet0];
                Sommet adjacent1 = this.sommets[indexSommet1];
                // Modifie le Sommet 0
                int[] newAjdacents0 = Fonctions.TableauUtils.Concatene(adjacent0.Adjacents(), indexSommet1);
                adjacent0.SetAdjacents(newAjdacents0);
                // Modifie le Sommet 1
                int[] newAjdacents1 = Fonctions.TableauUtils.Concatene(adjacent1.Adjacents(), indexSommet0);
                adjacent1.SetAdjacents(newAjdacents1);
            }

            // ToString
            public override string ToString()
            {
                StringBuilder builder = new StringBuilder();
                builder.AppendLine("Graphe<sommet-arretes> non-oriente");
                builder.AppendLine($"Ordre: {ordre}, DataType: {typeof(DataSommet)}, {typeof(DataArrete)}");
                builder.AppendLine($"Arretes:");
                for (int i = 0; i < ordre; i++)
                {
                    for (int j = 0; j < ordre; j++)
                    {
                        if (i < j) { continue; }
                        else if (arretes[i, j] != null) { builder.Append("1"); }
                        else { builder.Append("0"); }
                    }
                    builder.AppendLine();
                }
                for (int i = 0; i < ordre; i++)
                {
                    builder.Append($"Sommet {i}: ");
                    builder.AppendLine(this.DonneeSommet(i).ToString());
                }

                return builder.ToString();
            }

            //// Methodes

            /// <summary>
            /// Compte le nombre total de liaisons (arêtes) dans le graphe.
            /// </summary>
            /// <returns>Le nombre total de liaisons dans le graphe.</returns>
            public int CompteLiaisons()
            {
                int nbliaisons = 0;
                foreach (Sommet sommet in sommets)
                {
                    nbliaisons += sommet.Degres();
                }
                nbliaisons /= 2;
                return nbliaisons;
            }

            /// <summary>
            /// Renvoie un tableau contenant des paires d'entiers représentant les sommets liés par une arête.
            /// </summary>
            /// <returns>Tableau des paires d'index des sommets liés par des arrêtes.</returns>
            public PaireInt[] Liaisons()
            {
                // Calculer le nombre total d'arêtes dans le graphe pour ensuite initialiser le tableau
                int totalArretes = CompteLiaisons();
                // Initialiser le tableau des arêtes
                PaireInt[] liaisons = new PaireInt[totalArretes];
                int index = 0;

                // Parcours chaque paire de sommets
                for (int i = 0; i < ordre; i++)
                {
                    for (int j = i; j <= i; j++)
                    {
                        // Verifie que c'est une arrete
                        if (arretes[i, j] != null)
                        {
                            // Ajoute la paire de sommets comme une arête
                            liaisons[index++] = new PaireInt(i, j);
                        }
                    }
                }

                return liaisons;
            }
        }

        /// <summary>
        /// Class qui implémente les tableaux de répartition (abstraction de fonctions de répartition).
        /// Ils permettent de modéliser et mettre en œuvre des lois de probabilité.
        /// </summary>
        /// <typeparam name="DataType">Le type de valeur que prend la Variable Aléatoire (V.A.) associée à la Loi de probabilité (représentée par le tableau).</typeparam>
        public class TabRepartition<DataType>
        {
            // Variables
            private DataType[] valeurs;
            private double[] repartitionCumulee; // tableau dont les valeurs sont croissantes et comprises entre 0 et 1, et dont la dernière valeur vaut 1.
            private Random random;

            // Constructeurs
            public TabRepartition()
            {
                valeurs = null;
                repartitionCumulee = null;
                random = new Random();
            }

            public TabRepartition(TabRepartition<DataType> model)
            {
                valeurs = (DataType[])model.valeurs.Clone();
                repartitionCumulee = (double[])model.repartitionCumulee.Clone();
                random = model.random;
            }

            public TabRepartition(DataType[] valeurs, double[] repartition, Random random = null)
            {
                // Erreur si les tableaux ont des longueurs différentes
                if (valeurs.Length != repartition.Length)
                {
                    throw new ArgumentException("Les tableaux doivent être de même longueur", nameof(valeurs) + ", " + nameof(repartition));
                }

                // Conversion de repartition en repartitionCumulee si nécessaire
                if (!IsRepartitionCumulee(repartition))
                {
                    repartition = Fonctions.NumUtils.Cumule(repartition);
                }

                // Vérification que la répartition est correcte
                if (!IsRepartitionCumulee(repartition))
                {
                    throw new ArgumentException("Ne peut représenter une fonction de répartition", nameof(repartition));
                }

                this.valeurs = (DataType[])valeurs.Clone();
                this.repartitionCumulee = (double[])repartition.Clone();
                this.random = random ?? new Random();
            }

            public TabRepartition(DataType[] valeurs, string methode = "uniforme", Random random = null)
            {
                this.valeurs = (DataType[])valeurs.Clone();
                this.random = random ?? new Random();

                // Génération de la répartition selon la méthode spécifiée
                double[] repartition = Fonctions.StatUtils.Repartition(valeurs.Length, methode);
                repartitionCumulee = Fonctions.NumUtils.Cumule(repartition);

                // Vérification de la validité de la répartition
                if (!IsRepartitionCumulee(repartitionCumulee))
                {
                    throw new Exception("La répartition n'est pas valide");
                }

            }

            // Set
            public void SetRandom(Random newRandom) { random = newRandom; }
            public void SetRandom(int seed) { random = new Random(seed); }

            // Propriétés
            public DataType[] Valeurs => valeurs;

            public double[] Repartition => repartitionCumulee;

            // Méthodes
            public void SetValeurs(DataType[] valeurs)
            {
                // Vérification de la compatibilité des longueurs
                if (repartitionCumulee != null && repartitionCumulee.Length != valeurs.Length)
                {
                    throw new ArgumentException("Longueur incompatible avec celle de la répartition", nameof(valeurs));
                }

                this.valeurs = (DataType[])valeurs.Clone();
            }

            public void SetRepartition(double[] tableau)
            {
                // Validité de la répartition
                if (IsRepartition(tableau))
                {
                    tableau = Fonctions.NumUtils.Cumule(tableau);
                }
                else if (!IsRepartitionCumulee(tableau))
                {
                    throw new ArgumentException("Ne peut représenter une fonction de répartition", nameof(tableau));
                }

                // Vérification de la compatibilité des longueurs
                if (valeurs != null && tableau.Length != valeurs.Length)
                {
                    throw new ArgumentException("Longueur incompatible avec celle des valeurs", nameof(tableau));
                }

                repartitionCumulee = (double[])tableau.Clone();
            }

            public override string ToString()
            {
                StringBuilder builder = new StringBuilder();
                builder.Append("Valeurs: ");
                builder.AppendLine(string.Join(", ", valeurs));
                builder.Append("Repartition: ");
                builder.AppendLine(string.Join(", ", repartitionCumulee));
                return builder.ToString();
            }

            public bool IsEmpty() => valeurs == null || repartitionCumulee == null;

            public static bool IsRepartition(double[] tableau)
            {
                // Vérification que les valeurs sont comprises entre 0 et 1
                if (!tableau.All(valeur => valeur >= 0 && valeur <= 1))
                {
                    return false;
                }
                // Vérification que la somme totale des valeurs vaut 1
                if (1d - Fonctions.NumUtils.Somme(tableau) > double.Epsilon)
                {
                    return false;
                }

                return true;
            }

            public static bool IsRepartitionCumulee(double[] tableau)
            {
                // Vérification que les valeurs sont croissantes
                for (int i = 1; i < tableau.Length; i++)
                {
                    if (tableau[i] < tableau[i - 1])
                    {
                        return false;
                    }
                }
                // Vérification que les valeurs sont comprises entre 0 et 1
                if (!tableau.All(valeur => valeur >= 0 && valeur <= 1))
                {
                    return false;
                }
                // Vérification que la dernière valeur vaut 1 (avec prise en compte des arrondis)(
                if (tableau[^1] < 1 - double.Epsilon || tableau[^1] > 1 + double.Epsilon)
                {
                    return false;
                }
                return true;
            }

            public DataType Observation()
            {
                // Vérification des erreurs
                if (IsEmpty())
                {
                    throw new InvalidOperationException("Le tableau de répartition est vide ou non initialisé.");
                }
                if (!IsRepartitionCumulee(repartitionCumulee))
                {
                    throw new Exception("La répartition cumulée n'est pas valide");
                }

                // Génération d'un nombre aléatoire entre 0 et 1
                double aleatoire = random.NextDouble();
                if (aleatoire > 1 || aleatoire < 0)
                {
                    throw new Exception("Le double généré n'appartient pas à [0,1], c'est un problème important");
                }

                // Recherche de l'index où la valeur est juste supérieure à aléatoire dans la répartition cumulative
                int indexValeur = 0;
                while (indexValeur + 1 < repartitionCumulee.Length && aleatoire > repartitionCumulee[indexValeur + 1] - repartitionCumulee[0])
                {
                    indexValeur++;
                }

                // Retourne la valeur correspondante dans le tableau des valeurs
                return valeurs[indexValeur];
            }
        }

        /// <summary>
        /// Permet de gérer des matrices d'entiers représentant des formes.
        /// La visée de cette classe est représentative (et non calculatoire).
        /// </summary>
        public class Blueprint
        {
            // Variables
            private int width;
            private int height;
            private int[,] matrice;
            private PaireInt position; // Position de la valeur [0,0] du blueprint par rapport à l'origine du reprère

            // Constructeurs
            public Blueprint()
            {
                width = 0;
                height = 0;
                matrice = null;
                position = new PaireInt(0, 0);
            }

            /// <summary>
            /// Constructeur par copie profonde
            /// </summary>
            /// <param name="model">La copie profonde du blueprint model</param>
            public Blueprint(Blueprint model)
            {
                width = model.width;
                height = model.height;
                matrice = MatriceUtils.CopieProfonde(model.matrice);
                position = new PaireInt(model.position);
            }

            public Blueprint(int[,] matrice, PaireInt position = null)
            {
                // Origine
                if (position == null)
                {
                    position = new PaireInt(0, 0);
                }

                width = matrice.GetLength(0);
                height = matrice.GetLength(1);
                this.matrice = matrice;
                this.position = position;
            }

            /// <summary>
            /// Crée un blueprint de dimension width x height
            /// </summary>
            /// <param name="width">largueur du Blueprint a creer</param>
            /// <param name="height">hauteur du Blueprint a creer</param>
            /// <param name="value">vlaleur qui sera assignee a chaque position dans le blueprint</param>
            /// <param name="position">position de la valeur d'indice [0,0] dans le repere</param>
            public Blueprint(int width, int height = -1, int? value = 0, PaireInt position = default)
            {
                // Erreur
                if (height < -1)
                {
                    throw new ArgumentException("les dimmensions ne peuvent être négatives", "height");
                }
                if (width < 0)
                {
                    throw new ArgumentException("les dimmensions ne peuvent être négatives", "width");
                }

                // Hauteur de la matrice
                height = height == -1 ? width : height;

                // Initialisation de la matrice
                int[,] matrice = new int[width, height];

                // Remplissage de la matrice
                for (int i = 0; i < matrice.GetLength(0); i++)
                {
                    for (int j = 0; j < matrice.GetLength(1); j++)
                    {
                        matrice[i, j] = value.Value;
                    }
                }

                // Position
                if (position == null)
                {
                    position = new PaireInt(0, 0);
                }

                this.width = width;
                this.height = height;
                this.matrice = matrice;
                this.position = position;
            }

            // Getters
            public int Width() { return width; }
            public int Height() { return height; }
            public int[,] Matrice() { return matrice; }
            public int Valeur(int x, int y) { return this[x, y]; }
            public PaireInt Position() { return position; }

            // Index
            /// <summary>
            /// Retourne ou définit la valeur de la matrice à la position souhaitée
            /// (la position de la valeur prend en compte le paramètre position).
            /// </summary>
            /// <param name="x">La position horizontale</param>
            /// <param name="y">La position verticale</param>
            /// <returns>La valeur</returns>
            public int this[int x, int y]
            {
                get
                {
                    if (IsValidPosition(x, y))
                    {
                        return matrice[x - position.X(), y - position.Y()];
                    }
                    else
                    {
                        throw new ArgumentException("La position de la valeur souhaitée doit être valide", "position");
                    }
                }
                set
                {
                    if (IsValidPosition(x, y))
                    {
                        matrice[x - position.X(), y - position.Y()] = value;
                    }
                    else
                    {
                        throw new ArgumentException("La position de la valeur souhaitée doit être valide", "position");
                    }
                }
            }
            /// <summary>
            /// Retourne ou définit la valeur de la matrice à la position souhaitée
            /// (la position de la valeur prend en compte le paramètre position).
            /// </summary>
            /// <param name="position">La position (x,y)</param>
            /// <returns>La valeur</returns>
            public int this[PaireInt position]
            {
                get
                {
                    int x = position.X(), y = position.Y();
                    if (IsValidPosition(x, y))
                    {
                        return matrice[x - this.position.X(), y - this.position.Y()];
                    }
                    else
                    {
                        throw new ArgumentException("La position de la valeur souhaitée doit être valide", "position");
                    }
                }
                set
                {
                    int x = position.X(), y = position.Y();
                    if (IsValidPosition(x, y))
                    {
                        matrice[x - this.position.X(), y - this.position.Y()] = value;
                    }
                    else
                    {
                        throw new ArgumentException("La position de la valeur souhaitée doit être valide", "position");
                    }
                }
            }

            // Set
            public void Set(Blueprint model)
            {
                width = model.width;
                height = model.height;
                matrice = model.matrice;
                position = model.position;
            }
            public void SetMatrix(int[,] matrix)
            {
                this.matrice = matrix;
                width = matrix.GetLength(0);
                height = matrix.GetLength(1);
            }
            public void SetPosition(PaireInt position)
            {
                this.position = position;
            }

            // ToString
            public override string ToString()
            {
                // Erreurs
                if (width != matrice.GetLength(0)) { throw new Exception("La largeur de la matrice 'GetLength(0)' n'est pas corrélée à la largeur 'width' du Blueprint"); }
                if (height != matrice.GetLength(1)) { throw new Exception("La hauteur de la matrice 'GetLength(1)' n'est pas corrélée à la hauteur 'height' du Blueprint"); }

                StringBuilder sb = new StringBuilder();
                sb.AppendLine("Class Blueprint");
                sb.AppendLine($"Width: {width}, Height: {height}");
                sb.AppendLine($"Position x: {position.X()}, y: {position.Y()}");

                if (matrice != null)
                {
                    sb.AppendLine("Matrix:");

                    for (int i = 0; i < width; i++)
                    {
                        for (int j = 0; j < height; j++)
                        {
                            sb.Append(matrice[i, j] + "\t");
                        }
                        sb.AppendLine();
                    }
                }
                else
                {
                    sb.AppendLine("Matrix: null");
                }

                return sb.ToString();
            }

            // Indicateurs
            /// <summary>
            /// Vérifie si le Blueprint est vide
            /// </summary>
            /// <returns>true si la matrice est nulle</returns>
            public bool IsEmpty()
            {
                // Vérifie si la matrice est null
                if (matrice == null)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            /// <summary>
            /// Vérifie si le Blueprint est transparent (si toutes les valeurs valent -1)
            /// </summary>
            /// <returns>true si le Blueprint est transparent</returns>
            public bool IsTransparent()
            {
                // Vérifie que la matrice n'est pas null
                if (matrice == null)
                {
                    throw new InvalidOperationException("La matrice est null. Impossible de déterminer si elle est transparente.");
                }

                // Parcourt la matrice pour vérifier si toutes les valeurs sont -1
                for (int i = 0; i < width; i++)
                {
                    for (int j = 0; j < height; j++)
                    {
                        if (matrice[i, j] != -1)
                        {
                            return false; // Si une valeur différente de -1 est trouvée, la matrice n'est pas transparente
                        }
                    }
                }

                return true; // Si aucune valeur différente de -1 n'est trouvée, la matrice est transparente
            }
            /// <summary>
            /// Méthode pour valider les positions
            /// </summary>
            /// <param name="x">cooordonnée x de la position à vérifier</param>
            /// <param name="y">cooordonnée y de la position à vérifier</param>
            /// <returns>true si les dimensions du blueprint couvrent cette position</returns>
            public bool IsValidPosition(int x, int y)
            {
                return position.X() <= x && x < position.X() + width &&
                       position.Y() <= y && y < position.Y() + height;
            }

            //// applications à la Matrice

            /// <summary>
            /// Indique si la matrice du blueprint contient une valeur où non
            /// </summary>
            /// <typeparam name="T">Le type de données de la matrice</typeparam>
            /// <param name="matrice">La matrice</param>
            /// <param name="valeur">La valeur recherchée dans la matrice</param>
            /// <returns>true si la valeur recherchée est dans la matrice, false sinon</returns>
            public bool Contient(int valeur) { return matrice.Contient(valeur); }

            /// <summary>
            /// Trouve les indices des cellules de la matrice du blueprint qui contiennent une valeur spécifique.
            /// </summary>
            /// <param name="valeur">La valeur recherchée dans la matrice du blueprint.</param>
            /// <returns>Une liste de tuples représentant les indices (ligne, colonne) des cellules contenant la valeur recherchée.</returns>
            public List<Paire<int>> Indices(int valeur) { return matrice.Indices(valeur); }

            /// <summary>
            /// Trouve la valeur maximale dans la matrice du blueprint.
            /// </summary>
            /// <returns>L'élément maximal de la matrice.</returns>
            /// <exception cref="ArgumentException">Lancé si la matrice est nulle ou vide.</exception> 
            public int Max() { return matrice.Max(); }

            /// <summary>
            /// Trouve la valeur minimale dans une matrice du blueprint.
            /// </summary>
            /// <returns>L'élément minimal de la matrice du blueprint.</returns>
            /// <exception cref="ArgumentException">Lancé si la matrice est nulle ou vide.</exception>
            public int Min() { return matrice.Min(); }

            /// <summary>
            /// Donne les indices des cellules  de la matrice du blueprint contenant la valeur maximale
            /// </summary>
            /// <returns>Liste d'est paires (ligne, colonne) des indices des cellules contenant la valeur maximale de la matrice</returns>
            public List<Paire<int>> IndicesMax() { return matrice.IndicesMax(); }

            /// <summary>
            /// Donne les indices des cellules  de la matrice du blueprint contenant la valeur minimale
            /// </summary>
            /// <returns>Liste d'est paires (ligne, colonne) des indices des cellules contenant la valeur minimale de la matrice</returns>
            public List<Paire<int>> IndicesMin() { return matrice.IndicesMin(); }

            //// applications au Dessin

            /// <summary>
            /// Fonction qui inverse le sens du blueprint en fonction d'un vecteur unitaire.
            /// Il faut peut être la mettre a jour
            /// </summary>
            /// <param name="direction">Le vecteur unitaire qui définit le(s) sens à inverser dans la matrice.</param>
            public void InverserSens(PaireInt direction)
            {
                // Erreur
                if (Math.Abs(direction.X()) != 1 || Math.Abs(direction.Y()) != 1)
                {
                    throw new ArgumentException("Les valeurs de la paire doivent êtres 1 ou -1", "direction");
                }

                // Copie temporaire de la matrice
                int[,] tempMatrix = new int[width, height];
                Array.Copy(matrice, tempMatrix, matrice.Length);

                // Inversion du sens en fonction du vecteur direction
                if (direction[0] == -1) // Inversion horizontale
                {
                    for (int i = 0; i < width; i++)
                    {
                        for (int j = 0; j < height; j++)
                        {
                            matrice[width - 1 - i, j] = tempMatrix[i, j];
                        }
                    }
                }
                if (direction[1] == -1) // Inversion verticale
                {
                    for (int i = 0; i < width; i++)
                    {
                        for (int j = 0; j < height; j++)
                        {
                            matrice[i, height - 1 - j] = tempMatrix[i, j];
                        }
                    }
                }
            }

            /// <summary>
            /// Redimensionne le blueprint à la taille spécifiée.
            /// Les nouvelles cases sont remplie avec la valeur voulue.
            /// Si les valeurs de la matrice d'origine dépassent de la nouvelle matrice, elles seront rognées et donc perdues.
            /// </summary>
            /// <param name="newWidth">La nouvelle largeur du blueprint.</param>
            /// <param name="newHeight">La nouvelle hauteur du blueprint.</param>
            /// <param name="decalage">Le vecteur de la différence des positions de l'encienne matrice et de la nouvelle matrice.</param>
            /// <param name="value">La valeur assignée aux nouvelles cases.</param>
            private void ResizeBlueprint(int newWidth, int newHeight, PaireInt newPosition = default, int value = -1)
            {
                // Creation d'un nouveau Blueprint (pour ne pas écraser les données de celui-ci)
                Blueprint resized = new Blueprint(newWidth, newHeight, position: newPosition);
                // Remplissage de la matrice
                for (int i = resized.Position().X(); i < resized.Position().X() + newWidth; i++)
                {
                    for (int j = resized.Position().Y(); j < resized.Position().Y() + newHeight; j++)
                    {
                        if (IsValidPosition(i,j))
                        {
                            resized[i, j] = this[i, j];
                        }
                        else
                        {
                            resized[i, j] = value;
                        }

                    }
                }

                // Assignation des valeurs
                matrice = resized.matrice;
                height = newHeight;
                width = newWidth;
                position = resized.Position();
            }

            /// <summary>
            /// Insère un blueprint dans un autre en fonction du comportement spécifié.
            /// Si le paramètre resize est vrai, la taille du blueprint est recalculée pour qu'il contienne tout ce qu'on souhaite ajouter, peu importe la position.
            /// </summary>
            /// <param name="blueprintToAdd">Le blueprint à insérer.</param>
            /// <param name="position">La position à laquelle insérer le blueprint.</param>
            /// <param name="insertionBehavior">Le comportement d'insertion à utiliser : "replace" ou "min".</param>
            /// <param name="resize">Indique si le blueprint doit être redimensionné pour contenir tout ce qu'on souhaite ajouter.</param>
            public void InsertToBlueprint(Blueprint blueprintToAdd, Paire<int> position = null, string insertionBehavior = "replace", bool resize = false)
            {
                // Erreur                
                if (blueprintToAdd.IsEmpty()) // Vérifie que le blueprint à ajouter n'est pas vide
                {
                    throw new ArgumentNullException("blueprintToAdd", "Le blueprint à ajouter est vide.");
                }
                if (IsEmpty())
                {
                    // initialise avec le blueprint à ajouter
                    SetMatrix(blueprintToAdd.matrice);
                    if (position != null)
                    {
                        SetPosition(PaireInt.Somme(blueprintToAdd.Position(), position));
                    }
                    else
                    {
                        SetPosition(blueprintToAdd.Position());
                    }
                    return;
                }

                // decalage
                if (position != null) { blueprintToAdd.SetPosition(PaireInt.Somme(blueprintToAdd.Position(), position)); } // Cela modifie le blueprint donné. la transformation est inversée à la toute fin de l'execution de cette fonction

                // Redimensionne le blueprint si Resize
                if (resize)
                {
                    // Coordonnees extremes
                    int minX = NumUtils.Min(this.position.X(), blueprintToAdd.Position().X());
                    int maxX = NumUtils.Max(width + this.position.X(), blueprintToAdd.Width() + blueprintToAdd.Position().X());
                    int minY = NumUtils.Min(this.position.Y(), blueprintToAdd.Position().Y());
                    int maxY = NumUtils.Max(height + this.position.Y(), blueprintToAdd.Height() + blueprintToAdd.Position().Y());
                    // Decalage
                    PaireInt newPosition = new PaireInt(
                        NumUtils.Min(this.position.X(), blueprintToAdd.Position().X()),
                        NumUtils.Min(this.position.Y(), blueprintToAdd.Position().Y()));
                    // Nouvelles dimensions
                    int newWidth = maxX - minX;
                    int newHeight = maxY - minY;

                    ResizeBlueprint(newWidth, newHeight, newPosition: newPosition);
                }


                //// Parcours des coordonnees du blueprint à insérer seulement
                // Recuperations des coordonnee a parcourrir
                int firstX, lastX, firstY, lastY;
                if (resize)
                {
                    firstX = blueprintToAdd.Position().X();
                    lastX = blueprintToAdd.Width() + blueprintToAdd.Position().X() - 1;
                    firstY = blueprintToAdd.Position().Y();
                    lastY = blueprintToAdd.Height() + blueprintToAdd.Position().Y() - 1;
                }
                else
                {
                    firstX = NumUtils.Max(this.position.X(), blueprintToAdd.Position().X());
                    lastX = NumUtils.Min(this.position.X() + width, blueprintToAdd.Position().X() + blueprintToAdd.Width()) - 1;
                    firstY = NumUtils.Max(this.position.Y(), blueprintToAdd.Position().Y());
                    lastY = NumUtils.Min(this.position.Y() + height, blueprintToAdd.Position().Y() + blueprintToAdd.Height()) - 1;
                }


                for (int i = firstX; i <= lastX; i++)
                {

                    for (int j = firstY; j <= lastY; j++)
                    {
                        // Récupération de la valeur à insérer
                        int valeur = blueprintToAdd[i, j];
                        // Vérification si non vide
                        if (valeur == -1)
                        {
                            continue;
                        }
                        // Initialisation de la valeur du blueprint d'accueil en fonction du comportement d'insertion
                        if (insertionBehavior == "replace" || (insertionBehavior == "min" && (this[i, j] == -1 || valeur < this[i, j])))
                        {
                            this[i, j] = valeur;
                        }
                    }
                }

                // Annulation du decalage
                if (position != null) { blueprintToAdd.SetPosition(PaireInt.Soustraction(blueprintToAdd.Position(), position)); }
            }

            /// <summary>
            /// Fonction qui crée un blueprint transparent et lui ajoute un motif aux positions souhaitées
            /// L'ajout se fait position par position dans l'ordre du tableau
            /// </summary>
            /// <param name="positions"> les positions auxquelles ajouter le motif </param>
            /// <param name="blueprintToAdd"> le motif à inserer dans le blueprint final </param>
            /// <returns></returns>
            public void InsertToBlueprint(Blueprint blueprintToAdd, Paire<int>[] positions, string insertionBehavior = "replace", bool resize = false)
            {
                // Erreur                
                if (blueprintToAdd.IsEmpty()) // Vérifie que le blueprint à ajouter n'est pas vide
                {
                    throw new ArgumentNullException("blueprintToAdd", "Le blueprint à ajouter est vide.");
                }

                if (IsEmpty())
                {
                    int minX = int.MaxValue, maxX = int.MinValue;
                    int minY = int.MaxValue, maxY = int.MinValue;
                    for (int i = 0; i < positions.Length; i++)
                    {
                        PaireInt position = positions[i];
                        if (minX > position[0]) { minX = position[0]; }
                        if (maxX < position[0]) { maxX = position[0]; }
                        if (minY > position[1]) { minY = position[1]; }
                        if (maxY < position[1]) { maxY = position[1]; }
                    }
                    int newWidth = -minX + maxX + blueprintToAdd.Width(); 
                    int newHeight = -minY + maxY + blueprintToAdd.Height();
                    PaireInt newPosition = new PaireInt(minX + blueprintToAdd.Position().X(), minY + blueprintToAdd.Position().Y());

                    Set(Transparent(newWidth, newHeight, newPosition));
                    resize = false;
                }

                // Redimensionne le blueprint si Resize
                if (resize)
                {
                    int minX = int.MinValue, maxX = int.MaxValue;
                    int minY = int.MinValue, maxY = int.MaxValue;
                    for (int i = 0; i < positions.Length; i++)
                    {
                        PaireInt position = positions[i];
                        if (minX > position[0]) { minX = position[0]; }
                        if (maxX < position[0]) { maxX = position[0]; }
                        if (minY > position[1]) { minY = position[1]; }
                        if (maxY < position[1]) { maxY = position[1]; }
                    }

                    int width = -minX + maxX + blueprintToAdd.Width();
                    int height = -minY + maxY + blueprintToAdd.Height();         
                    PaireInt newPosition = new PaireInt(
                        Fonctions.NumUtils.Min(position.X(), minX + blueprintToAdd.Position().X()),
                        Fonctions.NumUtils.Min(position.Y(), minY + blueprintToAdd.Position().Y()));

                    ResizeBlueprint(width, height, newPosition: newPosition);
                }


                //  Remplissage du blueprint
                for (int i = 0; i < positions.Length; i++)
                {
                    PaireInt positionAjout = positions[i];
                    int firstX = NumUtils.Max(position.X(), positionAjout.X() + blueprintToAdd.Position().X());
                    int lastX = NumUtils.Min(position.X() + width, positionAjout.X() + blueprintToAdd.Position().X() + blueprintToAdd.Width()) - 1;
                    int firstY = NumUtils.Max(position.Y(), positionAjout.Y() + blueprintToAdd.Position().Y());
                    int lastY = NumUtils.Min(position.Y() + height, positionAjout.Y() + blueprintToAdd.Position().Y() + blueprintToAdd.Height()) - 1;

                    for (int k = firstX; k <= lastX; k++)
                    {
                        for (int l = firstY; l <= lastY; l++)
                        {
                            // Récupération de la valeur à insérer
                            int valeur = blueprintToAdd[k-positionAjout.X(), l-positionAjout.Y()];
                            // Vérification si non vide
                            if (valeur == -1)
                            {
                                continue;
                            }
                            // Initialisation de la valeur du blueprint d'accueil en fonction du comportement d'insertion
                            if (insertionBehavior == "replace" || (insertionBehavior == "min" && (this[k, l] == -1 || valeur < this[k, l])))
                            {
                                this[k, l] = valeur;
                            }
                        }
                    }
                }
            }

            //// Fonctions : Constructeurs Specifiques

            /// <summary>
            /// Fonction qui construit et renvoie un Blueprint transparent (matrice remplie de -1).
            /// </summary>
            /// <param name="width">La largeur de la matrice du blueprint.</param>
            /// <param name="height">La hauteur de la matrice du blueprint. Par défaut, elle vaut length.</param>
            /// <param name="position">position de la valeur d'indice [0,0] dans le repere</param>
            public static Blueprint Transparent(int width, int height = -1, PaireInt position = null)
            {
                return new Blueprint(width, height, -1, position: position);
            }

            /// <summary>
            /// Fonction qui construit et renvoie un Blueprint avec une bordure et transparent à l'intérieur.
            /// </summary>
            /// <param name="width">La longueur de la matrice du blueprint.</param>
            /// <param name="height">La hauteur de la matrice du blueprint. Par défaut, elle vaut length.</param>
            /// <param name="value">La valeur donnee a la bordure. Par défaut, elle vaut 0.</param>
            /// <param name="borderWidth">La largeur de la bordure. Par défaut, elle vaut 1.</param>
            public static Blueprint Box(int width, int? height = null, int? value = 0, int? borderWidth = 1, PaireInt position = null)
            {
                // Erreur
                if (borderWidth.Value < 0) // Verification de la largeur de la bordure
                {
                    throw new ArgumentException("La largeur du bord doit être positive.", "borderWidth");
                }

                // Initialisation du Blueprint
                if (!height.HasValue) { height = width; }
                Blueprint box = Blueprint.Transparent(width, height.Value, position: position);

                // Initialisation des indices des valeurs appartenant a la bordure
                int[] X, Y;
                if (borderWidth.Value > box.Width() / 2.0)
                {
                    X = Fonctions.IntUtils.Sequence(0, box.Width());
                }
                else
                {
                    X = Fonctions.TableauUtils.Concatene(
                        Fonctions.IntUtils.Sequence(0, borderWidth.Value),
                        Fonctions.IntUtils.Sequence(box.Width() - 1, box.Width() - 1 - borderWidth.Value, -1)
                        );
                }
                if (borderWidth.Value > box.Height() / 2.0)
                {
                    Y = Fonctions.IntUtils.Sequence(0, box.Height());
                }
                else
                {
                    Y = Fonctions.TableauUtils.Concatene(
                        Fonctions.IntUtils.Sequence(0, borderWidth.Value),
                        Fonctions.IntUtils.Sequence(box.Height() - 1, box.Height() - 1 - borderWidth.Value, -1)
                        );
                }

                // Remplissage de la bordure
                int[,] boxMatrix = box.matrice;
                for (int i = 0; i < box.Width(); i++)
                {
                    for (int j = 0; j < box.Height(); j++)
                    {
                        if (Fonctions.TableauUtils.Contient(X, i) || Fonctions.TableauUtils.Contient(Y, j))
                        {
                            boxMatrix[i, j] = value.Value;
                        }
                    }
                }

                box.SetMatrix(boxMatrix);
                return box;
            }

            /// <summary>
            /// Fonction qui crée un blueprint représentant une courbe.
            /// La courbure est donnée par l'ajout successif de courbes sinusoïdales de longueures d'ondes décroissantes (on appelle ces opérations perturbations)
            /// </summary>
            /// <param name="dimensions">Les dimensions x et y du vecteur allant du point de départ au point d'arrivée</param>
            /// <param name="pattern">Le blueprint élémentait que va servir pour remplir le tracé calculé</param>
            /// <param name="courbure">Facteur des perturbations. Plus la valeur est petite, moins les perturbations serons importantes et réciproquement</param>
            /// <param name="sinuosite">Le nombre de pertubations apportées</param>
            /// <param name="randomSeed">La graine pour la génération aléatoire des perturbations</param>
            /// <return>Le blueprint représentant la courbe</returns>
            public static Blueprint Courbe(PaireInt dimensions, Blueprint pattern, float courbure, int sinuosite, int randomSeed)
            {
                // On copie dimension
                PaireInt coordinates = new PaireInt(dimensions);
                // Choisi si besoin d'inverser les axes X et Y
                bool flip = false;
                if (Math.Abs(coordinates.X()) < Math.Abs(coordinates.Y()))
                {
                    // Echange les axes pour la construction
                    int temp = coordinates[0];
                    coordinates[0] = coordinates[1];
                    coordinates[1] = temp;
                    flip = true;
                }
                
                
                // Crée un blueprint de taille suffisante pour contenir le trait (il pourrait être redimensionné si la courbe dépasse)
                // Recherche des coordonnées extremes
                int minX = Fonctions.NumUtils.Min(coordinates.X(), 0);
                int maxX = Fonctions.NumUtils.Max(0, coordinates.X());
                int minY = Fonctions.NumUtils.Min(coordinates.Y(), 0);
                int maxY = Fonctions.NumUtils.Max(0, coordinates.Y());
                // Dimensions du blueprint basées sur les coordonnées maximales
                int length = -minX + maxX + pattern.Width();
                int height = -minY + maxY + pattern.Height();
                // Initialisation du blueprint
                Blueprint courbeBlueprint = Blueprint.Transparent(length, height, position: new PaireInt(minX + pattern.Position().X(), minY + pattern.Position().Y()));



                ////  Remplissage du blueprint
                int i = 0, j = 0; // initialisation de la boucle

                if (coordinates.X() != 0) // On évite une division par zero
                {
                    // Vecteur unitaire de direction
                    Paire<double> direction = coordinates.Unitaire();
                    // direction d'increment
                    int incrementI = Fonctions.NumUtils.Signe(direction.X());
                    int incrementJ;
                    // abscisse 
                    int lastI = incrementI > 0 ? maxX : minX;
                    // ordonnee à atteindre pour chaque abscisse
                    float limJ;
                    // Boucle de remplissage du Blueprint
                    while ((incrementI > 0 && i <= lastI) || (incrementI < 0 && i >= lastI))
                    {
                        float ordonnee = (i + incrementI) * (float)direction.Y() / (float)direction.X(); // calcul de l'ordonnee sur la droite suivant les coordonnees choisies
                        float perturbation =  (float)Fonctions.MathUtils.SommeSinusoidales(i,sinuosite,(maxX-minX)*2,0.2f,randomSeed); // Somme de sinusoïdes faisant onduler le trait
                        limJ = ordonnee + courbure * perturbation; 
                        incrementJ = j < limJ ? 1 : -1; // calcule le sens d'incrémentation en fonction de la limite à atteindre
                        do
                        {
                            courbeBlueprint.InsertToBlueprint(pattern, new PaireInt(i, j), insertionBehavior: "min", resize:true);
                            j += incrementJ;
                        } while ((incrementJ > 0 && j <= limJ) || (incrementJ < 0 && j >= limJ));
                        j -= incrementJ; // On décrémente j pour qu'il soit le même à l'abscisse suivante
                        i += incrementI;
                    }
                }
                else
                {
                    throw new Exception("x ne peut être égal à 0. Il faut reprendre le code");
                }

                // flip axes

                if (flip)
                {
                    // Calcul de la position
                    PaireInt tempPosition = new PaireInt(courbeBlueprint.Position().Y(),courbeBlueprint.Position().X());
                    courbeBlueprint.SetPosition(new PaireInt());
                    Blueprint temp = new Blueprint(courbeBlueprint.Height(), courbeBlueprint.Width());
                    for (i = 0; i< courbeBlueprint.Width(); i++)
                    {
                        for (j = 0; j < courbeBlueprint.Height(); j++)
                        {
                            temp[j,i] = courbeBlueprint[i, j];
                        }
                    }
                    courbeBlueprint = temp;
                    courbeBlueprint.SetPosition(tempPosition);
                }
                
                return courbeBlueprint;
            }
            public static Blueprint Courbe(PaireInt dimensions, Blueprint pattern)
            {
                return Courbe(dimensions, pattern, 0, 0, 0);
            }

        }
    }
}