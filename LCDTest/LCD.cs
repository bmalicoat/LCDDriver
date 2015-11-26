using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Gpio;

namespace LCDTest
{
    //LCD_CHR = True
    //LCD_CMD = False
    //# Timing constants
    //E_PULSE = 0.0005
    //E_DELAY = 0.0005

    class LCD
    {
        // LCD Wiring
        // 1 = GND
        // 2 = 5V
        // 3 = Contrast (0-5V) GND for now
        // 4 = RS (Register Select) Command Register when low, Data Register when high *GPIO 4*
        // 5 = RW (Read/Write) Grounded to be low signal so that we always are writing to LCD
        // 6 = E (Enable)  *GPIO 5*
        // 7-10 Data 0-3 (Not used in 4-bit mode)
        // 11-14 Data 4-7 (Used in 4-bit mode)  *GPIO 12, 13, 16, 18*
        // 15 = Backlight (+5V through resistor)
        // 16 = Backlight (GND)

        private const int RS = 4;
        private const int E = 5;
        private const int DB4 = 12;
        private const int DB5 = 13;
        private const int DB6 = 16;
        private const int DB7 = 18;

        private const int LINE1 = 0x80;
        private const int LINE2 = 0xC0;

        private const int WIDTH = 16;

        // GPIO Pins
        private GpioPin RegisterSelectPin;
        private GpioPin EnablePin;
        private GpioPin DataBusPin4;
        private GpioPin DataBusPin5;
        private GpioPin DataBusPin6;
        private GpioPin DataBusPin7;

        private enum RegisterMode
        {
            Command = 0,
            Data = 1
        }

        public LCD()
        {
            var gpio = GpioController.GetDefault();

            if (gpio == null)
            {
                Debug.WriteLine("Couldn't reach GPIO controller");
                return;
            }

            RegisterSelectPin = gpio.OpenPin(RS);
            EnablePin = gpio.OpenPin(E);
            DataBusPin4 = gpio.OpenPin(DB4);
            DataBusPin5 = gpio.OpenPin(DB5);
            DataBusPin6 = gpio.OpenPin(DB6);
            DataBusPin7 = gpio.OpenPin(DB7);

            RegisterSelectPin.SetDriveMode(GpioPinDriveMode.Output);
            EnablePin.SetDriveMode(GpioPinDriveMode.Output);
            DataBusPin4.SetDriveMode(GpioPinDriveMode.Output);
            DataBusPin5.SetDriveMode(GpioPinDriveMode.Output);
            DataBusPin6.SetDriveMode(GpioPinDriveMode.Output);
            DataBusPin7.SetDriveMode(GpioPinDriveMode.Output);

            Initialize();
        }

        private void Initialize()
        {
            SendBytes(0x30, RegisterMode.Command); // Initialize part 1
            Sleep(5);
            SendBytes(0x30, RegisterMode.Command); // Initialize part 2
            Sleep(1);
            SendBytes(0x30, RegisterMode.Command); // Initialize part 3
            SendBytes(0x3F, RegisterMode.Command); // Set number of display lines and font
            SendBytes(0x07, RegisterMode.Command); // Set 
            //SendBytes(0x06, RegisterMode.Command); // Cursor move direction
            SendBytes(0x0C, RegisterMode.Command); // Display On, Cursor Off, Blink Off
            //SendBytes(0x28, RegisterMode.Command); // Data length, Number of lines, Font size
            //SendBytes(0x01, RegisterMode.Command); // Clear Display
            Sleep(5);
        }

        public void ClearDisplay()
        {
            SendBytes(0x01, RegisterMode.Command);
        }

        public void DisplayOff()
        {
            SendBytes(0x08, RegisterMode.Command);
        }

        private void SendBytes(byte data, RegisterMode mode)
        {
            RegisterSelectPin.Write((GpioPinValue)mode);

            // In 4-bit mode, we'll write the high bits to data pins 4-7,
            // then toggle the enable pin, then write the low bits to 4-7
            ClearDataPins();

            WriteHighIfData(data, 0x10, DataBusPin4);
            WriteHighIfData(data, 0x20, DataBusPin5);
            WriteHighIfData(data, 0x30, DataBusPin6);
            WriteHighIfData(data, 0x40, DataBusPin7);

            ToggleEnablePin();

            ClearDataPins();

            WriteHighIfData(data, 0x01, DataBusPin4);
            WriteHighIfData(data, 0x02, DataBusPin5);
            WriteHighIfData(data, 0x03, DataBusPin6);
            WriteHighIfData(data, 0x04, DataBusPin7);
        }

        private void ClearDataPins()
        {
            DataBusPin4.Write(GpioPinValue.Low);
            DataBusPin5.Write(GpioPinValue.Low);
            DataBusPin6.Write(GpioPinValue.Low);
            DataBusPin7.Write(GpioPinValue.Low);
        }

        private void WriteHighIfData(byte data, byte mask, GpioPin pin)
        {
            if ((data & mask) == 1)
            {
                pin.Write(GpioPinValue.High);
            }
        }

        private void ToggleEnablePin()
        {
            Sleep(5);
            EnablePin.Write(GpioPinValue.High);
            Sleep(5);
            EnablePin.Write(GpioPinValue.Low);
            Sleep(5);
        }

        static void Sleep(int ms)
        {
            new System.Threading.ManualResetEvent(false).WaitOne(ms);
        }

        public void WriteString(string str)
        {
            foreach (char c in str)
            {
                SendBytes((byte)c, RegisterMode.Data);
            }
        }
    }
}
