using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AngouriMath;

namespace CSInterpolation
{
    public class CSLagrange
    {
        private string result = "";
        public double[] coefficients { get; private set; }
        public CSLagrange(List<double> x, List<double> y)
        {
            int degree = x.Count - 1;
            coefficients = new double[degree + 1];

            for (int i = 0; i < y.Count; ++i)
            {
                // Oblicz wielomian L_i(x) i pomnóż przez y[i]
                double[] liCoefficients = new double[degree + 1];
                liCoefficients[0] = 1; // L_i(x) zaczyna się od 1 jako stała

                for (int j = 0; j < x.Count; ++j)
                {
                    if (i != j)
                    {
                        double denominator = x[i] - x[j];
                        double[] newCoefficients = new double[degree + 1];

                        // Przesunięcie współczynników i dodanie nowego składnika
                        for (int k = degree; k >= 0; --k)
                        {
                            if (k > 0)
                            {
                                newCoefficients[k] += liCoefficients[k - 1];
                            }
                            newCoefficients[k] -= liCoefficients[k] * x[j];
                        }

                        // Dzielimy przez denominator
                        for (int k = 0; k <= degree; ++k)
                        {
                            newCoefficients[k] /= denominator;
                        }

                        liCoefficients = newCoefficients;
                    }
                }

                // Dodajemy y[i] * L_i(x) do końcowego wielomianu
                for (int k = 0; k <= degree; ++k)
                {
                    coefficients[k] += y[i] * liCoefficients[k];
                }
            }

            // Przekształcamy tablicę współczynników na postać tekstową
            for (int i = coefficients.Length - 1; i > 1; --i)
            {
                double coefR = Math.Round(coefficients[i], 2);
                if (coefR != 0)
                {
                    if(coefR != 1)
                        result += coefR;
                    result+= "x^" + i + " + ";
                }
            }
            result += Math.Round(coefficients[1], 2) + "x + ";
            result += Math.Round(coefficients[0], 2);
        }
        public string getResult() { return result; }
    }
}
