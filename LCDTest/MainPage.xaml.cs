using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace LCDTest
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        LCD lcd;

        public MainPage()
        {
            this.InitializeComponent();

            lcd = new LCD();
            lcd.WriteString("The quick brown fox jumped over the lazy dog. The dog incidentally loved supercalifragilicstic words like such as.");
            LCD.Sleep(2000);
            lcd.ClearDisplay();

            lcd.WriteStringOnLine("LEFT", LCD.DisplayLine.LineTwo, LCD.LineJustification.Left);
            lcd.WriteStringOnLine("CENTER", LCD.DisplayLine.LineTwo, LCD.LineJustification.Center);
            lcd.WriteStringOnLine("RIGHT", LCD.DisplayLine.LineTwo, LCD.LineJustification.Right);

            LCD.Sleep(2000);
            lcd.ClearDisplay();
            lcd.WriteStringOnLine("Title Test", LCD.DisplayLine.LineOne, LCD.LineJustification.Center);
            lcd.WriteString("This string spans many lines, but the title remains consistent. The title could be many lines itself!", LCD.DisplayLine.LineTwo);

            LCD.Sleep(2000);
            lcd.ClearDisplay();
            lcd.WriteString("What       happens  when you have    a lot of   spacessssssssssssssssssss");

            LCD.Sleep(2000);
            lcd.ClearDisplay();
        }
    }
}
