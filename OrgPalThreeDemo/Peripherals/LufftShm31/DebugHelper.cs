using System;
using System.Collections;
using System.Diagnostics;
using System.Text;

namespace OrgPalThreeDemo.Peripherals.LufftShm31
{
    public static class DebugHelper
    {
        public static void DebugListAvailableInputRegisters(Hashtable inRegisters)
        {
            foreach (ModbusInputRegister item in inRegisters.Values)
            {
                Debug.WriteLine($"Reg Address: {item.RegisterAddress}");
                Debug.WriteLine($"Reg Type: {item.RegisterType}");
                Debug.WriteLine($"Reg ValueType: {item.ValueType}");
                Debug.WriteLine($"Reg ValueSF: {item.ValueScaleFactor}");
                Debug.WriteLine($"Reg ValueMin: {item.ValueRange.MinimumValue}");
                Debug.WriteLine($"Reg ValueMax: {item.ValueRange.MaximumValue}");
                Debug.WriteLine();
            }
        }
    }
}
