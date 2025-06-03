using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace NutriTrack.Models
{
    public class Product : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        public Product()
        {
            Id = Guid.NewGuid();
            Name = "New Product";
        }

        public Guid Id { get; set; }

        private string _name;
        public string Name
        {
            get => _name;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Название продукта не может быть пустым.");
                SetProperty(ref _name, value);
            }
        }

        private double _caloriesPer100g;
        public double CaloriesPer100g
        {
            get => _caloriesPer100g;
            set
            {
                if (value < 0)
                    throw new ArgumentException("Калорийность не может быть отрицательной.");
                SetProperty(ref _caloriesPer100g, value);
            }
        }

        private double _protein;
        public double Protein
        {
            get => _protein;
            set
            {
                if (value < 0)
                    throw new ArgumentException("Белки не могут быть отрицательными.");
                SetProperty(ref _protein, value);
            }
        }

        private double _fat;
        public double Fat
        {
            get => _fat;
            set
            {
                if (value < 0)
                    throw new ArgumentException("Жиры не могут быть отрицательными.");
                SetProperty(ref _fat, value);
            }
        }

        private double _carbohydrates;
        public double Carbohydrates
        {
            get => _carbohydrates;
            set
            {
                if (value < 0)
                    throw new ArgumentException("Углеводы не могут быть отрицательными.");
                SetProperty(ref _carbohydrates, value);
            }
        }
    }
}