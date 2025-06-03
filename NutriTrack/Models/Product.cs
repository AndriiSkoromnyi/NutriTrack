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

        public Guid Id { get; set; } = Guid.NewGuid();

        private string _name;
        public string Name
        {
            get => _name;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Product name cannot be empty.");
                if (_name != value)
                {
                    _name = value;
                    OnPropertyChanged();
                }
            }
        }

        private double _caloriesPer100g;
        public double CaloriesPer100g
        {
            get => _caloriesPer100g;
            set
            {
                if (value < 0)
                    throw new ArgumentException("Calories cannot be negative.");
                if (_caloriesPer100g != value)
                {
                    _caloriesPer100g = value;
                    OnPropertyChanged();
                }
            }
        }

        private double _protein;
        public double Protein
        {
            get => _protein;
            set
            {
                if (value < 0)
                    throw new ArgumentException("Protein cannot be negative.");
                if (_protein != value)
                {
                    _protein = value;
                    OnPropertyChanged();
                }
            }
        }

        private double _fat;
        public double Fat
        {
            get => _fat;
            set
            {
                if (value < 0)
                    throw new ArgumentException("Fat cannot be negative.");
                if (_fat != value)
                {
                    _fat = value;
                    OnPropertyChanged();
                }
            }
        }

        private double _carbohydrates;
        public double Carbohydrates
        {
            get => _carbohydrates;
            set
            {
                if (value < 0)
                    throw new ArgumentException("Carbohydrates cannot be negative.");
                if (_carbohydrates != value)
                {
                    _carbohydrates = value;
                    OnPropertyChanged();
                }
            }
        }
    }
}