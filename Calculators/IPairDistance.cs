using System.Collections.Generic;

namespace AI.Calculators
{
    interface IPairDistance
    {
        double CalculateDistance(KeyValuePair<double, double> pair);
        double OuterFunction(double pairDistanceSum);
    }
}
