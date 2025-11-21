using System;
using Xunit;
using Sensors;

namespace Tests
{
    public class TemperatureSensorTests
    {
        [Fact]
        public void InitializeSensor_ValidInputs_Success()
        {
            // Arrange
            var sensor = new TemperatureSensor();

            // Act
            sensor.InitializeSensor("TestSensor", "Room A", 20.0, 25.0);

            // Assert
            Assert.Equal("TestSensor", sensor.Name);
            Assert.Equal("Room A", sensor.Location);
            Assert.Equal(20.0, sensor.MinValue);
            Assert.Equal(25.0, sensor.MaxValue);
        }

        [Fact]
        public void InitializeSensor_EmptyName_ThrowsException()
        {
            // Arrange
            var sensor = new TemperatureSensor();

            // Act & Assert
            Assert.Throws<ArgumentException>(() =>
                sensor.InitializeSensor("", "Room A", 20.0, 25.0));
        }

        [Fact]
        public void InitializeSensor_EmptyLocation_ThrowsException()
        {
            // Arrange
            var sensor = new TemperatureSensor();

            // Act & Assert
            Assert.Throws<ArgumentException>(() =>
                sensor.InitializeSensor("TestSensor", "", 20.0, 25.0));
        }

        [Fact]
        public void InitializeSensor_MinGreaterThanMax_ThrowsException()
        {
            // Arrange
            var sensor = new TemperatureSensor();

            // Act & Assert
            Assert.Throws<ArgumentException>(() =>
                sensor.InitializeSensor("TestSensor", "Room A", 25.0, 20.0));
        }

        [Fact]
        public void InitializeSensor_BelowAbsoluteZero_ThrowsException()
        {
            // Arrange
            var sensor = new TemperatureSensor();

            // Act & Assert
            Assert.Throws<ArgumentException>(() =>
                sensor.InitializeSensor("TestSensor", "Room A", -300.0, 25.0));
        }

        [Fact]
        public void StartSensor_SetsRunningState()
        {
            // Arrange
            var sensor = new TemperatureSensor();
            sensor.InitializeSensor("TestSensor", "Room A", 20.0, 25.0);

            // Act
            sensor.StartSensor();

            // Assert - sensor should be running (tested via SimulateData)
            double temp = sensor.SimulateData();
            Assert.True(temp > 0 || temp < 0); // Just verify it returns a value
        }

        [Fact]
        public void SimulateData_WithoutStart_ThrowsException()
        {
            // Arrange
            var sensor = new TemperatureSensor();
            sensor.InitializeSensor("TestSensor", "Room A", 20.0, 25.0);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => sensor.SimulateData());
        }

        [Fact]
        public void SimulateData_ReturnsTemperatureInReasonableRange()
        {
            // Arrange
            var sensor = new TemperatureSensor();
            sensor.InitializeSensor("TestSensor", "Room A", 22.0, 24.0);
            sensor.StartSensor();

            // Act
            double temp = sensor.SimulateData();

            // Assert - should be near the target range (allowing for noise)
            Assert.InRange(temp, 20.0, 26.0);
        }

        [Fact]
        public void ValidateData_ValidReading_ReturnsTrue()
        {
            // Arrange
            var sensor = new TemperatureSensor();
            sensor.InitializeSensor("TestSensor", "Room A", 20.0, 25.0);
            var reading = new Reading
            {
                SensorName = "TestSensor",
                Value = 22.5,
                DateTime = DateTime.Now
            };

            // Act
            bool isValid = sensor.ValidateData(reading);

            // Assert
            Assert.True(isValid);
        }

        [Fact]
        public void ValidateData_OutOfRangeHigh_ReturnsFalse()
        {
            // Arrange
            var sensor = new TemperatureSensor();
            sensor.InitializeSensor("TestSensor", "Room A", 20.0, 25.0);
            var reading = new Reading
            {
                SensorName = "TestSensor",
                Value = 30.0,
                DateTime = DateTime.Now
            };

            // Act
            bool isValid = sensor.ValidateData(reading);

            // Assert
            Assert.False(isValid);
        }

        [Fact]
        public void ValidateData_OutOfRangeLow_ReturnsFalse()
        {
            // Arrange
            var sensor = new TemperatureSensor();
            sensor.InitializeSensor("TestSensor", "Room A", 20.0, 25.0);
            var reading = new Reading
            {
                SensorName = "TestSensor",
                Value = 15.0,
                DateTime = DateTime.Now
            };

            // Act
            bool isValid = sensor.ValidateData(reading);

            // Assert
            Assert.False(isValid);
        }

