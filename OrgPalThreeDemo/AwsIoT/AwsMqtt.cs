using System;
using System.Text;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;

namespace AwsIoT
{
    public static class AwsMqtt
    {

        public static string Host = string.Empty; //make sure to add your AWS endpoint and region, e.g. "<endpoint>-ats.iot.<region>.amazonaws.com"
        public static int Port = 8883; //add your port if different from the default e.g. 8883
        public static string ThingName = string.Empty; //add your "thing"

        public static string ClientRsaSha256Crt = string.Empty; //Device Certificate copied from AWS
        public static string ClientRsaKey = string.Empty; //Device private key copied from AWS
        public static byte[] RootCA; //AWS root CA
        public static MqttClient Client;
        public static AwsShadow Shadow = new AwsShadow();
    }
}
