
using System;
using System.Threading;

namespace Sensors
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine("=== Temperature Sensor Simulation ===\n");

            try
            {

                // Configuration file path
                string configPath = "sensor_config.yaml";

                // Create sample config if it doesn't exist
                if (!System.IO.File.Exists(configPath))
                {
                    Console.WriteLine("Configuration file not found. Creating sample configuration...\n");
                    ConfigLoader.CreateSampleConfig(configPath);
                }

                // Load configuration
                Console.WriteLine($"Loading configuration from {configPath}...");
                var config = ConfigLoader.LoadFromFile(configPath);
                Console.WriteLine("Configuration loaded successfully!\n");

                // Initialize sensor
                var sensor = new TemperatureSensor();
                sensor.InitializeSensor(
                    config.Name,
                    config.Location,
                    config.MinValue,
                    config.MaxValue
                );

                // Start sensor
                sensor.StartSensor();
                Console.WriteLine();

                // Variables for simulation
                int readingCount = 0;
                int faultCycle = 0;
                bool faultActive = false;

                Console.WriteLine("Simulation running... (Press Ctrl+C to stop)\n");
                Console.WriteLine($"Normal Range: {config.MinValue}°C - {config.MaxValue}°C");
                Console.WriteLine($"Alert Thresholds: {config.MinThreshold}°C - {config.MaxThreshold}°C\n");

                // Simulation loop
                while (true)
                {
                    readingCount++;

                    // Inject fault every 50 readings (simulate cooling failure)
                    if (readingCount % 50 == 0 && !faultActive)
                    {
                        sensor.InjectFault();
                        faultActive = true;
                        faultCycle = 0;
                    }

                    // Clear fault after 10 cycles
                    if (faultActive)
                    {
                        faultCycle++;
                        if (faultCycle >= 10)
                        {
                            sensor.ClearFault();
                            faultActive = false;
                        }
                    }

                    // Simulate temperature reading
                    double temperature = sensor.SimulateData();

                    // Create reading object
                    var reading = new Reading
                    {
                        ReadingId = readingCount,
                        SensorName = config.Name,
                        Value = temperature,
                        DateTime = DateTime.Now
                    };

                    // Validate data
                    bool isValid = sensor.ValidateData(reading);

                    // Store in history
                    sensor.StoreData(reading);

                    // Log the data
                    sensor.LogData(reading);

                    // Check for anomalies
                    if (sensor.GetHistoryCount() >= 5)
                    {
                        sensor.DetectAnomaly(reading);
                    }

                    // Check thresholds
                    sensor.CheckThreshold(reading, config.MinThreshold, config.MaxThreshold);

                    // Display smoothed data every 10 readings
                    if (readingCount % 10 == 0 && sensor.GetHistoryCount() >= 5)
                    {
                        double smoothed = sensor.SmoothData();
                        Console.WriteLine($"[SMOOTHED] Moving average (last 5): {smoothed:F2}°C");
                    }

                    // Display statistics every 20 readings
                    if (readingCount % 20 == 0)
                    {
                        Console.WriteLine($"\n--- Statistics (Total readings: {readingCount}) ---");
                        Console.WriteLine($"History stored: {sensor.GetHistoryCount()} readings\n");
                    }

                    // Wait 1 second between readings
                    Thread.Sleep(1000);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n[ERROR] Application failed: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }
    }
}