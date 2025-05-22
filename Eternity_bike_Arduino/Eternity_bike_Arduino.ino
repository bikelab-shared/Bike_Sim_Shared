/*
  This version uses Tacx Flux-2 36688. It identifies the different Tacx Flux-2
  - Tacx Flux-2 36688
  - Tacx Flux-2 18201
  
  The circuit:
  - Arduino Uno WiFi Rev2 board
*/
#include<Uduino.h>
#include <ArduinoBLE.h>

Uduino uduino("IndoorBikeData"); // Declare and name your object

uint8_t SpeedLSB = 0x00;
uint8_t SpeedMSB = 0x00;
uint16_t Speed = 0;
float SpeedO;

int speedWindGenerator = 0; // für Wind generator

float ResistanceAngle = 0.0;
float steeringAngle = 0.0;

int FCPinit = 0;
int ResistanceChange = 0; // Ob es eine Änderung zum vorherigen Status gegeben hat
int Resistance = 0;


int FrontBrakeForce = 0;
int RearBrakeForce = 0;
int CombinedBrakeForce = 0;

unsigned long lastMillisIDB;
unsigned long lastMillisSteer;
unsigned long lastMillisResistance;

void setup() {
  Serial.begin(115200);
  while (!Serial);
  lastMillisIDB = millis();

  // begin initialization
  if (!BLE.begin()) {
    uduino.println("starting BLE failed!");

    while (1);
  }

  uduino.println("BLE Central - Indoor Bike Data");
  uduino.println("Make sure to turn on the device.");

  // start scanning for peripheral
  BLE.scan();

  pinMode(12, OUTPUT); //Initiates Motor Channel A pin
  pinMode(9, OUTPUT); //Initiates Brake Channel A pin
}
void loop() {

  uduino.update();
  if (uduino.isConnected() || 1 == 1) {

    // check if a peripheral has been discovered
    BLEDevice peripheral = BLE.available();

    if (peripheral) {
      // discovered a peripheral, print out address, local name, and advertised service
      uduino.print("Found ");
      uduino.print(peripheral.address());
      uduino.print(" '");
      uduino.print(peripheral.localName());
      uduino.print("' ");
      uduino.print(peripheral.advertisedServiceUuid());
      uduino.println();

      // Lab-Bike: "Tacx Flux-2 36688"
      // Forschungsfest-Bike: "Tacx Flux-2 18201"

      // Check if the peripheral is a Tacx Flux-2, the local name will be:
      if (peripheral.localName() == "Tacx Flux-2 36688") {
        // stop scanning

        BLE.stopScan();
        uduino.print("Tacx found");
        monitorIndoorBikeData(peripheral);

        // peripheral disconnected, start scanning again
        BLE.scan();
      }
    }
  }
}

