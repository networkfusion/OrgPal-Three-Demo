﻿using nanoFramework.Aws.IoTCore.Devices;

namespace OrgPalThreeDemo.AwsIotCore
{
    public static class MqttConnector
    {
        public static string ConnectionMethod { get; set; } = string.Empty; //TODO: for future, could be WebSocket or MQTT
        public static string Host { get; set; } = string.Empty; //make sure to add your AWS endpoint and region, e.g. "<endpoint>-ats.iot.<region>.amazonaws.com"
        public static int Port { get; set; } = 8883; //add your port if different from the default e.g. 8883 // TODO: not going to change really!
        public static string ThingName { get; set; } = string.Empty; //add your "thing"

        public class ConnectionCertificates
        {
            public string ClientRsaSha256Crt { get; set; } = string.Empty; //Device Certificate copied from AWS
            public string ClientRsaKey { get; set; } = string.Empty; //Device private key copied from AWS
            public byte[] RootCA { get; set; } //AWS root CA
        }

        public static ConnectionCertificates ProvisioningConnectionCertificates { get; set; } = new ConnectionCertificates();
        public static ConnectionCertificates PrimaryConnectionCertificates { get; set; } = new ConnectionCertificates();
        public static ConnectionCertificates RotationConnectionCertificates { get; set; } = new ConnectionCertificates();

        public static MqttConnectionClient Client { get; set; }

        public static bool CheckConfigValid(ConnectionCertificates connectionCertificates)
        {
            if (string.IsNullOrEmpty(connectionCertificates.ClientRsaSha256Crt))
            {
                return false;
            }
            if (connectionCertificates.RootCA.Length < 100) // we will presume that it has to be greater...
            {
                return false;
            }
            if (string.IsNullOrEmpty(connectionCertificates.ClientRsaKey))
            {
                return false;
            }
            if (string.IsNullOrEmpty(Host))
            {
                return false;
            }

            return true;

        }
    }
}
