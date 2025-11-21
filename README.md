# Doccumentation #

## Overview #
this app simulates a virtual temperature sensor that monitors a data center room. it generates realistic temperature readings, validates data, detects anomalies and was supposed to store everything in a database but i couldnt get that to go.

## Features #
- Initialise sensors from YAML config files
- Generate realistic temperature readings with noise
- Validate data against acceptable ranges
- Log all activities with timestamps
- Store reading history
- Detect temperature anomalies
- Graceful shutdown

## Advanced Features ##
- Alert when threshholds are exceeded
- Simulate cooling system failures
- Security validation on inputs

## Setup Instructions ##
**Clone Repository**
- git clone <your-repo-url>
- cd temperature-sesnor-simulation

**Restore Packages**
- dotnet restore

**Built the project**
- dotnet build

**Run Tests**
dotnet test

**Run Application
- dotnet run

## Required NuGet Packages #
- YamlDotNet
- Microsoft.EntityFrameworkCore.sqlite
- Microsoft.EntityFrameworkCore
- xUnit

## Understanding Output #
**Normal Reading**
- [2024-11-21 10:30:45] DataCenter-Sensor-01 | 22.35°C | Status: VALID

**Smoothed Data (every 10 readings)**
- [SMOOTHED] Moving average (last 5): 22.94°C

**Anomaly Detected**
- [ANOMALY] Detected! Current: 26.50°C, Average: 22.80°C, Deviation: 3.70°C

**Threshold Alert**
- [ALERT] Threshold exceeded! Current: 26.20°C, Thresholds: [21.0, 25.0]

**Fault Simulation (every 50 readings)**
- [FAULT] Cooling failure injected - temperature will rise
[FAULT] Fault cleared - returning to normal operation

**How to stop app**
- ctrl C

## Function Guide #
**InitializeSensor(name, location, minValue, maxValue)**
- Sets up the sensor with config data
- Validates all inputs
- Throws exceptions for invalid data

**StartSensor()**
- Starts the sensor

**SimulateData()**
- Generates a temp reading with realistic noise
- Returns temps in degrees Celcius
- 5% chance of spike

**ValidateData(reading)**
- Checks if temp is within valid range
- returns true if valid, if not it returns false

**LogData(reading)**
- Logs reading to console and was supposed to log to database but couldnt get it to work

**StoreData(reading)**
- Saves reading to memory
- Saves last 100 readings in memory

**SmoothData()**
- Calculates moving average of last 5 readings
- returns smoothed temp

**DetectAnomaly(reading)**
- Checks for unusual temps
- Returns true if anomaly is detected
- Threshold of 1.5C deviation from recent average

**CheckThreshold(reading, min, max)**
- Checks if reading exceeds alert thresholds
- Returns true if exceeded

**InjectFault()**
- Simulates cooling system failure (temp rises)

**ClearFault()**
- Returns System to normal operation

**ShutdownSensor()**
- Stops sensor and clears memory

## Testing #
**Run all Tests**
- dotnet test

**Test Coverage**
The tests includes 20+ tests covering:

- Sensor Initialization (valid/invalid inputs)
- Data Simulation and validation
- History Storage
- Smoothing and calculations
- Anomaly detection
- Fault ijection
- Shutdown procedures
