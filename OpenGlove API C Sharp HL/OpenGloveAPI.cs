﻿using OpenGlove_API_C_Sharp_HL.ServiceReference1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using WebSocketSharp;

namespace OpenGlove_API_C_Sharp_HL
{
    public class OpenGloveAPI
    {
        /// <summary>
        /// Singleton instance of the API
        /// </summary>
        private static OpenGloveAPI instance;
        bool WebSocketActive;
        /// <summary>
        /// Client for the WCF service
        /// </summary>
        private OGServiceClient serviceClient;

        public delegate void FingerMovement(int region, int value);
        WebSocket WebSocketClient;
        public event FingerMovement fingersFunction;
        OpenGloveAPI()
        {
            NetHttpBinding binding = new NetHttpBinding();
            EndpointAddress address = new EndpointAddress("http://localhost:8733/Design_Time_Addresses/OpenGloveWCF/OGService/");
            serviceClient = new OGServiceClient(binding, address);
            WebSocketActive = false;
        }

   
        /// <summary>
        /// Gets the current API instance
        /// </summary>
        /// <returns>Current API instance</returns>
        public static OpenGloveAPI GetInstance()
        {
            if (instance == null)
            {
                instance = new OpenGloveAPI();
            }
            return instance;
        }

       // WebSocket ws = new WebSocket("ws://localhost:9876/rightGlove");

        public void readFingers( )
        {
            using (WebSocketClient)
            {
                WebSocketClient.OnMessage += (sender, e) => {
                    int mapping, value;
                    string[] words;
                    if (e.Data != null)
                    {
                        words = e.Data.Split(',');
                        try
                        {
                            mapping = Int32.Parse(words[0]);
                            value = Int32.Parse(words[1]);
                            fingersFunction?.Invoke(mapping, value);
                        }
                        catch
                        {
                            Console.WriteLine("ERROR");
                        }
                    }
                };
                WebSocketClient.Connect();
                WebSocketActive = true;
                while (WebSocketActive == true) { }
            }
        }

        Task mytask;
        public void captureFingers(Glove selectedGlove)
        {
            WebSocketClient = new WebSocket("ws://localhost:9876/" + selectedGlove.Port);
            try
            {
                mytask = Task.Run(() =>
                {
                    readFingers();
                });
            }
            catch
            {

            }
        }

        public void stopReadFingers()
        {
            WebSocketClient.Close();
            WebSocketActive = false;
        }
        

        /// <summary>
        /// List of current connected devices
        /// </summary>
        private List<Glove> devices;

        /// <summary>
        /// Property for getting current devices
        /// </summary>
        public List<Glove> Devices
        {
            get
            {
                devices = serviceClient.GetGloves().ToList();
                return devices;
            }
        }

        /// <summary>
        /// Refreshes the current devices list
        /// </summary>
        /// <returns></returns>
        public List<Glove> UpdateDevices()
        {
            return serviceClient.RefreshGloves().ToList();
        }

        public int addFlexor(Glove selectedGlove, int flexor, int mapping)
        {
            return this.serviceClient.addFlexor(selectedGlove.BluetoothAddress, flexor, mapping);
        }

        public int removeFlexor(Glove selectedGlove, int mapping)
        {
            return this.serviceClient.removeFlexor(selectedGlove.BluetoothAddress, mapping);
        }

        public void calibrateFlexors(Glove selectedGlove)
        {
           this.serviceClient.calibrateFlexors(selectedGlove.BluetoothAddress);
        }

        public void confirmCalibration(Glove selectedGlove)
        {
            this.serviceClient.confirmCalibration(selectedGlove.BluetoothAddress);
        }

        public void setThreshold(Glove selectedGlove, int value)
        {
            this.serviceClient.setThreshold(selectedGlove.BluetoothAddress,value);
        }

        public void resetFlexors(Glove selectedGlove)
        {
            this.serviceClient.resetFlexors(selectedGlove.BluetoothAddress);
        }






        /// <summary>
        /// Establishes connection with a glove
        /// </summary>
        /// <param name="selectedGlove">A Glove object to be connected</param>
        /// <returns>Result code</returns>
        public int Connect(Glove selectedGlove)
        {
            try
            {
                return this.serviceClient.Connect(selectedGlove.BluetoothAddress);
            }
            catch (Exception)
            {

                return -1;
            }
        }

