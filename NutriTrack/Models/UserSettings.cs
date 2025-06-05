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
        private double _dailyCalorieGoal = 2000;
        public double DailyCalorieGoal
        {
            get => _dailyCalorieGoal;
            set
            {
                if (value < 0)
                    throw new ArgumentException("Daily calorie goal cannot be negative.");
                _dailyCalorieGoal = value;
            }
        }

        private double _dailyProteinGoal = 75;
        public double DailyProteinGoal
        {
            get => _dailyProteinGoal;
            set
            {
                if (value < 0)
                    throw new ArgumentException("Daily protein goal cannot be negative.");
                _dailyProteinGoal = value;
            }
        }

        private double _dailyFatGoal = 70;
        public double DailyFatGoal
        {
            get => _dailyFatGoal;
            set
            {
                if (value < 0)
                    throw new ArgumentException("Daily fat goal cannot be negative.");
                _dailyFatGoal = value;
            }
        }

        private double _dailyCarbsGoal = 250;
        public double DailyCarbsGoal
        {
            get => _dailyCarbsGoal;
            set
            {
                if (value < 0)
                    throw new ArgumentException("Daily carbs goal cannot be negative.");
                _dailyCarbsGoal = value;
            }
        }

        public WeightUnit WeightUnit { get; set; } = WeightUnit.Grams;
    }
}