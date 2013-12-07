using System;
using System.Collections;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using Microsoft.SPOT;
using Microsoft.SPOT.Presentation;
using Microsoft.SPOT.Presentation.Controls;
using Microsoft.SPOT.Presentation.Media;
using Microsoft.SPOT.Touch;

using Gadgeteer.Networking;
using GT = Gadgeteer;
using GTM = Gadgeteer.Modules;
using Gadgeteer.Modules.GHIElectronics;
using Gadgeteer.Modules.Seeed;

using IndianaJones.NETMF.Json;

using Toolbox.NETMF.NET;

namespace TestSpiderApp
{
    public partial class Program
    {
        ArrayList Measurements = new ArrayList();
        

        // This method is run when the mainboard is powered up or reset.   
        void ProgramStarted()
        {
            /*******************************************************************************************
            Modules added in the Program.gadgeteer designer view are used by typing 
            their name followed by a period, e.g.  button.  or  camera.
            
            Many modules generate useful events. Type +=<tab><tab> to add a handler to an event, e.g.:
                button.ButtonPressed +=<tab><tab>
            
            If you want to do something periodically, use a GT.Timer and handle its Tick event, e.g.:
                GT.Timer timer = new GT.Timer(1000); // every second (1000ms)
                timer.Tick +=<tab><tab>
                timer.Start();
            *******************************************************************************************/

            button.ButtonPressed += new Button.ButtonEventHandler(button_ButtonPressed);

            // Use Debug.Print to show messages in Visual Studio's "Output" window during debugging.
            Debug.Print("Program Started");

            // Event that fires when a measurement is ready
            accelerometer.MeasurementComplete += new Accelerometer.MeasurementCompleteEventHandler(accelerometer_MeasurementComplete);

            // Set the time between measurements and start continuous measurements.
            accelerometer.ContinuousMeasurementInterval = new TimeSpan(0, 0, 0, 0, 250);
            accelerometer.StartContinuousMeasurements();

            ethernet.UseDHCP();
            ethernet.NetworkUp += new GTM.Module.NetworkModule.NetworkEventHandler(ethernet_NetworkUp);
        }

        void ethernet_NetworkUp(GTM.Module.NetworkModule sender, GTM.Module.NetworkModule.NetworkState state)
        {
            Debug.Print("Network Up");             
            var NetworkSettings = ethernet.NetworkSettings;             
            Debug.Print("IP Address: " + NetworkSettings.IPAddress);             
            Debug.Print("DHCP Enabled: " + NetworkSettings.IsDhcpEnabled);             
            Debug.Print("Subnet Mask: " + NetworkSettings.SubnetMask);             
            Debug.Print("Gateway: " + NetworkSettings.GatewayAddress);
        }

        void button_ButtonPressed(Button sender, Button.ButtonState state)
        {
            led.TurnOff();
            accelerometer.StopContinuousMeasurements();

            String rawData = "";
            foreach (Accelerometer.Acceleration measurement in Measurements)
            {
                var set = measurement.X.ToString() + "," + measurement.Y.ToString() + "," + measurement.Z.ToString();
                rawData += set;
            }

            var post = HttpHelper.CreateHttpPostRequest("http://quiet-reef-3526.herokuapp.com", POSTContent.CreateTextBasedContent(rawData), "text/plain");
            post.SendRequest();
            post.ResponseReceived += new HttpRequest.ResponseHandler(post_ResponseReceived);
       } 

        void post_ResponseReceived(HttpRequest sender, HttpResponse response)
        {
            throw new NotImplementedException();
        }

        void accelerometer_MeasurementComplete(Accelerometer sender, Accelerometer.Acceleration acceleration)
        {
            led.TurnOff();
            Debug.Print("Acceleration: " + acceleration);
            double max = 0.1;
            double min = -0.1;

            Measurements.Add(acceleration);

            //if (acceleration.X > 1 || acceleration.X < 0.7 || acceleration.Y > -0.3 || acceleration.Y < -0.4 || acceleration.Z > 0.1 || acceleration.Z < -0.1 )
            if (acceleration.Z > 0.1 || acceleration.Z < -0.1)
            {
                led.AddRed();
            }
            else
            {
                led.AddGreen();
            }
        }
    }
}
