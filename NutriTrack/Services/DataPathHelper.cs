using System;
using System.IO;
using System.Reflection;

namespace NutriTrack.Services
{
    public static class DataPathHelper
    {
        public static string GetDataPath()
        {
            // Get the directory where the application is running
            var assemblyLocation = Assembly.GetExecutingAssembly().Location;
            var baseDirectory = Path.GetDirectoryName(assemblyLocation);
            
            // Create a Data directory in the application directory
            var dataPath = Path.Combine(baseDirectory, "Data");
            Directory.CreateDirectory(dataPath);
            
            return dataPath;
        }
    }
} 