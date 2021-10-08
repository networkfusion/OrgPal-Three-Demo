using nanoFramework.AwsIoT;

namespace OrgPalThreeDemo.AwsIotCore
{
    public static class MqttConnector
    {
        public static string ConnectionMethod = string.Empty; //TODO: for future, could be WebSocket or MQTT
        public static string Host = string.Empty; //make sure to add your AWS endpoint and region, e.g. "<endpoint>-ats.iot.<region>.amazonaws.com"
        public static int Port = 8883; //add your port if different from the default e.g. 8883 // TODO: not going to change really!
        public static string ThingName = string.Empty; //add your "thing"

        public static string ClientRsaSha256Crt = string.Empty; //Device Certificate copied from AWS
        public static string ClientRsaKey = string.Empty; //Device private key copied from AWS
        public static byte[] RootCA; //AWS root CA
        public static MqttConnectionClient Client;
    }
}
