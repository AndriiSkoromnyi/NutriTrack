using System;
using System.Collections.Generic;

namespace NutriTrack.Models
{
    public class DailySummary
    {
        public DateTime Date { get; set; }

        public double TotalCalories { get; set; }

        public double TotalProtein { get; set; }

        public double TotalFat { get; set; }

        public double TotalCarbohydrates { get; set; }

        public List<MealEntry> MealEntries { get; set; } = new List<MealEntry>();
    }
}