void monitorIndoorBikeData(BLEDevice peripheral) {
  // connect to the peripheral
  uduino.println("Connecting ...");
  if (peripheral.connect()) {
    uduino.println("Connected");
  } else {
    uduino.println("Failed to connect!");
    return;
  }

  // discover peripheral attributes FTMS
  uduino.println("Discovering service 0x1826 ...");
  if (peripheral.discoverService("1826")) {
    uduino.println("Service discovered 1826");
  } else {
    uduino.println("Attribute discovery failed.");
    peripheral.disconnect();

    while (1);
    return;
  }

  // retrieve the simple key characteristic 2ad2 Indoor Bike Data
  BLECharacteristic indoorBikeDataCharacteristic = peripheral.characteristic("2ad2");

  // subscribe to the simple key characteristic
  uduino.println("Subscribing to simple key characteristic 2ad2 Indoor Bike Data...");
  if (!indoorBikeDataCharacteristic) {
    uduino.println("no simple key characteristic 2ad2 Indoor Bike Data found!");
    peripheral.disconnect();
    return;
  } else if (!indoorBikeDataCharacteristic.canSubscribe()) {
    uduino.println("simple key characteristic 2ad2 Indoor Bike Data is not subscribable!");
    peripheral.disconnect();
    return;
  } else if (!indoorBikeDataCharacteristic.subscribe()) {
    uduino.println("subscription 2ad2 Indoor Bike Data failed!");
    peripheral.disconnect();
    return;
  } else {
    uduino.println("Subscribed");
    uduino.println("Empfange Daten von Characterisitic: 2ad2 Indoor Bike Data");
  }

  // retrieve the FitnessMachineControlPointCharacteristic characteristic
  BLECharacteristic FitnessMachineControlPointCharacteristic = peripheral.characteristic("2ad9");

  // subscribe to the simple key characteristic
  uduino.println("Subscribing to simple key characteristic 2ad9 FitnessMachineControlPointCharacteristic...");
  if (!indoorBikeDataCharacteristic) {
    uduino.println("no simple key characteristic 2ad9 FitnessMachineControlPointCharacteristic found!");
    peripheral.disconnect();
    return;
  } else if (!indoorBikeDataCharacteristic.canSubscribe()) {
    uduino.println("simple key characteristic 2ad9 FitnessMachineControlPointCharacteristic is not subscribable!");
    peripheral.disconnect();
    return;
  } else if (!indoorBikeDataCharacteristic.subscribe()) {
    uduino.println("subscription 2ad9 FitnessMachineControlPointCharacteristic failed!");
    peripheral.disconnect();
    return;
  } else {
    uduino.println("Subscribed");
    uduino.println("Empfange Daten von Characterisitic: 2ad9 FitnessMachineControlPointCharacteristic");
  }


  if (!FitnessMachineControlPointCharacteristic) {
    uduino.println("Peripheral does not have Fitness Machine Control Point Characteristic!");
    peripheral.disconnect();
    return;
  } else if (!FitnessMachineControlPointCharacteristic.canWrite()) {
    uduino.println("Peripheral does not have a writable Fitness Machine Control Point Characteristic!");
    peripheral.disconnect();
    return;
  }

  while (peripheral.connected()) {
    // while the peripheral is connected

    if ((millis() - lastMillisSteer) >= 25) {
      lastMillisSteer = millis();


      uduino.print(SpeedO, 2);
      uduino.print(",");

      //Steering Angle
      int steeringAngle = map(analogRead(A2), 0 , 1023, 90, -90);
      int processedSteeringAngle = 180.00 / 134.00 * steeringAngle;
      uduino.print(processedSteeringAngle);
      uduino.print(",");

      //Brakes
      uduino.print(FrontBrakeForce);
      uduino.print(",");
      uduino.print(RearBrakeForce);
      uduino.print(",");
      uduino.print(CombinedBrakeForce);
      uduino.print(",");

      //Resistance
      uduino.println(Resistance);
      //uduino.println("0");

    }
    if ((millis() - lastMillisIDB) >= 95) {
      lastMillisIDB = millis();
      // yes, get the value, characteristic is 1 byte so use byte value
      byte value = 0;
      int descriptorValueSize = indoorBikeDataCharacteristic.valueSize();
      byte descriptorValue[descriptorValueSize];

      for (int i = 0; i < descriptorValueSize; i++) {
        descriptorValue[i] = indoorBikeDataCharacteristic.value()[i];

        if (i == 2) {
          SpeedLSB = indoorBikeDataCharacteristic.value()[i];
        }
        if (i == 3) {
          SpeedMSB = indoorBikeDataCharacteristic.value()[i];
          Speed = SpeedMSB;
          Speed <<= 8;
          Speed = Speed | SpeedLSB;
          SpeedO = Speed * 0.01;

          speedWindGenerator = map(SpeedO, 0 , 20, 0, 255);

          digitalWrite(12, HIGH); //Establishes forward direction of Channel A
          digitalWrite(9, HIGH);   //Disengage the Brake for Channel A
          if (speedWindGenerator >= 100) {
            //analogWrite(3, speedWindGenerator);   //Spins the motor on Channel A at full speed
          } else {
            digitalWrite(9, LOW);
          }
        }


        FrontBrakeForce = map(analogRead(A3), 0 , 1023, 0, 100);  //Force of Front Brake
        RearBrakeForce = map(analogRead(A4), 0 , 1023, 0, 100);  //Force of Rear Brake
        CombinedBrakeForce = FrontBrakeForce + RearBrakeForce;
        //TODO FIX
        //Hier mit combinedBrakeForce die Resistance beeinflussen..., maximal werte??? 
        //*2 weil hintere BrakeSensor kaputt ist
        //CombinedBrakeForce = CombinedBrakeForce * 2;
      }
    }

    if ((millis() - lastMillisResistance) >= 415) {
      lastMillisResistance = millis();

      if (FCPinit  == 0) {       // beim ersten Aufruf wird der Initalprozess ausgeführt

        int i = 0;
        uint8_t FMCPCreset = 0x00;  // um FMCP zu verwenden wird es mit dem Hexwert 00 zurückgesetzt
        byte FMCPCstart = 0x07;     // um FMCP zu starten, wird der wert 07 übermittelt

        uduino.print("FMCPCreset: ");
        uduino.println(FMCPCreset, HEX);
        uduino.print("FMCPCstart: ");
        uduino.println(FMCPCstart, HEX);
        uduino.print("init = 0 reset");
        uduino.println(FMCPCreset);

        uint8_t value[2];
        FitnessMachineControlPointCharacteristic.readValue(value, 2);   //value wird der Statuswert von FMCP zugewiesen
        uduino.print("Init Byte 1: ");
        uduino.print(value[0], HEX);
        uduino.print(" Init Byte 2: ");
        uduino.println(value[1], HEX);

        FitnessMachineControlPointCharacteristic.writeValue(FMCPCreset, 2);
        
        if (FitnessMachineControlPointCharacteristic.valueUpdated()) {
          uduino.println("Request Control");
          uint8_t value[3];
          FitnessMachineControlPointCharacteristic.readValue(value, 3);
          uduino.print("Byte 1: ");
          uduino.println(value[0], HEX);
          uduino.print("Byte 2: ");
          uduino.println(value[1], HEX);
          uduino.print("Byte 3: ");
          uduino.println(value[2], HEX);
        } else {
          uduino.println("no Request Control");
          uint8_t value[3];
          FitnessMachineControlPointCharacteristic.readValue(value, 3);
          uduino.print("Byte 1: ");
          uduino.println(value[0], HEX);
          uduino.print("Byte 2: ");
          uduino.println(value[1], HEX);
          uduino.print("Byte 3: ");
          uduino.println(value[2], HEX);
        }

        FitnessMachineControlPointCharacteristic.writeValue(FMCPCstart, 1);
        if (FitnessMachineControlPointCharacteristic.valueUpdated()) {
          uduino.println("Start Training");
          byte value = 0;
          FitnessMachineControlPointCharacteristic.readValue(value);
          uduino.print(" FMCPCstart Value : ");
          uduino.println(value, HEX);
        }

        FCPinit = 1;
        uduino.println("Init 1");
      }

      Resistance = map(CombinedBrakeForce, 0 , 200, 0, 10);  //angle für den Widerstand
      
      uint8_t OpCode = 0x04;
      int ResistanceSupport = Resistance * 40;
      int Divisor = 256;
      uint8_t MSB = 0;
      uint8_t LSB = 0;
      
      MSB = ResistanceSupport / Divisor;
      LSB = ResistanceSupport - (MSB * 256);

      uint8_t PotResistance[3] = {OpCode, LSB, MSB};

      FitnessMachineControlPointCharacteristic.writeValue(PotResistance, 3); // 04E8030 100%
      /*if (ResistanceSupport != ResistanceChange) {
        FitnessMachineControlPointCharacteristic.writeValue(PotResistance, 3); // 04E8030 100%
        ResistanceChange = ResistanceSupport;
      }*/
    }
  }
  uduino.println("SensorTag disconnected!");
}
