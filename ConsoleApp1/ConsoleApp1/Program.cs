using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Enumeration;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            communicate().Wait();
        }

        static void callback(GattCharacteristic sender, GattValueChangedEventArgs eventArgs)
        {
            Console.Write(System.Text.Encoding.ASCII.GetString(eventArgs.CharacteristicValue.ToArray()));
        }

        static async Task communicate()
        {
            var selector = GattDeviceService.GetDeviceSelectorFromUuid(new Guid("00050000-6727-11e5-988e-f07959ddcdfb"));
            var collection = await DeviceInformation.FindAllAsync(selector);
            foreach(DeviceInformation info in collection)
            {
                Console.WriteLine(string.Format("Name={0} IsEnabled={1}", info.Name, info.IsEnabled));

                var service = await GattDeviceService.FromIdAsync(info.Id);

                var tx_characteristics = service.GetCharacteristics(ToolboxIdentifications.GattCharacteristicsUuid.TX)[0];
                var rx_characteristics = service.GetChatacteristics(ToolboxIdentifications.GattCharacteristicsUuid.RX)[0];
                rx_characteristics.ValueChanged += callback;
                await rx_characteristics.WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.Notify);
                while (true)
                {
                    var input = Console.ReadLine();
                    var result = await tx_characteristics.WriteValueAsync(System.Text.Encoding.ASCII.GetBytes(input + "\n").AsBuffer(), GattWriteOption.WriteWithoutResponse);
                }
            }
        }
    }
}
