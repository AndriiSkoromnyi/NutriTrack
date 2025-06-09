using NutriTrack.Models;

namespace NutriTrack.Services
{
    public interface IWeightConversionService
    {
        double Convert(double value, WeightUnit from, WeightUnit to);
        string FormatWeight(double value, WeightUnit unit);
    }

    public class WeightConversionService : IWeightConversionService
    {
        private const double GramsPerOunce = 28.3495;

        public double Convert(double value, WeightUnit from, WeightUnit to)
        {
            if (from == to) return value;

            return from == WeightUnit.Grams
                ? value / GramsPerOunce // Grams to Ounces
                : value * GramsPerOunce; // Ounces to Grams
        }

        public string FormatWeight(double value, WeightUnit unit)
        {
            return unit switch
            {
                WeightUnit.Grams => $"{value:F1}g",
                WeightUnit.Ounces => $"{value:F1}oz",
                _ => $"{value:F1}"
            };
        }
    }
} 