        [Fact]
        public void ValidateData_NullReading_ThrowsException()
        {
            // Arrange
            var sensor = new TemperatureSensor();
            sensor.InitializeSensor("TestSensor", "Room A", 20.0, 25.0);

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => sensor.ValidateData(null));
        }

        [Fact]
        public void StoreData_AddsToHistory()
        {
            // Arrange
            var sensor = new TemperatureSensor();
            sensor.InitializeSensor("TestSensor", "Room A", 20.0, 25.0);
            var reading = new Reading
            {
                SensorName = "TestSensor",
                Value = 22.5,
                DateTime = DateTime.Now
            };

            // Act
            sensor.StoreData(reading);

            // Assert
            Assert.Equal(1, sensor.GetHistoryCount());
        }

        [Fact]
        public void StoreData_NullReading_ThrowsException()
        {
            // Arrange
            var sensor = new TemperatureSensor();
            sensor.InitializeSensor("TestSensor", "Room A", 20.0, 25.0);

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => sensor.StoreData(null));
        }

        [Fact]
        public void SmoothData_EmptyHistory_ReturnsZero()
        {
            // Arrange
            var sensor = new TemperatureSensor();
            sensor.InitializeSensor("TestSensor", "Room A", 20.0, 25.0);

            // Act
            double smoothed = sensor.SmoothData();

            // Assert
            Assert.Equal(0, smoothed);
        }

        [Fact]
        public void SmoothData_WithData_ReturnsAverage()
        {
            // Arrange
            var sensor = new TemperatureSensor();
            sensor.InitializeSensor("TestSensor", "Room A", 20.0, 25.0);

            // Add 5 readings
            for (int i = 0; i < 5; i++)
            {
                var reading = new Reading
                {
                    SensorName = "TestSensor",
                    Value = 22.0 + i,
                    DateTime = DateTime.Now
                };
                sensor.StoreData(reading);
            }

            // Act
            double smoothed = sensor.SmoothData();

            // Assert - average of 22, 23, 24, 25, 26 = 24
            Assert.Equal(24.0, smoothed);
        }

        [Fact]
        public void DetectAnomaly_InsufficientData_ReturnsFalse()
        {
            // Arrange
            var sensor = new TemperatureSensor();
            sensor.InitializeSensor("TestSensor", "Room A", 20.0, 25.0);
            var reading = new Reading
            {
                SensorName = "TestSensor",
                Value = 22.5,
                DateTime = DateTime.Now
            };

            // Act
            bool isAnomaly = sensor.DetectAnomaly(reading);

            // Assert
            Assert.False(isAnomaly);
        }

        [Fact]
        public void DetectAnomaly_LargeDeviation_ReturnsTrue()
        {
            // Arrange
            var sensor = new TemperatureSensor();
            sensor.InitializeSensor("TestSensor", "Room A", 20.0, 25.0);

            // Add stable readings
            for (int i = 0; i < 10; i++)
            {
                sensor.StoreData(new Reading
                {
                    SensorName = "TestSensor",
                    Value = 22.0,
                    DateTime = DateTime.Now
                });
            }

            // Add anomalous reading
            var anomalousReading = new Reading
            {
                SensorName = "TestSensor",
                Value = 30.0, // Large spike
                DateTime = DateTime.Now
            };

            // Act
            bool isAnomaly = sensor.DetectAnomaly(anomalousReading);

            // Assert
            Assert.True(isAnomaly);
        }

        [Fact]
        public void CheckThreshold_WithinThresholds_ReturnsFalse()
        {
            // Arrange
            var sensor = new TemperatureSensor();
            sensor.InitializeSensor("TestSensor", "Room A", 20.0, 25.0);
            var reading = new Reading
            {
                SensorName = "TestSensor",
                Value = 22.5,
                DateTime = DateTime.Now
            };

            // Act
            bool exceeded = sensor.CheckThreshold(reading, 20.0, 25.0);

            // Assert
            Assert.False(exceeded);
        }

        [Fact]
        public void CheckThreshold_ExceedsMax_ReturnsTrue()
        {
            // Arrange
            var sensor = new TemperatureSensor();
            sensor.InitializeSensor("TestSensor", "Room A", 20.0, 25.0);
            var reading = new Reading
            {
                SensorName = "TestSensor",
                Value = 26.0,
                DateTime = DateTime.Now
            };

            // Act
            bool exceeded = sensor.CheckThreshold(reading, 20.0, 25.0);

            // Assert
            Assert.True(exceeded);
        }

        [Fact]
        public void InjectFault_CausesFaultCondition()
        {
            // Arrange
            var sensor = new TemperatureSensor();
            sensor.InitializeSensor("TestSensor", "Room A", 22.0, 24.0);
            sensor.StartSensor();

            // Act
            sensor.InjectFault();
            double temp1 = sensor.SimulateData();
            System.Threading.Thread.Sleep(10);
            double temp2 = sensor.SimulateData();

            // Assert - temperature should be rising or high
            Assert.True(temp2 >= temp1 || temp1 > 25.0);
        }

        [Fact]
        public void ShutdownSensor_ClearsHistory()
        {
            // Arrange
            var sensor = new TemperatureSensor();
            sensor.InitializeSensor("TestSensor", "Room A", 20.0, 25.0);
            sensor.StoreData(new Reading
            {
                SensorName = "TestSensor",
                Value = 22.0,
                DateTime = DateTime.Now
            });

            // Act
            sensor.ShutdownSensor();

            // Assert
            Assert.Equal(0, sensor.GetHistoryCount());
        }

        [Fact]
        public void GetHistory_ReturnsReadOnlyCollection()
        {
            // Arrange
            var sensor = new TemperatureSensor();
            sensor.InitializeSensor("TestSensor", "Room A", 20.0, 25.0);
            sensor.StoreData(new Reading
            {
                SensorName = "TestSensor",
                Value = 22.0,
                DateTime = DateTime.Now
            });

            // Act
            var history = sensor.GetHistory();

            // Assert
            Assert.NotNull(history);
            Assert.Single(history);
        }
    }
}
