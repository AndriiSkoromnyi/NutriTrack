using System;

namespace NutriTrack.Models
{
    public enum WeightUnit
    {
        Grams,
        Ounces
    }

    public class UserSettings
    {
        private double _dailyCalorieGoal;
        public double DailyCalorieGoal
        {
            get => _dailyCalorieGoal;
            set
            {
                if (value <= 0)
                    throw new ArgumentException("Цель по калориям должна быть больше нуля.");
                _dailyCalorieGoal = value;
            }
        }

        public WeightUnit WeightUnit { get; set; } = WeightUnit.Grams;
    }
}