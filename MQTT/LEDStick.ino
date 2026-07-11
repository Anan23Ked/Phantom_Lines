#include <ESP8266WiFi.h>        // ESP8266 WiFi library
#include <PubSubClient.h>       // Handles the MQTT protocol
#include <Adafruit_NeoPixel.h>  // Library for controlling WS2812B/NeoPixel LED strips
#include "arduino_secrets.h"    // External file for sensitive data (SSID, passwords)

// --- LED STRIP CONFIGURATION ---
#define LED_PIN 14             // GPIO pin connected to the LED strip data line (D6 = GPIO12)
#define LED_COUNT 144          // Total number of LEDs in your strip
#define BRIGHTNESS 50          // LED brightness (0-255), start lower to avoid power issues

// Create NeoPixel strip object
// NEO_GRB = LED strip uses GRB color order
// NEO_KHZ800 = 800 KHz bitstream (most common for WS2812 LEDs)
Adafruit_NeoPixel strip(LED_COUNT, LED_PIN, NEO_GRB + NEO_KHZ800);

// --- MQTT & WIFI CONFIGURATION ---
const char* ssid          = SECRET_SSID;
const char* password      = SECRET_PASS;
const char* mqtt_username = SECRET_MQTTUSER;
const char* mqtt_password = SECRET_MQTTPASS;

// Broker configuration - same as your breathing sensor
const char* mqtt_server   = "mqtt.cetools.org"; 
const int mqtt_port       = 1884;               

// Client ID and Topics
const char* MQTT_CLIENT_ID = "student/ce/mvn/phantomlines";      // Unique client ID
const char* MQTT_TOPIC_SUB = "student/ucfnake/PhantomLines";    // Topic to SUBSCRIBE to (receive messages)
const char* MQTT_TOPIC_STATE = "phantomlines/device/state";      // Topic to publish connection status

// Create WiFi and MQTT clients
WiFiClient wifiClient;
PubSubClient mqttClient(wifiClient);

// --- FUNCTION PROTOTYPES ---
void startWifi();
void reconnectMQTT();
void mqttCallback(char* topic, byte* payload, unsigned int length);
void setLED(int ledNumber, int red, int green, int blue);
void clearAllLEDs();

// Variables
int lastLED = -1;

// --- SETUP FUNCTION ---
void setup() {
  Serial.begin(115200);      // ESP8266 typically uses 115200 baud rate
  delay(1000);               // Give serial monitor time to open
  
  Serial.println("\n*** PHANTOMLINES LED CONTROLLER (ESP8266) ***");
  
  // 1. INITIALIZE LED STRIP
  strip.begin();              // Initialize the NeoPixel strip object
  strip.show();               // Turn OFF all LEDs initially (important!)
  strip.setBrightness(BRIGHTNESS); // Set overall brightness
  
  Serial.println("LED strip initialized (all OFF)");
  
  // 2. CONNECT TO WIFI
  startWifi();
  
  // 3. CONFIGURE MQTT CLIENT
  mqttClient.setServer(mqtt_server, mqtt_port);  // Set the MQTT broker address and port
  mqttClient.setCallback(mqttCallback);          // Set the function to handle incoming messages
  
  // 4. CONNECT TO MQTT BROKER
  reconnectMQTT();
  
  Serial.println("Setup complete. Waiting for MQTT messages...");
  Serial.println("--------------------------------------------");
}

// --- WIFI CONNECTION FUNCTION ---
void startWifi() {
  Serial.print("Connecting to WiFi: ");
  Serial.println(ssid);
  
  // Start WiFi connection
  WiFi.begin(ssid, password);
  
  // Wait for connection
  while (WiFi.status() != WL_CONNECTED) {
    delay(500);
    Serial.print(".");
         

  }
  
  Serial.println("\nWiFi connected!");
  Serial.print("IP address: ");
  Serial.println(WiFi.localIP());
}

// --- MQTT CONNECTION FUNCTION ---
void reconnectMQTT() {
  // Loop until we're reconnected
  while (!mqttClient.connected()) {
    Serial.print("Attempting MQTT connection...");
    
    // Attempt to connect with your credentials
    if (mqttClient.connect(MQTT_CLIENT_ID, mqtt_username, mqtt_password)) {
      Serial.println("connected!");
      
      // Publish online status
      mqttClient.publish(MQTT_TOPIC_STATE, "online");
      
      // Subscribe to the phantomlines topic to receive LED commands
      mqttClient.subscribe(MQTT_TOPIC_SUB);
      Serial.print("Subscribed to topic: ");
      Serial.println(MQTT_TOPIC_SUB);
      
    } else {
      Serial.print("failed, rc=");
      Serial.print(mqttClient.state());
      Serial.println(" retrying in 5 seconds...");
      delay(5000);
    }
  }
}


// --- MQTT CALLBACK FUNCTION (handles incoming messages) ---
void mqttCallback(char* topic, byte* payload, unsigned int length) {
  // Convert the payload (byte array) into a string
  String message = "";
  for (unsigned int i = 0; i < length; i++) {
    message += (char)payload[i];
  }

  // Command: Clear all LEDs
  if (message.equals("CLEAR")) {
    Serial.print("Clear");
    clearAllLEDs();
    lastLED = -1;             // Reset tracker
  }

  else {
    // Parse LED number
    int ledNum = message.toInt();

    // Validate
    if (ledNum >= 0 && ledNum < LED_COUNT) {

      // Turn OFF the previous LED if one was lit
      if (lastLED != -1 && lastLED != ledNum) {
        strip.setPixelColor(lastLED, strip.Color(0, 0, 0));
        
      }

      // Turn ON the new LED
      setLED(ledNum, 255, 255, 255);
      lastLED = ledNum;      // Remember it
    }
    else {
      Serial.println("Error: Invalid LED number or out of range");
    }
  }
}

// Set a specific LED to a color
void setLED(int ledNumber, int red, int green, int blue) {
  // Validate LED number is within range
  if (ledNumber >= 0 && ledNumber < LED_COUNT) {
    strip.setPixelColor(ledNumber, strip.Color(red, green, blue));
    strip.show(); // Update the strip to display the changes

  } else {
    Serial.print("Error: LED ");
    Serial.print(ledNumber);
    Serial.println(" is out of range!");
  }
}

// Turn off all LEDs
void clearAllLEDs() {
  strip.clear();  // Set all LEDs to off
  strip.show();   // Update the strip
  Serial.println("All LEDs cleared");
}

// --- MAIN LOOP ---
void loop() {
  // 1. Maintain WiFi connection
  if (WiFi.status() != WL_CONNECTED) {
    Serial.println("WiFi disconnected! Reconnecting...");
    startWifi();
  }
  
  // 2. Maintain MQTT connection
  if (!mqttClient.connected()) {
    Serial.println("MQTT disconnected! Reconnecting...");
    reconnectMQTT();
  }
  
  // 3. Process incoming MQTT messages
  // This MUST be called regularly to receive messages
  mqttClient.loop();
  
  // Small delay to prevent overwhelming the system
  delay(10);
}
