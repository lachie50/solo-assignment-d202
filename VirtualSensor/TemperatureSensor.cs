using System;
using System.Collections.Generic;
using System.Linq;

namespace Sensors
{
    public class TemperatureSensor
    {
        // Sensor properties
        public string Name { get; private set; }
        public string Location { get; private set; }
        public double MinValue { get; private set; }
        public double MaxValue { get; private set; }

        // Operational state
        private bool _isRunning;
        private List<Reading> _dataHistory;
        private Random _random;
        private double _currentTemperature;
        private bool _faultInjected;

        // Configuration constants
        private const double NoiseLevel = 0.3;
        private const double AnomalyThreshold = 1.5;
        private const int SmoothingWindowSize = 5;

        public TemperatureSensor()
        {
            _dataHistory = new List<Reading>();
            _random = new Random();
            _isRunning = false;
            _faultInjected = false;
        }

        /// <summary>
        /// Initialize sensor with configuration data
        /// </summary>
        public void InitializeSensor(string name, string location, double minValue, double maxValue)
        {
            // Validate inputs
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Sensor name cannot be empty", nameof(name));

            if (string.IsNullOrWhiteSpace(location))
                throw new ArgumentException("Location cannot be empty", nameof(location));

            if (minValue >= maxValue)
                throw new ArgumentException("MinValue must be less than MaxValue");

            if (minValue < -273.15) // Absolute zero check
                throw new ArgumentException("Temperature cannot be below absolute zero");

            Name = name;
            Location = location;
            MinValue = minValue;
            MaxValue = maxValue;
            _currentTemperature = (minValue + maxValue) / 2;

            Console.WriteLine($"[INIT] Sensor '{Name}' initialized at {Location}");
            Console.WriteLine($"[INIT] Range: {MinValue}°C to {MaxValue}°C");
        }
        /// <summary>
        /// Start the sensor operation
        /// </summary>
        public void StartSensor()
        {
            if (_isRunning)
            {
                Console.WriteLine($"[WARNING] Sensor '{Name}' is already running");
                return;
            }

            _isRunning = true;
            Console.WriteLine($"[START] Sensor '{Name}' started successfully");
        }

        /// <summary>
        /// Simulate a temperature reading with noise
        /// </summary>
        public double SimulateData()
        {
            if (!_isRunning)
                throw new InvalidOperationException("Sensor must be started before simulating data");

            double temperature;

            if (_faultInjected)
            {
                // Simulate exponential temperature rise (cooling failure)
                temperature = _currentTemperature + _random.NextDouble() * 2;
                if (temperature > MaxValue + 10)
                    temperature = MaxValue + 10; // Stabilize at higher value
            }
            else
            {
                // Normal operation: target temperature with noise
                double targetTemp = (MinValue + MaxValue) / 2;
                double noise = (_random.NextDouble() * 2 - 1) * NoiseLevel;
                temperature = targetTemp + noise;

                // Occasional random spikes (5% chance)
                if (_random.NextDouble() < 0.05)
                {
                    temperature += _random.NextDouble() * 3 * (_random.Next(2) == 0 ? 1 : -1);
                }
            }

            _currentTemperature = temperature;
            return Math.Round(temperature, 2);
        }
        /// <summary>
        /// Validate if temperature reading is within acceptable range
        /// </summary>
        public bool ValidateData(Reading sensorData)
        {
            if (sensorData == null)
                throw new ArgumentNullException(nameof(sensorData));

            bool isValid = sensorData.Value >= MinValue && sensorData.Value <= MaxValue;

            if (!isValid)
            {
                Console.WriteLine($"[VALIDATION] FAILED - Reading {sensorData.Value:F2}°C out of range [{MinValue}, {MaxValue}]");
            }

            return isValid;
        }

        /// <summary>
        /// Log sensor data with timestamp
        /// </summary>
        public void LogData(Reading sensorData)
        {
            if (sensorData == null)
                throw new ArgumentNullException(nameof(sensorData));

            string status = ValidateData(sensorData) ? "VALID" : "INVALID";
            string logMessage = $"[{sensorData.DateTime:yyyy-MM-dd HH:mm:ss}] {sensorData.SensorName} | {sensorData.Value:F2}°C | Status: {status}";

            Console.WriteLine(logMessage);

            // In a real application, you would also write to a file:
            // File.AppendAllText("sensor_log.txt", logMessage + Environment.NewLine);
        }

        /// <summary>
        /// Store reading in history for analysis and database
        /// </summary>
        public void StoreData(Reading sensorData)
        {
            if (sensorData == null)
                throw new ArgumentNullException(nameof(sensorData));

            // Store in memory for quick access
            _dataHistory.Add(sensorData);

            // Keep only last 100 readings in memory to prevent memory issues
            if (_dataHistory.Count > 100)
            {
                _dataHistory.RemoveAt(0);
            }

            // Store in database for permanent storage
            // DatabaseService.StoreReading(sensorData);
        }

        /// <summary>
        /// Apply moving average smoothing to reduce noise
        /// </summary>
        public double SmoothData()
        {
            if (_dataHistory.Count == 0)
                return 0;

            int windowSize = Math.Min(SmoothingWindowSize, _dataHistory.Count);
            var recentReadings = _dataHistory.TakeLast(windowSize);
            double smoothedValue = recentReadings.Average(r => r.Value);

            return Math.Round(smoothedValue, 2);
        }

        /// <summary>
        /// Detect anomalies based on deviation from recent average
        /// </summary>
        public bool DetectAnomaly(Reading sensorData)
        {
            if (sensorData == null)
                throw new ArgumentNullException(nameof(sensorData));

            if (_dataHistory.Count < 5)
                return false; // Not enough data

            double recentAverage = _dataHistory.TakeLast(10).Average(r => r.Value);
            double deviation = Math.Abs(sensorData.Value - recentAverage);

            bool isAnomaly = deviation > AnomalyThreshold;

            if (isAnomaly)
            {
                Console.WriteLine($"[ANOMALY] Detected! Current: {sensorData.Value:F2}°C, Average: {recentAverage:F2}°C, Deviation: {deviation:F2}°C");
            }

            return isAnomaly;
        }

        /// <summary>
        /// Check if reading exceeds defined thresholds
        /// </summary>
        public bool CheckThreshold(Reading sensorData, double minThreshold, double maxThreshold)
        {
            if (sensorData == null)
                throw new ArgumentNullException(nameof(sensorData));

            bool exceeded = sensorData.Value < minThreshold || sensorData.Value > maxThreshold;

            if (exceeded)
            {
                Console.WriteLine($"[ALERT] Threshold exceeded! Current: {sensorData.Value:F2}°C, Thresholds: [{minThreshold}, {maxThreshold}]");
            }

            return exceeded;
        }

        /// <summary>
        /// Get current data history count
        /// </summary>
        public int GetHistoryCount()
        {
            return _dataHistory.Count;
        }

        /// <summary>
        /// Get data history (for testing and analysis)
        /// </summary>
        public IReadOnlyList<Reading> GetHistory()
        {
            return _dataHistory.AsReadOnly();
        }
    }
}