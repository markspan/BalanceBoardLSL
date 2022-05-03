#include <Joystick.h>

// Create the Joystick
// Joystick(Joystick HID_ID, Joystick Type, Button Count, HAT Switch Count, Include X, Include Y, Include Z,
// Include Rx, Include Ry, Include Rz, Include Rudder, Include Throttle. Include accelerometer, Include Brake, Include Steering
Joystick_ Joystick(JOYSTICK_DEFAULT_REPORT_ID, JOYSTICK_TYPE_JOYSTICK, 0, 0, true, false, false, false, true, false, false, false, false, false, false);

int mvm = 5; // micro vibration motor is connected with pin number 5 which is the pwm pin. 
int vresistor = A1; 
int data = 0;
unsigned long t0;
int ramp = 0;

void setup() {
  pinMode(mvm, OUTPUT);
  pinMode(vresistor, INPUT);  
  
  // Set Range Values
  Joystick.setXAxisRange(0, 153);
  Joystick.setRyAxisRange(0,100);
  Joystick.begin(false);
  t0=micros();
}
 
void loop() {
// put your main code here, to run repeatedly:
// Het trilmotortje heeft een voedingsbereik van 0 ... 3.0V.
// De OUTPUT van de Arduino Micro is 5V. Dit is te veel voor het trilmotortje. 
// De oplossing is om de duty cycle aan te passen. Range: 0 ... 255 => duty cycle: 100%
// 3V => 3/5 = 60%
// 255 * 0.6 = 153 stapjes om op de uitgang 3.0V te krijgen.
//
// Een ander probleem is de stroomsterkte die een poort van de Micro kan leveren. Tijdens het aanzetten van de trilmotor is er een piek in de stroomsterkte. 
// Deze is te groot voor de uitgang van de Micro. De oplossing is het gebruik van een FET. De uitgang van de Micro is aangesloten op de Gain (ingang) van de FET.
// De uitgang van de FET volgt het uitgangssignaal van de Micro. De FET wordt rechtstreeks gevoed met de 5V aansluiting van de Micro.
// Deze voeding kan wel genoeg leveren voor het trilmotoryje.

  data = map(analogRead(vresistor), 0, 1023, 0, 153);
  
  Joystick.setXAxis(data);  
  Joystick.setRyAxis(++ramp);
  if (ramp >= 100)
    ramp = 0;

  while(micros()-t0 < 9950);
  t0=micros();
  // Write the data in one go
  Joystick.sendState(); 
  analogWrite(mvm, data); 
}
