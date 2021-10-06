using nanoFramework.TestFramework;
using System;

namespace nanoFramwwork.AwsIoT.Tests
{
    [TestClass]
    public class ShadowTests
    {
        [TestMethod]
        public void get_shadow_to_class()
        {
        }

        [TestMethod]
        public void get_shadow_to_json()
        {
        }

        [TestMethod]
        public void update_shadow_to_class()
        {
        }

        [TestMethod]
        public void update_shadow_to_json()
        {
        }
    }
}


//using System;
//using System.Diagnostics;
//using System.Security.Cryptography.X509Certificates;
//using System.Threading;
//using nanoFramework.AwsIot;

//namespace nanoFramework.Aws.Tests
//{
//    public class program //TODO: finish tests!
//    {
//        MqttConnectionClient _awsMqttClient;

//        void Setup() {
//            //TODO: should this use the AWS CLI to provision an instance, or at least reccomend one is already setup???

//            //X509Certificate caCert = new X509Certificate(AwsIoTCoreDefaultRootCA);
//            //X509Certificate2 clientCert = new X509Certificate2(AwsIoTCoreUriInstance.ClientRsaSha256Crt, AwsIoTCoreUriInstance.ClientRsaKey, ""); //make sure to add a correct pfx certificate

//            //_awsMqttClient = new MqttConnectionClient("AwsIoTCoreUriInstance", "nanoFrameworkTestRunner", clientCert, nanoFramework.M2Mqtt.Messages.MqttQoSLevel.AtLeastOnce, caCert);
//        }

//        void Connect_to_the_mqtt_broker() 
//        {
//            //might have to take into account network config, time, and availability here!
//            _awsMqttClient.Open();
//        }

//        void Send_a_telemetry_message_to_the_broker() { }

//        void Receive_a_client_message_from_the_broker() { }

//        void Get_the_unnamed_shadow()
//        {
//            var shadow = _awsMqttClient.GetShadow(new CancellationTokenSource(15000).Token);
//            if (shadow != null)
//            {
//                Debug.WriteLine($"Get shadow result:");
//                //Debug.WriteLine($"Desired:  {shadow.state.desired.ToJson()}");
//                //Debug.WriteLine($"Reported:  {shadow.state.reported.ToJson()}");
//            }
//        }

//        void Update_the_unnamed_shadow() { }

//        void Delete_the_unnamed_shadow() { }

//        void Handle_the_delta_unnamed_shadow() { }

//        void Cleanup() 
//        {
//            _awsMqttClient.Close();
//        }
//    }
//}

