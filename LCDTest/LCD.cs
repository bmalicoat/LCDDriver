using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Gpio;

namespace LCDTest
{
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

        public enum DisplayLine
        {
            LineOne = 0x80,
            LineTwo = 0xC0,
            LineThree = 0x94,
            LineFour = 0xD4
        }

        public enum LineJustification
        {
            Left = 0,
            Center,
            Right
        }

        public const int LINE1 = 0x80;
        public const int LINE2 = 0xC0;
        public const int LINE3 = 0x94;
        public const int LINE4 = 0xD4;

        private const int WIDTH = 20;

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

        private byte ClearDisplayCommand = 0x01;
        private byte ReturnHomeCommand = 0x02;
        private byte EntrySetModeCommand = 0x04;
        private byte DisplayControlCommand = 008;
        private byte CursorShiftCommand = 0x10;
        private byte FunctionSetCommand = 0x20;
        private byte SetCGRAMAddressCommand = 0x40;
        private byte SetDDRAMAddressCommand = 0x80;

        private byte DisplayEntryRight = 0x00;
        private byte DisplayEntryLeft = 0x02;

        private byte ShiftStyleDecrement = 0x00;
        private byte ShiftStyleIncrement = 0x01;

        private byte DisplayOff = 0x00;
        private byte DisplayOn = 0x04;

        private byte CursorOff = 0x00;
        private byte CursorOn = 0x02;

        private byte BlinkOff = 0x00;
        private byte BlinkOn = 0x01;

        private byte DisplayMove = 0x08;
        private byte CursorMove = 0x00;
        private byte MoveLeft = 0x00;
        private byte MoveRight = 0x04;

