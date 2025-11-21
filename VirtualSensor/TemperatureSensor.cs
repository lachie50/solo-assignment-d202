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
    }
}