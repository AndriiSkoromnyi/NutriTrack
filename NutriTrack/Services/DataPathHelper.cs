using System;
using System.IO;
using System.Reflection;

namespace NutriTrack.Services
{
    public static class DataPathHelper
    {
        private static string _dataPath;

        public static string GetDataPath()
        {
            if (_dataPath != null)
            {
                return _dataPath;
            }

            // Get the directory where the application is running
            var assemblyLocation = Assembly.GetExecutingAssembly().Location;
            var baseDirectory = Path.GetDirectoryName(assemblyLocation);
            
            // Create a Data directory in the application directory
            _dataPath = Path.Combine(baseDirectory, "Data");
            Directory.CreateDirectory(_dataPath);
            
            return _dataPath;
        }
    }
} 