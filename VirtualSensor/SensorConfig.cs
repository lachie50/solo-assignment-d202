using System;
using System.IO;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Sensors
{
    public class SensorConfiguration
    {
        public string Name { get; set; }
        public string Location { get; set; }
        public double MinValue { get; set; }
        public double MaxValue { get; set; }
        public double MinThreshold { get; set; }
        public double MaxThreshold { get; set; }
    }

    public static class ConfigLoader
    {
        /// <summary>
        /// Load and validate sensor configuration from YAML file
        /// </summary>
        public static SensorConfiguration LoadFromFile(string filePath)
        {
            // Security: Validate file path to prevent directory traversal
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("File path cannot be empty", nameof(filePath));

            // Sanitize path - remove any suspicious characters
            if (filePath.Contains("..") || filePath.Contains("~"))
                throw new SecurityException("Invalid file path - directory traversal not allowed");

            // Check file exists
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"Configuration file not found: {filePath}");

            try
            {
                string yamlContent = File.ReadAllText(filePath);

                // Parse YAML
                var deserializer = new DeserializerBuilder()
                    .WithNamingConvention(CamelCaseNamingConvention.Instance)
                    .Build();

                var config = deserializer.Deserialize<SensorConfiguration>(yamlContent);

                // Validate configuration
                ValidateConfiguration(config);

                return config;
            }
            catch (YamlDotNet.Core.YamlException ex)
            {
                throw new InvalidOperationException($"Failed to parse YAML configuration: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error loading configuration: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Validate configuration values
        /// </summary>
        private static void ValidateConfiguration(SensorConfiguration config)
        {
            if (config == null)
                throw new InvalidOperationException("Configuration is null");

            if (string.IsNullOrWhiteSpace(config.Name))
                throw new InvalidOperationException("Sensor name is required");

            if (string.IsNullOrWhiteSpace(config.Location))
                throw new InvalidOperationException("Location is required");

            if (config.MinValue >= config.MaxValue)
                throw new InvalidOperationException($"MinValue ({config.MinValue}) must be less than MaxValue ({config.MaxValue})");

            if (config.MinThreshold >= config.MaxThreshold)
                throw new InvalidOperationException($"MinThreshold ({config.MinThreshold}) must be less than MaxThreshold ({config.MaxThreshold})");
        }

        /// <summary>
        /// Create a sample configuration file
        /// </summary>
        public static void CreateSampleConfig(string filePath)
        {
            var sampleConfig = new SensorConfiguration
            {
                Name = "DataCenter-Sensor-01",
                Location = "Server Room A",
                MinValue = 22.0,
                MaxValue = 24.0,
                MinThreshold = 21.0,
                MaxThreshold = 25.0
            };

            var serializer = new SerializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();

            string yaml = serializer.Serialize(sampleConfig);
            File.WriteAllText(filePath, yaml);

            Console.WriteLine($"Sample configuration created: {filePath}");
        }
    }

    public class SecurityException : Exception
    {
        public SecurityException(string message) : base(message) { }
    }
}

