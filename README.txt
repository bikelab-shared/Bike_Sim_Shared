# Eternity Bike Simulator

A Unity-based VR cycling simulator integrating a physical bike setup, Tacx trainer, Arduino sensors, and a motion platform for immersive feedback and motion cueing.

## 🛠️ Features

- **VR Integration** via Oculus Quest 2 using OVRCameraRig
- **Physical Bike Integration** with handlebar and brakes
- **Tacx Flux 2 Trainer** for speed input and resistance
- **Arduino Communication** over BLE for brake and resistance data
- **Motion Platform** (ForceSeatMI) tilt and pitch feedback
- **Platform Calculation Models** (e.g., Realism, No Tilt)
- **Visual Tilting** of camera for motion cueing

## 🎮 Controls

- `T` – Cycle Platform Tilt Models
- `Z` – Toggle Data Logging
- `R` – Reset Scene

## 🧩 Structure Overview

- `GameControllerScript.cs` – Core control logic for bike, sensors, and motion
- `BikeControllerScript.cs` – Handles steering and visual tilt
- `HandleBarColllider.cs` – Handles performance measurment
- `Uduino` – BLE-based communication with Arduino
- `ForceSeatMI` – SDK integration for motion platform

## 🧪 Debug Options

Set `activateCalculationLogging = true` in `GameControllerScript` to get live debug info on tilt, pitch, and speed.

## 🔧 Setup Notes

- [1] Link between Oculus and PC is working
- [2] Make sure the Arduino is flashed and available via BLE as `IndoorBikeData`
- [3] ForceSeatPM must be installed and the platform connected via ForceSeatMI
- [4] Press Play in configured Unity-Game

## 📂 Assets to Review

- Prefabs: `EternityBike`
- Scenes: `BikeSimulator`
