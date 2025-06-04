using System;

namespace NutriTrack.Models
{
    public enum MealType
    {
        Breakfast,
        Lunch,
        Dinner,
        Snack
    }

    public class MealEntry
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid ProductId { get; set; }

        private double _weight;
        public double Weight
        {
            get => _weight;
            set
            {
                if (value <= 0)
                    throw new ArgumentException("Weight must be greater than zero.");
                _weight = value;
            }
        }

        public MealType MealType { get; set; }

        private DateTime _date;
        public DateTime Date
        {
            get => _date;
            set
            {
                // Allow dates up to the end of the current day
                var maxDate = DateTime.Today.AddDays(1).AddSeconds(-1);
                if (value > maxDate)
                    throw new ArgumentException("Date cannot be in the future.");
                _date = value;
            }
        }
    }
}