// #include <FastGPIO.h>

#define lickport_pin 7
int lickport_state = LOW;
long lc = 0; // lick count
#define reward_len 100
long reward_timer = 0;
int r = 0 ;
//
int rflag = 0;
long start = 0;



// #define c_EncoderPinA 2
// #define c_EncoderPinB 11

// volatile bool _EncoderBSet;
// volatile long _EncoderTicks = 0;


#define solenoid_pin 6
int solenoid_pin_state = LOW;
int solenoid_timer = 0;

#define ttl_0_pin 4 // frame times
#define ttl_1_pin 12 // trigger microscope
int ttl_1_state = LOW;

int cmd = 0;
int incomingByte = 0;  // cmd coming in from Unity
int scan_flag = 0 ;




void setup() {
  // put your setup code here, to run once:
  Serial.begin(57600);

  
//   FastGPIO::Pin<c_EncoderPinB>::setInputPulledUp();
//   attachInterrupt(digitalPinToInterrupt(c_EncoderPinA), HandleMotorInterruptA, RISING);

  pinMode (lickport_pin,INPUT);
  digitalWrite(lickport_pin,LOW);
  
  pinMode (solenoid_pin,OUTPUT);
  digitalWrite(solenoid_pin,LOW);
  
  pinMode (ttl_0_pin,OUTPUT);
  digitalWrite(ttl_0_pin,LOW);
  
  pinMode (ttl_1_pin,OUTPUT);
  digitalWrite(ttl_1_pin,LOW);
  
 
  
  
}

void loop() {


  // read lickport
  lickport_state = digitalRead(lickport_pin);
  lc += lickport_state;


  // make sure ttl_0 is low
  digitalWrite(ttl_0_pin,LOW); // make sure ttl0 is down

  // check state of reward pin
  if ((solenoid_pin_state == HIGH)){
     if (millis()-reward_timer>reward_len) {
      digitalWrite(solenoid_pin, LOW);
      solenoid_pin_state = LOW;
     }   
  }

  
  switch(cmd) {
    
    
    case 0: // just read rotary and licks
      break;

    case 2: // reset reward flag
      rflag = 0;
      break;

    case 3: // reward lickport licks that are more than 3 sec apart
      if (lc>0 & rflag==0) {
        rflag = 1;
        r=1;
        solenoid_pin_state= HIGH;
        digitalWrite(solenoid_pin,HIGH);
        reward_timer = millis();
        start = millis();
      }

      if (millis()-start>3000) {
        rflag = 0;
      }
      break;

    case 4: // deliver reward
      if (solenoid_pin_state==LOW) { // reward not yet dispensed
        solenoid_pin_state = HIGH;
        digitalWrite(solenoid_pin,HIGH);
        reward_timer = millis();
        r=1;
      }
      break;

    case 5: // cleaning/loading lickports/open solenoid without timer
      digitalWrite(solenoid_pin,HIGH);
      break;

    case 6: // close solenoid
      digitalWrite(solenoid_pin,LOW);

    case 8: // toggle microscope on or off

      //if (ttl_1_state==LOW) {
      digitalWrite(ttl_1_pin, HIGH);
        //ttl_1_state==HIGH;
      //} else {
        //digitalWrite(ttl_1_pin,LOW);
        //ttl_1_state=LOW;  
      //}
      
      
      scan_flag = 0;
      break;

    case 9: // start collecting ttl0's for syncing
      scan_flag=1;

    case 12: // reward if there's a lick
      if (rflag == 0)
      { // if reward not dispensed
        if (lc > 0)  { // if lick 
          rflag = 1;
          solenoid_pin_state = HIGH;
          digitalWrite(solenoid_pin,HIGH);
          reward_timer = millis();
          r=1; // reward
        }
      }    
      break;

    case 13: // toggle microscope off
      digitalWrite(ttl_1_pin, LOW);
    
  }

  // print to serial port
  if (Serial.available()>0) { // if new Unity frame
   
   
    cmd = Serial.parseInt(); 
  
    if (scan_flag>0) { // if scanning
      digitalWrite(ttl_0_pin,HIGH); // send frame syncing ttl to scanbox
    }
    
    Serial.print(lc);
    Serial.print("\t");
    Serial.print(r);
  //    Serial.print("\t");
  //     Serial.print(_EncoderTicks);     
    Serial.println("");
    lc = 0; // reset lick count
    r=0; // reset reward count
  //     _EncoderTicks=0; // reset rotary encoder

  }
  

}

// void HandleMotorInterruptA()
// {
//    // read B pin
//    _EncoderBSet = FastGPIO::Pin<c_EncoderPinB>::isInputHigh();
//   // and adjust counter + if A leads B 
//    _EncoderTicks += _EncoderBSet ? -1 : +1;
  
// }
