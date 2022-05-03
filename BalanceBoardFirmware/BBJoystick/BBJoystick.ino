/*
Firmware to attach a wii balance board via usb to a pc. Arduino micro pro code.
1st version: m.m.span 25-11-2021

Soldered four wires to the BB PCB: outputs of the op2333 operational amplifiers (U1 and U2) to A0-A4

Problem solved: the BB will shut down after one second if not connected through BT.....


*/
//#define DEBUG
#include "Joystick.h"
// Create Joystick with four axis (X,Y,Z and Throttle)
Joystick_ Joystick(JOYSTICK_DEFAULT_REPORT_ID, 
  JOYSTICK_TYPE_JOYSTICK, 0, 0,
  true, true, true, true, true, false,
  false, false, false, false, false);
  
// Maximum and Minimum values for the axes.
#define HIVAL 1000
#define LOVAL -24

int ramp = 0;
 
int sensorPin0 = A0;    // select the input pin for the BB sensor
int sensorPin1 = A1;    // select the input pin for the BB sensor
int sensorPin2 = A2;    // select the input pin for the BB sensor
int sensorPin3 = A3;    // select the input pin for the BB sensor

int RB = 0;  // variable to store the value coming from the sensor
int RO = 0;  // variable to store the value coming from the sensor
int LB = 0;  // variable to store the value coming from the sensor
int LO = 0;  // variable to store the value coming from the sensor

int cRB = 0;  // variable to store the value coming from the sensor
int cRO = 0;  // variable to store the value coming from the sensor
int cLB = 0;  // variable to store the value coming from the sensor
int cLO = 0;  // variable to store the value coming from the sensor

unsigned long t0;

void setup() {
  // declare the ledPin as an OUTPUT:
  pinMode(A0, INPUT_PULLUP);
  pinMode(A1, INPUT_PULLUP);
  pinMode(A2, INPUT_PULLUP);
  pinMode(A3, INPUT_PULLUP);
  
  cLO=cLB=cRB=cRO=0;
  
  // Find zero points on start.
  for (int i = 1; i<101; i++)
  {
    cLO = cLO+analogRead(sensorPin0);
    cLB = cLB+analogRead(sensorPin1);
    cRB = cRB+analogRead(sensorPin2);
    cRO = cRO+analogRead(sensorPin3);
    delay(1);
  }
  cLO=cLB=cRB=cRO=0;
  for (int i = 1; i<101; i++)
  {
    cLO = cLO+analogRead(sensorPin0);
    cLB = cLB+analogRead(sensorPin1);
    cRB = cRB+analogRead(sensorPin2);
    cRO = cRO+analogRead(sensorPin3);
    delay(5);
  }
  
  cLO=cLO/100;
  cLB=cLB/100;
  cRB=cRB/100;
  cRO=cRO/100;
  
  Joystick.setXAxisRange(LOVAL, HIVAL);
  Joystick.setYAxisRange(LOVAL, HIVAL);
  Joystick.setZAxisRange(LOVAL, HIVAL);
  Joystick.setRxAxisRange(LOVAL, HIVAL);
  Joystick.setRyAxisRange(0,100);
  
  Joystick.begin(false);
  t0=micros();
}

void loop() {
 
  // read the value from the sensor ten times, substracting zero points:
  LO=LB=RB=RO=0;
  for (int i = 1; i<11; i++)
  {
    LO = LO+(analogRead(sensorPin0)-cLO);
    LB = LB+(analogRead(sensorPin1)-cLB);
    RB = RB+(analogRead(sensorPin2)-cRB);
    RO = RO+(analogRead(sensorPin3)-cRO);
    //delay(5);
  }
  // And write them
  Joystick.setXAxis(LO);
  Joystick.setYAxis(LB);
  Joystick.setZAxis(RB);
  Joystick.setRxAxis(RO);
  Joystick.setRyAxis(++ramp);
  
  if (ramp >= 100)
    ramp = 0;
    
  // Wait until aproximately 10 ms has passed....
  while(micros()-t0 < 9950);
  t0=micros();
  // Write the data in one go
  Joystick.sendState(); 
  }
