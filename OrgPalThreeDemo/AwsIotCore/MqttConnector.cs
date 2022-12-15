using nanoFramework.Aws.IoTCore.Devices;

namespace OrgPalThreeDemo.AwsIotCore
{
    public static class MqttConnector
    {
        public static string ConnectionMethod { get; set; } = string.Empty; //TODO: for future, could be WebSocket or MQTT
        public static string Host { get; set; } = string.Empty; //make sure to add your AWS endpoint and region, e.g. "<endpoint>-ats.iot.<region>.amazonaws.com"
        public static int Port { get; set; } = 8883; //add your port if different from the default e.g. 8883 // TODO: not going to change really!
        public static string ThingName { get; set; } = string.Empty; //add your "thing"

        public static string ClientRsaSha256Crt { get; set; } = string.Empty; //Device Certificate copied from AWS
        public static string ClientRsaKey { get; set; } = string.Empty; //Device private key copied from AWS
        public static byte[] RootCA { get; set; } //AWS root CA
        public static MqttConnectionClient Client { get; set; }

        public static bool CheckConfigValid()
        {
            if (string.IsNullOrEmpty(AwsIotCore.MqttConnector.ClientRsaSha256Crt))
            {
                return false;
            }
            if (AwsIotCore.MqttConnector.RootCA.Length < 100) // we will presume that it has to be greater...
            {
                return false;
            }
            if (string.IsNullOrEmpty(AwsIotCore.MqttConnector.ClientRsaKey))
            {
                return false;
            }
            if (string.IsNullOrEmpty(AwsIotCore.MqttConnector.Host))
            {
                return false;
            }

            return true;

        }
    }
}
