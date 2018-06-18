#include "DHT.h"

#define DHTPIN 2    

#define DHTTYPE DHT11   
DHT dht(DHTPIN, DHTTYPE);
String readData = "0";
int t;
int h; 
void setup() {
  Serial.begin(9600); // inicjujemy połączenie szeregowe
    
  

    dht.begin(); // włączamy czujnik
  
}

void loop() {


  
  do 
  {
   readData = Serial.readStringUntil('\n');
   
   h = dht.readHumidity(); // odczyt wilgotności
   t = dht.readTemperature(); // odczyt temperatury
   
  


    if (isnan(t) || isnan(h)) 
    {
      Serial.println("Failed to read from DHT");
    } 
    else 
    {
      Serial.println(t);//
      Serial.println(h);


    }
  }while (readData == "1");
 }
 
  

