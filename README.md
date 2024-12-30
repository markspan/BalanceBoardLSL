# BalanceBoardLSL

Get data from an adapted Wii Balanceboard or rotary dial and stream it to labstreaminglayer.


# HIDlsl: Description 

## Overview

The HIDlsl application is designed for interfacing adapted balance boards and rotary force feedback devices with a PC. It utilizes **SharpDX.DirectInput** for hardware communication and **Lab Streaming Layer (LSL)** for data streaming to other software (e.g., MATLAB).

### Key Features
- Detects and interfaces with adapted devices such as balance boards and rotary encoders.
- Streams data to external software in real-time using LSL.
- Configurable for multiple device types.

## Getting Started

### System Requirements
- Windows operating system
- Supported .NET Framework
- [Lab Streaming Layer](https://github.com/labstreaminglayer/liblsl) library
- [SharpDX](https://github.com/sharpdx/SharpDX) library

### Installation
1. Clone or download the repository containing the application code.
2. Install the required dependencies using NuGet or your preferred package manager.

### How to Run
1. Launch the application.
2. Connect your device (e.g., Adapted Balance Board or Rotary ForceFeed).
3. Select the device from the drop-down menu.
4. Click the "Link" button to start data streaming.

## Application Details

### Main Components

#### 1. Device Detection
The application uses `SharpDX.DirectInput` to detect compatible devices:
- **Adapted Balance Board**: Identified under `DeviceType.Joystick` or `DeviceType.Supplemental`.
- **Rotary ForceFeed**: Identified based on product name containing "Micro".

#### 2. Data Streaming
The application initializes an LSL stream with metadata describing the device and data channels. Sample data is collected from the device and streamed to external software.

##### Example MATLAB Plot
To correctly read XDF data in MATLAB, use the following:
```matlab
load_xdf('your_data.xdf');
plot(double(bitshift(uint32(data'), -8)) / 2.7); % Convert to kg (approx.)
```

### Device-Specific Behavior

#### Adapted Balance Board
- Streams 5 channels: bottom-left, top-left, top-right, bottom-right weights (in kilograms), and a sawtooth calibration signal.

#### Rotary ForceFeed
- Streams 2 channels: rotary position and sawtooth calibration signal.

### LSL Metadata
The LSL stream metadata includes:
- Device author and manufacturer information.
- Channel labels, types, and units.

## Development Notes

- Ensure device drivers are installed for successful detection.
- The application relies on buffered data acquisition for accurate sampling.

## Troubleshooting

### Common Issues
- **Device Not Found**: Ensure the device is connected and recognized by the system.
- **Link Button Disabled**: Indicates no compatible device was detected.

## Contributing
Pull requests and contributions are welcome. Please follow the coding standards and include documentation for new features.

## Author
M.M. Span  
University of Groningen  
[markspan.github.io](https://markspan.github.io)


