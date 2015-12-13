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
        BMP280 tempSensor;
        LCD lcd;

        public MainPage()
        {
            this.InitializeComponent();


            lcd = new LCD();

            // TEST CASE
            //lcd.WriteString("The quick brown fox jumped over the lazy dog. The dog incidentally loved supercalifragilicstic words like such as.");
            //LCD.Sleep(2000);
            //lcd.ClearDisplay();

            //lcd.WriteStringOnLine("LEFT", LCD.DisplayLine.LineTwo, LCD.LineJustification.Left);
            ////lcd.ClearLine(LCD.DisplayLine.LineTwo);
            //lcd.WriteStringOnLine("CENTER", LCD.DisplayLine.LineTwo, LCD.LineJustification.Center);
            ////lcd.ClearLine(LCD.DisplayLine.LineThree);
            //lcd.WriteStringOnLine("RIGHT", LCD.DisplayLine.LineTwo, LCD.LineJustification.Right);
            ////lcd.ClearLine(LCD.DisplayLine.LineFour);

            //LCD.Sleep(2000);
            //lcd.ClearDisplay();

            //lcd.WriteStringOnLine("Title Test", LCD.DisplayLine.LineOne, LCD.LineJustification.Center);
            //lcd.WriteString("This string spans many lines, but the title remains consistent. The title could be many lines itself!", LCD.DisplayLine.LineTwo);

            //LCD.Sleep(2000);
            //lcd.ClearDisplay();
            //lcd.WriteString("What       happens  when you have    a lot of   spacesspacesacesspaces");

            //LCD.Sleep(2000);
            //lcd.ClearDisplay();
            //lcd.WriteStringOnLine("I", LCD.DisplayLine.LineOne, LCD.LineJustification.Right);
            ////lcd.ClearLine(LCD.DisplayLine.LineTwo);
            //lcd.WriteStringOnLine("LOVE", LCD.DisplayLine.LineTwo, LCD.LineJustification.Center);
            ////lcd.ClearLine(LCD.DisplayLine.LineThree);
            //lcd.WriteStringOnLine("Baby", LCD.DisplayLine.LineThree, LCD.LineJustification.Right);

            //lcd.ClearDisplay();
            //lcd.WriteStringOnLine("FRIGBERT!", LCD.DisplayLine.LineTwo, LCD.LineJustification.Center);


            //LCD.Sleep(5000);
            //lcd.ClearDisplay();
            //LCD.Sleep(3000);
            //lcd.WriteStringOnLine("and ava", LCD.DisplayLine.LineThree, LCD.LineJustification.Right);

        }
        protected override async void OnNavigatedTo(NavigationEventArgs navArgs)
        {
            tempSensor = new BMP280();
            //Initialize the sensor
            await tempSensor.Initialize();
            while (true)
            {
                await GetTemp();
            }
        }

        public async Task GetTemp()
        {
            try
            {
                float temp = 0;
                float pressure = 0;
                float altitude = 0;

                //This is based on your local sea level pressure (Unit: Hectopascal)
                const float seaLevelPressure = 1020f;

                temp = await tempSensor.ReadTemperature();
                pressure = await tempSensor.ReadPreasure();
                altitude = await tempSensor.ReadAltitude(seaLevelPressure);

                temp = temp * 1.8f + 32f;

                lcd.WriteStringOnLine(temp.ToString() + "F", LCD.DisplayLine.LineTwo, LCD.LineJustification.Right);
                lcd.WriteStringOnLine(pressure.ToString() + "Pa", LCD.DisplayLine.LineThree, LCD.LineJustification.Right);
                lcd.WriteStringOnLine(altitude.ToString() + "m", LCD.DisplayLine.LineFour, LCD.LineJustification.Right);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        const string WebAPIURL = "http://ivyjoy.com/quote.shtml";
        public async Task<string> GetQuote()
        {
            Debug.WriteLine("InternetLed::MakeWebApiCall");

            string responseString = "No response";

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    // Make the call
                    responseString = await client.GetStringAsync(WebAPIURL);

                    // Let us know what the returned string was
                    Debug.WriteLine(String.Format("Response string: [{0}]", responseString));
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }

            // return the blink delay
            return responseString;
        }
    }
}