        private byte Mode8Bit = 0x10;
        private byte Mode4Bit = 0x00;
        private byte Mode2Line = 0x08;
        private byte Mode1Line = 0x00;
        private byte Mode5x10 = 0x04;
        private byte Mode5x8 = 0x00;

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
            SendBytes(0x33, RegisterMode.Command);
            SendBytes(0x32, RegisterMode.Command);
            SendBytes(0x06, RegisterMode.Command);
            SendBytes(0x0C, RegisterMode.Command);
            SendBytes(0x28, RegisterMode.Command);
            ClearDisplay();
            Sleep(5);
        }

        public void ClearDisplay()
        {
            SendCommand(ClearDisplayCommand);
        }

        public void ReturnHome()
        {
            SendCommand(ReturnHomeCommand);
        }

        private void SendCommand(int data)
        {
            SendBytes(data, RegisterMode.Command);
        }

        private void SendData(int data)
        {
            SendBytes(data, RegisterMode.Data);
        }

        private void SendBytes(int data, RegisterMode mode)
        {
            RegisterSelectPin.Write((GpioPinValue)mode);

            // In 4-bit mode, we'll write the high bits to data pins 4-7,
            // then toggle the enable pin, then write the low bits to 4-7
            ClearDataPins();

            WriteHighIfData(data, 0x10, DataBusPin4);
            WriteHighIfData(data, 0x20, DataBusPin5);
            WriteHighIfData(data, 0x40, DataBusPin6);
            WriteHighIfData(data, 0x80, DataBusPin7);

            ToggleEnablePin();

            ClearDataPins();

            WriteHighIfData(data, 0x01, DataBusPin4);
            WriteHighIfData(data, 0x02, DataBusPin5);
            WriteHighIfData(data, 0x04, DataBusPin6);
            WriteHighIfData(data, 0x08, DataBusPin7);

            ToggleEnablePin();
        }

        private void ClearDataPins()
        {
            DataBusPin4.Write(GpioPinValue.Low);
            DataBusPin5.Write(GpioPinValue.Low);
            DataBusPin6.Write(GpioPinValue.Low);
            DataBusPin7.Write(GpioPinValue.Low);
        }

        private void WriteHighIfData(int data, byte mask, GpioPin pin)
        {
            if ((data & mask) != 0)
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

        private DisplayLine AdvanceLine(DisplayLine currentLine, DisplayLine lineToResetTo = DisplayLine.LineOne)
        {
            DisplayLine newLine = DisplayLine.LineOne;

            switch (currentLine)
            {
                case DisplayLine.LineOne:
                    newLine = DisplayLine.LineTwo;
                    break;
                case DisplayLine.LineTwo:
                    newLine = DisplayLine.LineThree;
                    break;
                case DisplayLine.LineThree:
                    newLine = DisplayLine.LineFour;
                    break;
                case DisplayLine.LineFour:
                    newLine = lineToResetTo;

                    Sleep(500);

                    if (lineToResetTo == DisplayLine.LineOne)
                    {
                        ClearDisplay();
                    }
                    else
                    {
                        switch (lineToResetTo)
                        {
                            case DisplayLine.LineTwo:
                                ClearLine(DisplayLine.LineTwo);
                                ClearLine(DisplayLine.LineThree);
                                ClearLine(DisplayLine.LineFour);
                                break;
                            case DisplayLine.LineThree:
                                ClearLine(DisplayLine.LineThree);
                                ClearLine(DisplayLine.LineFour);
                                break;
                            case DisplayLine.LineFour:
                                ClearLine(DisplayLine.LineFour);
                                break;
                        }
                    }
                    break;
            }

            SendCommand((int)newLine);

            return newLine;
        }

        private string JustifyString(string str, LineJustification justification)
        {
            string justified = str;
            int whitespace = WIDTH - str.Length;

            if (whitespace > 0)
            {
                switch (justification)
                {
                    case LineJustification.Left:
                        // do nothing, already left justified
                        break;
                    case LineJustification.Center:
                        if (whitespace % 2 == 0)
                        {
                            justified = WhiteSpace(whitespace / 2) + str + WhiteSpace(whitespace / 2);
                        }
                        else
                        {
                            justified = WhiteSpace(whitespace / 2) + str + WhiteSpace((whitespace - 1) / 2);
                        }

                        break;
                    case LineJustification.Right:
                        justified = WhiteSpace(whitespace) + str;
                        break;
                }
            }

            return justified;
        }

        private string WhiteSpace(int number)
        {
            string ws = "";

            for (int i = 0; i < number; i++)
            {
                ws += " ";
            }

            return ws;
        }

        public static void Sleep(int ms)
        {
            new System.Threading.ManualResetEvent(false).WaitOne(ms);
        }

        public void WriteStringOnLine(string str, DisplayLine line = DisplayLine.LineOne, LineJustification justification = LineJustification.Left)
        {
            SendCommand((int)line);

            if (justification != LineJustification.Left)
            {
                str = JustifyString(str, justification);
            }

            byte[] asciiValues = Encoding.ASCII.GetBytes(str);

            for (int i = 0; i < str.Length && i < WIDTH; i++)
            {
                SendData(asciiValues[i]);
            }
        }

        public void ClearLine(DisplayLine line)
        {
            SendCommand((int)line);

            for (int i = 0; i < WIDTH; i++)
            {
                SendData(0x20);
            }
        }

        public void WriteString(string str, DisplayLine startLine = DisplayLine.LineOne)
        {
            int currentCharInLine = 1;
            int remainingCharsInLine = WIDTH;

            DisplayLine currentLine = startLine;
            SendCommand((int)currentLine);

            string[] words = str.Split(' ');

            foreach (string word in words)
            {
                bool breakWord = false;
                byte[] asciiValues = Encoding.ASCII.GetBytes(word);

                remainingCharsInLine = WIDTH - currentCharInLine;

                if ((remainingCharsInLine == 0 && word.Length <= WIDTH) || (word.Length > remainingCharsInLine && word.Length <= WIDTH))
                {
                    currentLine = AdvanceLine(currentLine, startLine);
                    currentCharInLine = 1;
                }
                else if (word.Length > WIDTH)
                {
                    breakWord = true;
                }

                for (int i = 0; i < word.Length; i++)
                {
                    remainingCharsInLine = WIDTH - currentCharInLine;
                    if (remainingCharsInLine == 0 && breakWord)
                    {

                        if (i != 0)
                        {
                            // only write the - if we have actually started writing the word
                            SendData(0x2D);
                        }

                        currentLine = AdvanceLine(currentLine, startLine);
                        currentCharInLine = 1;
                    }

                    SendData(asciiValues[i]);
                    currentCharInLine++;
                }

                SendData(0x20);
                currentCharInLine++;
            }
        }
    }
}
