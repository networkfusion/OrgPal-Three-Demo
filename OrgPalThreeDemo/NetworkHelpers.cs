//
// Copyright (c) 2019 The nanoFramework project contributors
// See LICENSE file in the project root for full license information.
//

using System;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Threading;

namespace nanoFramework.Networking
{
    internal class NetworkHelpers
    {

        private static bool _requiresDateTime;

        static public ManualResetEvent IpAddressAvailable = new ManualResetEvent(false);
        static public ManualResetEvent DateTimeAvailable = new ManualResetEvent(false);

        internal static void SetupAndConnectNetwork(bool requiresDateTime = false)
        {
            NetworkChange.NetworkAddressChanged += AddressChangedCallback;
            NetworkChange.NetworkAvailabilityChanged += NetworkChange_NetworkAvailabilityChanged;

            _requiresDateTime = requiresDateTime;
            new Thread(WorkingThread).Start();
        }

        private static void NetworkChange_NetworkAvailabilityChanged(object sender, NetworkAvailabilityEventArgs e)
        {
            Debug.WriteLine("Network availability changed");
        }

        internal static void WorkingThread()
        {
            NetworkInterface[] nis = NetworkInterface.GetAllNetworkInterfaces();

            if (nis.Length > 0)
            {
                // get the first interface
                NetworkInterface ni = nis[0];


                if (ni.NetworkInterfaceType != NetworkInterfaceType.Wireless80211)
                {
                    // network interface is Ethernet
                    Debug.WriteLine("Network connection is: Ethernet");
                }

                ni.EnableAutomaticDns();
                ni.EnableDhcp();

                // check if we have an IP
                CheckIP();

                if (_requiresDateTime)
                {
                    IpAddressAvailable.WaitOne();

                    SetDateTime();
                }
            }
            else
            {
                throw new NotSupportedException("ERROR: there is no network interface configured.\r\nOpen the 'Edit Network Configuration' in Device Explorer and configure one.");
            }
        }

        private static void SetDateTime()
        {
            int retryCount = 30;

            Debug.WriteLine("Waiting for a valid date & time...");

            // if SNTP is available and enabled on target device this can be skipped because we should have a valid date & time
            while (DateTime.UtcNow.Year < 2021)
            {
                // force update if we haven't a valid time after 30 seconds
                if (retryCount-- == 0)
                {
                    Debug.WriteLine("Forcing SNTP update...");
                    Sntp.Server2 = "uk.pool.ntp.org";
                    Sntp.UpdateNow();

                    // reset counter
                    retryCount = 30;
                }

                // wait for valid date & time
                Thread.Sleep(1000);
            }

            Debug.WriteLine($"We have valid date & time: {DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")}");

            DateTimeAvailable.Set();
        }

        private static bool CheckIP()
        {
            Debug.WriteLine("Checking for IP");

            var ni = NetworkInterface.GetAllNetworkInterfaces()[0];

            if (ni.IPv4Address != null && ni.IPv4Address.Length > 0)
            {
                if (ni.IPv4Address[0] != '0')
                {
                    Debug.WriteLine($"We have and IP: {ni.IPv4Address}");
                    IpAddressAvailable.Set();

                    return true;
                }
            }

            Debug.WriteLine("NO IP");

            return false;
        }

        static void AddressChangedCallback(object sender, EventArgs e)
        {
            CheckIP();
        }
    }
}
