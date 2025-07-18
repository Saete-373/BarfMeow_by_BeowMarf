# Barf Meow เกมทำอาหารให้เหล่าแมวเหมียวขนปุย

เกมทำอาหารแบบ Single player 2D Top Down ที่ควบคุมตัวละครด้วยท่าทางของมือ โดยเราจะสวมบทบาทเป็นเชฟทำอาหารบาร์ฟส่งให้เหล่าแมวเหมียวขนปุย ซึ่งพัฒนาด้วย Unity Engine ร่วมกับ MediaPipe และ โมเดล Multi-Layer Perceptron (MLP) ที่จะใช้เป็น Controller แทนการใช้ Mouse & Keyboard และ Joy-Stick ในการเล่น

## Feature

- ระบบตรวจจับท่าทางของมือในการควบคุมตัวละคร
- วัตถุดิบและสูตรอาหารที่มีความหลากหลาย
- ระบบเวลาและเงินตราสำหรับสร้างเป้าหมายและความท้าทายให้ผู้เล่น

## พัฒนาด้วย

- Unity Engine (version 2022.3.10f1)
- Unity C# Scripts
- MediaPipe
- Multi-Layer Perceptron (MLP)

| Gameplay |
| ![alt text](/Screenshots/BarfMeow_Screenshot1.png) | ![alt text](/Screenshots/BarfMeow_Screenshot1.png) |

## วิธีเล่น

1. สังเกต Order อาหารแล้วเลือกทำตามชอบ
2. ควบคุมตัวละครไปหยิบวัตถุดิบ เพื่อนำมาประกอบอาหาร
3. วัตถุดิบสด เช่น เนื้อสัตว์, ผัก, นม สามารถนำไปปรุงได้ที่โต๊ะทำอาหาร (มีโต๊ะหั่น, โต๊ะหม้อต้ม และตู้เย็นสำหรับแช่แข็ง)
4. นำอาหารที่ได้ไปส่งแล้วจะได้เงิน
5. จบเกมเมื่อจำนวนเงินถึงเป้าก่อนหมดเวลา

## คู่มือ Action ของแต่ละท่าทางมือ

![alt text](/Screenshots/BarfMeow_Gesture.png)

## การติดตั้ง

1. แตกไฟล์ CSS363-BarfMeow.rar
2. รัน gesture_recognition_service.py ในโฟลเดอร์ Model (ต้องแตกไฟล์ Model/Dataset.rar ก่อน)
3. เปิดไฟล์ CSS363-BarfMeow.exe เพื่อเล่นเกม
