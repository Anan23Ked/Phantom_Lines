# Phantom Lines — Technical Setup Guide

An AR-to-physical data visualisation device: a phone-tracked handheld stick with a 144-LED strip that lights up when it "intersects" virtual architectural surfaces in a Unity AR scene. Built for CASA0019 (Sensor Data Visualisation) by Ananya Kedlaya and Matilda Nelson.

> For the full design process, concept development, and build documentation, see the project write-up below this section (or the original README content). This section covers what you need to actually set up and run the system.

## System Architecture

```
┌─────────────────────┐         MQTT           ┌──────────────────────┐
│   Unity (AR app)    │ ─────────────────────▶ │  ESP8266 Feather     │
│   Phone-mounted     │  publish: LED index    │  Huzzah + NeoPixel   │
│  AR Foundation scene│  topic:                │  strip (144 LEDs)    │
│                     │  student/ucfnake/      │                      │
│  - Tracks stick pose│  PhantomLines          │  - Subscribes to     │
│    via phone sensors│                        │    LED index topic   │
│  - Raycasts from    │                        │  - Lights matching   │
│    Point A to       │                        │    LED, clears prev  │
│    Environment layer│                        │                      │
│  - Converts hit     │                        │                      │
│    distance → LED   │                        │                      │
│    index            │                        │                      │
└─────────────────────┘                        └──────────────────────┘
        │
        ▼
  mqtt.cetools.org:1884 (broker)
```

Both sides connect to a shared MQTT broker (`mqtt.cetools.org`, port 1884). Unity publishes the calculated LED index whenever the virtual stick intersects an AR surface; the ESP8266 firmware subscribes to that topic and drives the physical LED strip accordingly.

## Repo Structure

```
├── MQTT/
│   └── LEDStick.ino              # ESP8266 firmware: WiFi + MQTT + NeoPixel control
│
├── Unity/
│   ├── Assets/
│   │   ├── STICK/                # 3D model of the physical stick (.fbx) + prefab
│   │   ├── Resources/
│   │   │   ├── Models/           # AR scene 3D models (e.g. accessibility toilet scene)
│   │   │   └── Scripts/          # Core C# logic (see below)
│   │   ├── Prefab/
│   │   └── Video/
│   ├── Packages/                 # Unity package manifest
│   └── ProjectSettings/          # Unity project configuration
│
├── index.html                    # (supporting web asset)
├── LICENSE.txt                   # MIT
└── README.md
```

### Key Unity scripts (`Assets/Resources/Scripts/`)

| Script | Role |
|---|---|
| `StickIntersection.cs` | Raycasts from the virtual stick's Point A into the `Environment` layer, calculates hit distance, converts it to an LED index |
| `EnvironmentIdentifier.cs` | Tags detected AR planes/objects as part of the `Environment` layer used for intersection detection |
| `mqttManager.cs` | Manages the MQTT client connection (connect/reconnect) from within Unity |
| `mqttPublish.cs` | Publishes the calculated LED index to the MQTT broker |
| `tapToPlace.cs` | AR Foundation tap-to-place logic for anchoring the 3D model in the physical space |
| `uiAR.cs` | AR scene UI interactions |
| `DistanceDashboard.cs` | Displays live distance/measurement data on screen |

*(Descriptions above are inferred from the project write-up and file/function names — verify against the actual script contents.)*

## Prerequisites

**Unity side:**
- Unity Editor (check `ProjectSettings/ProjectVersion.txt` for the exact version used)
- AR Foundation package (and the relevant AR provider — ARKit for iOS / ARCore for Android) — check `Packages/manifest.json` for exact package versions
- A physical iOS/Android device that supports AR Foundation (AR doesn't run in the standard editor Play mode)

**Firmware side:**
- Arduino IDE with ESP8266 board support installed
- Libraries: `ESP8266WiFi`, `PubSubClient`, `Adafruit_NeoPixel`
- Adafruit Feather Huzzah ESP8266
- WS2812B/NeoPixel LED strip (144 LEDs), 5V power supply sized for your brightness setting
- Access credentials for an MQTT broker (the original project used `mqtt.cetools.org:1884` — UCL CASA's teaching broker; you'll need your own broker or credentials if reproducing this)

## Setup

### 1. Unity AR app

1. Open the `Unity/` folder as a project in Unity Hub (matching the Editor version in `ProjectSettings/ProjectVersion.txt`).
2. Let Unity resolve packages from `Packages/manifest.json`.
3. In `mqttManager.cs` / `mqttPublish.cs`, set the broker address, port, and topic to match your own MQTT broker (see firmware section below — both sides must use the same topic).
4. Build and deploy to a physical AR-capable device (File → Build Settings → iOS/Android).
5. Physically attach the phone to the top of the stick assembly (Point A in the tracking scheme).

### 2. ESP8266 firmware (`MQTT/LEDStick.ino`)

The sketch expects a separate `arduino_secrets.h` file (not committed to the repo, for credential safety) defining:

```cpp
#define SECRET_SSID     "your-wifi-ssid"
#define SECRET_PASS     "your-wifi-password"
#define SECRET_MQTTUSER "your-mqtt-username"
#define SECRET_MQTTPASS "your-mqtt-password"
```

Create this file in the same directory as `LEDStick.ino` before compiling.

Key config values to check/change at the top of `LEDStick.ino`:

| Constant | Purpose | Default |
|---|---|---|
| `LED_PIN` | GPIO pin wired to the LED strip data line | `14` |
| `LED_COUNT` | Number of LEDs in the strip | `144` |
| `BRIGHTNESS` | Strip brightness, 0–255 (start low to avoid overloading power supply) | `50` |
| `mqtt_server` / `mqtt_port` | MQTT broker address | `mqtt.cetools.org` / `1884` |
| `MQTT_CLIENT_ID` | Unique client identifier on the broker | `student/ce/mvn/phantomlines` |
| `MQTT_TOPIC_SUB` | Topic the ESP8266 listens on for LED commands | `student/ucfnake/PhantomLines` |
| `MQTT_TOPIC_STATE` | Topic the ESP8266 publishes its online status to | `phantomlines/device/state` |

> If you use your own broker, `MQTT_TOPIC_SUB` here must exactly match the topic `mqttPublish.cs` in Unity publishes to.

Flash steps:
1. Open `LEDStick.ino` in Arduino IDE.
2. Select the correct board (Adafruit Feather Huzzah ESP8266) and port.
3. Add `arduino_secrets.h` as described above.
4. Upload.
5. Open Serial Monitor at 115200 baud to confirm WiFi + MQTT connection.

### 3. Message protocol

The firmware expects plain-text MQTT payloads on `MQTT_TOPIC_SUB`:
- An integer `0`–`143` → lights that LED index (white, `255,255,255`) and turns off the previously lit LED
- The string `"CLEAR"` → turns off all LEDs

## Hardware Notes

- Power: 4x AA batteries in series for the LED strip, a separate LiPo battery for the Feather Huzzah — both share a common ground.
- The physical stick housing is a debarked, hand-chiselled wooden branch with a channel cut for the LED strip, chosen deliberately to contrast the device's electronic/virtual function with a tactile natural material.
- Known limitation: the MQTT publisher/subscriber connection isn't always persistent, and AR plane recognition for the `Environment` layer can be inconsistent frame-to-frame — see "Technical Areas of Improvement" in the project write-up for details.

## License

MIT — see `LICENSE.txt`.