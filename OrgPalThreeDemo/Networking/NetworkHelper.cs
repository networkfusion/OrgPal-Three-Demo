using System;
using System.Diagnostics;
using System.Threading;
using System.Net.NetworkInformation;

namespace OrgPalThreeDemo.Networking
{
    /// <summary>
    /// Connection Error class
    /// </summary>
    public class ConnectionError
    {
        /// <summary>
        /// Connection Error class constructor
        /// </summary>
        /// <param name="error">Error message.</param>
        /// <param name="ex">Exception</param>
        public ConnectionError(string error, Exception ex = null)
        {
            Error = error;
            Exception = ex;
        }

        /// <summary>
        /// The error message
        /// </summary>
        public string Error { get; internal set; }

        /// <summary>
        /// The possible Exception
        /// </summary>
        public Exception Exception { get; internal set; }
    }

    public class NetworkHelper
    {
        /// <summary>
        /// The error connection type
        /// </summary>
        public static ConnectionError ConnectionError { get; internal set; } = new ConnectionError("Not setup yet");


        /// <summary>
        /// Wait for a valid IP and potentially a date
        /// </summary>
        /// <param name="setDateTime">True to wait for a valid date</param>
        /// <param name="networkInterfaceType">The tye of interface.</param>
        /// <param name="token">A cancellation token</param>
        /// <returns>True if success</returns>
        public static bool WaitForValidIPAndDate(bool setDateTime, NetworkInterfaceType networkInterfaceType, CancellationToken token)
        {
            Debug.WriteLine("Checking for IP");
            while (!token.IsCancellationRequested && !IsValidIpAddress(networkInterfaceType))
            {
                Thread.Sleep(200);
            }

            if (token.IsCancellationRequested)
            {
                ConnectionError = new ConnectionError("Token expired while checking IP address", null);
                return false;
            }

            if (setDateTime)
            {
                Debug.WriteLine("Setting up system clock...");
                while (!token.IsCancellationRequested && !IsValidDateTime())
                {
                    Thread.Sleep(200);
                }
            }

            if (token.IsCancellationRequested)
            {
                ConnectionError = new ConnectionError("Token expired while setting system clock", null);
                return false;
            }

            if (setDateTime)
            {
                Debug.WriteLine($"We have a valid date: {DateTime.UtcNow}");
            }

            ConnectionError = new ConnectionError("No error", null);
            return true;
        }

        /// <summary>
        /// Check if the DateTime is valid.
        /// </summary>
        /// <returns>True if valid.</returns>
        public static bool IsValidDateTime() => DateTime.UtcNow.Year > 2020;

        /// <summary>
        /// Check if there is a valid IP address on a specific interface type.
        /// </summary>
        /// <param name="interfaceType">The type of interface.</param>
        /// <returns>True if valid.</returns>
        public static bool IsValidIpAddress(NetworkInterfaceType interfaceType)
        {
            NetworkInterface[] nis = NetworkInterface.GetAllNetworkInterfaces();
            foreach (NetworkInterface ni in nis)
            {
                if (ni.NetworkInterfaceType != interfaceType)
                {
                    break;
                }

                if (ni.IPv4Address != null && ni.IPv4Address.Length > 0)
                {
                    if (ni.IPv4Address[0] != '0')
                    {
                        Debug.WriteLine($"We have a valid IPv4 address: {ni.IPv4Address}");
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
