using System;
using System.Text;
using nanoFramework.Hardware.Stm32;
using System.Diagnostics;

namespace OrgPalThreeDemo.Networking
{
    /// <summary>
    /// A Helper class for handling STM32 board default network MAC addresses.
    /// </summary>
    public static class MacAddressHelper
    {
        private static byte[] _stm32DefaultMacAddress = new byte[6] { 0x00, 0x80, 0xE1, 0x01, 0x35, 0xD1 };


        /// <summary>
        /// Checks and if required sets the MAC on the current board.
        /// </summary>
        /// <remarks>
        /// This should be called at the begining of your `main` function.
        /// </remarks>
        public static void CheckSetDefaultMac()
        {
            Debug.WriteLine($"System MAC= {GetMacAsString()}");

            if (IsMacStaticStm32DeveloperId())
            {
                Debug.WriteLine($"WARNING: USING STM32 DEVELOPER MAC");
                Debug.WriteLine($"It is static and might be duplicated on the network!");
                Debug.WriteLine($"Due to this, we will assign a randomized locally administered one!");

                SetMacFromUniqueDeviceId();

                Debug.WriteLine($"New MAC Set: {GetMacAsString()}");
            }
        }

        /// <summary>
        /// Checks whether the currently set MAC address is a statically assigned STM32 development one.
        /// </summary>
        /// <returns>true if it is.</returns>
        public static bool IsMacStaticStm32DeveloperId()
        {
            return SequenceEqual(_stm32DefaultMacAddress, System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces()[0].PhysicalAddress);
        }

        /// <summary>
        /// Generates a unique MAC address from the mcu's serial number,
        /// sets it to be a "locally administered" MAC and updates the configuration block
        /// </summary>
        /// <remarks>
        /// The board will reboot after this change is made and should only be called once.
        /// </remarks>
        public static void SetMacFromUniqueDeviceId(bool multicast = false)
        {

            byte[] _newMacArray = new byte[6];

            Array.Copy(Utilities.UniqueDeviceId, 6, _newMacArray, 0, 6); //use the last 6 bytes of the unique ID (as more likely to be unique?!
            
            LocalizeMac(ref _newMacArray, multicast); // used so that it can have unit tests!
            
            System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces()[0].PhysicalAddress = _newMacArray;
        }

        /// <summary>
        /// Returns the current MAC address
        /// </summary>
        /// <param name="seperator">use a custom seperator (default is `:`)</param>
        /// <returns>Network MAC Address as a string</returns>
        public static string GetMacAsString(char seperator = ':')
        {
            // TODO: Word has it that string builder is slower on nF! (if required, suppress S1643)!
            var sb = new StringBuilder();
            foreach (byte b in System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces()[0].PhysicalAddress)
            {
                sb.Append(b.ToString("X2"));
                sb.Append(seperator);
            }
            return sb.ToString().TrimEnd(seperator);
        }

        /// <summary>
        /// Convert the MAC "localized" range.
        /// </summary>
        /// <param name="macBytes">Bytes of the MAC address</param>
        /// <param name="multicast">Whether it should be multicast</param>
        private static void LocalizeMac(ref byte[] macBytes, bool multicast)
        {
            // Resources: https://www.hellion.org.uk/cgi-bin/randmac.pl?source=1
            // https://en.wikipedia.org/wiki/MAC_address Universal/Local and Individual/Group bits in MAC addresses table.
            macBytes[0] &= 0xfc;
            if (multicast)
            {
                macBytes[0] |= 0x1; // Set the bit to multicast (not usually required in most situations).
            }
            macBytes[0] |= 0x2; // Set the bit to be a locally administered MAC.
        }

        /// <summary>
        /// Compare the elements of 2 different byte arrays
        /// </summary>
        /// <param name="first">First array to compare</param>
        /// <param name="second">Second array to compare</param>
        /// <returns>True if all elements are equa</returns>
        private static bool SequenceEqual(this byte[] first, byte[] second)
        {
            if (first.Length != second.Length)
            {
                return false;
            }

            for (int i = 0; i < first.Length; i++)
            {
                if (first[i] != second[i])
                {
                    return false;
                }
            }

            return true;
        }
    }
}
