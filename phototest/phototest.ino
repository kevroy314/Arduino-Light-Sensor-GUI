/* Simple test of the functionality of the photo resistor

Connect the photoresistor one leg to pin 0, and pin to +5V
Connect a resistor (around 10k is a good value, higher
values gives higher readings) from pin 0 to GND. (see appendix of arduino notebook page 37 for schematics).

----------------------------------------------------

           PhotoR     10K
 +5    o---/\/\/--.--/\/\/---o GND
                  |
 Pin 0 o-----------

----------------------------------------------------
*/

int lightPin = 0;
int lightPin1 = 1;
int lightPin2 = 2;
int lightPin3 = 3;
int ledPin = 13;
int ledBlinkCount = 0;

void setup()
{
    Serial.begin(9600);
    pinMode(ledPin, OUTPUT);
}

void loop()
{
    Serial.print("A,B,C: ");
    Serial.print(analogRead(lightPin));
    Serial.print(" ");
    Serial.print(analogRead(lightPin1));
    Serial.print(" ");
    Serial.println(analogRead(lightPin2));
    Serial.print(" ");
    Serial.println(analogRead(lightPin3));
    
    ledBlinkCount+=10;
    if(ledBlinkCount<300)
      digitalWrite(ledPin,LOW);
    if(ledBlinkCount>=300)
    {
      digitalWrite(ledPin, HIGH);
      if(ledBlinkCount>=350)
        ledBlinkCount=0;
    }
        
    delay(10);
}
