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
                    throw new ArgumentException("Вес должен быть больше нуля.");
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
                if (value > DateTime.Now)
                    throw new ArgumentException("Дата не может быть в будущем.");
                _date = value;
            }
        }
    }
}