        /// <summary>
        /// Closes a connection with a glove
        /// </summary>
        /// <param name="selectedGlove">A Glove object to be connected</param>
        /// <returns>Result code</returns>
        public int Disconnect(Glove selectedGlove)
        {
            try
            {
                return this.serviceClient.Disconnect(selectedGlove.BluetoothAddress);
            }
            catch (Exception)
            {

                return -1;
            }
        }

        /// <summary>
        /// Activates a region with certain intensity
        /// </summary>
        /// <param name="selectedGlove"></param>
        /// <param name="region"></param>
        /// <param name="intensity"></param>
        public void Activate(Glove selectedGlove, int region, int intensity)
        {
            int actuator = -1;
            foreach (var item in selectedGlove.GloveConfiguration.GloveProfile.Mappings)
            {
                if (item.Key.Equals(region.ToString()))
                {
                    actuator = Int32.Parse(item.Value);
                    break;
                }
            }
            if (actuator == -1)
            {
                return;
            }
            this.serviceClient.Activate(selectedGlove.BluetoothAddress, actuator, intensity);

        }

        public void Activate(Glove selectedGlove, List<int> regions, List<int> intensityList)
        {
            List<int> actuators = new List<int>();

            foreach (var region in regions)
            {
                int actuator = -1;
                foreach (var item in selectedGlove.GloveConfiguration.GloveProfile.Mappings)
                {
                    if (item.Key.Equals(region.ToString()))
                    {
                        actuator = Int32.Parse(item.Value);
                        break;
                    }
                }
                if (actuator == -1)
                {
                    return;
                }
                actuators.Add(actuator);
            }

            this.serviceClient.ActivateMany(selectedGlove.BluetoothAddress, actuators.ToArray(), intensityList.ToArray());
        }
    }


    public enum PalmarRegion
    {
        FingerSmallDistal,
        FingerRingDistal,
        FingerMiddleDistal,
        FingerIndexDistal,

        FingerSmallMiddle,
        FingerRingMiddle,
        FingerMiddleMiddle,
        FingerIndexMiddle,

        FingerSmallProximal,
        FingerRingProximal,
        FingerMiddleProximal,
        FingerIndexProximal,

        PalmSmallDistal,
        PalmRingDistal,
        PalmMiddleDistal,
        PalmIndexDistal,

        PalmSmallProximal,
        PalmRingProximal,
        PalmMiddleProximal,
        PalmIndexProximal,

        HypoThenarSmall,
        HypoThenarRing,
        ThenarMiddle,
        ThenarIndex,

        FingerThumbProximal,
        FingerThumbDistal,

        HypoThenarDistal,
        Thenar,

        HypoThenarProximal
    }

    public enum DorsalRegion
    {
        FingerSmallDistal = 29,
        FingerRingDistal,
        FingerMiddleDistal,
        FingerIndexDistal,

        FingerSmallMiddle,
        FingerRingMiddle,
        FingerMiddleMiddle,
        FingerIndexMiddle,

        FingerSmallProximal,
        FingerRingProximal,
        FingerMiddleProximal,
        FingerIndexProximal,

        PalmSmallDistal,
        PalmRingDistal,
        PalmMiddleDistal,
        PalmIndexDistal,

        PalmSmallProximal,
        PalmRingProximal,
        PalmMiddleProximal,
        PalmIndexProximal,

        HypoThenarSmall,
        HypoThenarRing,
        ThenarMiddle,
        ThenarIndex,

        FingerThumbProximal,
        FingerThumbDistal,

        HypoThenarDistal,
        Thenar,

        HypoThenarProximal
    }

    public enum FlexorsRegion
    {
        ThumbInterphalangealJoint = 0,
        IndexInterphalangealJoint,
        MiddleInterphalangealJoint,
        RingInterphalangealJoint,
        SmallInterphalangealJoint,

        ThumbMetacarpophalangealJoint,
        IndexMetacarpophalangealJoint,
        MiddleMetacarpophalangealJoint,
        RingMetacarpophalangealJoint,
        SmallMetacarpophalangealJoint
    }
}
