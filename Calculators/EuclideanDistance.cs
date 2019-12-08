using System;
using System.Collections.Generic;

namespace AI.Calculators
{
    class EuclideanDistance : IPairDistance
    {
        public double CalculateDistance(KeyValuePair<double, double> pair)
        {
            return Math.Pow(pair.Key - pair.Value, 2);
        }

        public double OuterFunction(double pairDistanceSum)
        {
            return Math.Sqrt(pairDistanceSum);
        }
    }
}
