#LCD Driver

I used a Raspberry Pi 2 Model B v1.1 with Windows 10 IoT and an LCD that had a HD44780 controller.

Here is the configuration I used to test the LCD functionality:
![Alt text](https://github.com/bmalicoat/LCDTest/blob/master/layout.png "LCD test schematic")

Here is an overview of the LCD wiring:

```
1 = GND
2 = 5V
3 = Contrast (0-5V)
4 = RS (Register Select) Command Register when low, Data Register when high *GPIO 4*
5 = RW (Read/Write) Grounded to be low signal so that we always are writing to LCD
6 = E (Enable)  *GPIO 5*
7-10 Data 0-3 (Not used in 4-bit mode)
11-14 Data 4-7 (Used in 4-bit mode)  *GPIO 12, 13, 16, 18*
15 = Backlight (+5V through resistor)
16 = Backlight (GND)
```
