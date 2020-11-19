const int lickport_pin = 7;
int lickport_state = LOW;
long lc = 0; // lick count
const int reward_len = 100;
long reward_timer = 0;
int r = 0 ;
//
int rflag = 0;
long start = 0;

const int encoder_pinA = 11;
const int encoder_pinB = 12;
int encoder_pos = 0;
int encoder_pinA_last = LOW;
int encoder_pinA_state = LOW;

const int solenoid_pin = 3;
int solenoid_pin_state = LOW;
int solenoid_timer = 0;

const int ttl_0_pin = 4;
const int ttl_1_pin = 5;

int cmd = 0;
int incomingByte = 0;  // cmd coming in from Unity
int scan_flag = 0 ;




void setup() {
  // put your setup code here, to run once:
  Serial.begin (57600);

  
  pinMode (encoder_pinA,INPUT);
  pinMode (encoder_pinB,INPUT);
  pinMode (lickport_pin,INPUT);
  pinMode (solenoid_pin,OUTPUT);
  pinMode (ttl_0_pin,OUTPUT);
  pinMode (ttl_1_pin,OUTPUT);
  
  digitalWrite(solenoid_pin,LOW);
  digitalWrite(ttl_0_pin,LOW);
  digitalWrite(ttl_1_pin,LOW);
  
  
  
}

void loop() {


  // read lickport
  lickport_state = digitalRead(lickport_pin);
  lc += lickport_state;

  // read rotary
   encoder_pinA_state = digitalRead(encoder_pinA);
   if ((encoder_pinA_last == LOW) && (encoder_pinA_state == HIGH)) { // if pinA switches 
     if (digitalRead(encoder_pinB) == LOW) { // if pinA leads pinB; going backwards
       encoder_pos--;
     } else { // else; going forwards
       encoder_pos++;
     }
  }

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

      digitalWrite(ttl_1_pin, HIGH);
      digitalWrite(ttl_1_pin,LOW);
      scan_flag = 0;
      break;

    case 9: // start collecting ttl0's for syncing
      scan_flag=1;
    
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
    Serial.print("\t");
    Serial.print(encoder_pos);     
    Serial.println("");
    lc = 0; // reset lick count
    r=0; // reset reward count
    encoder_pos=0; // reset rotary encoder

  }
  
  encoder_pinA_last = encoder_pinA_state; // reset rotary pin value
}
