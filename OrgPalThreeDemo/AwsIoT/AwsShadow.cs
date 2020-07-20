// https://github.com/aws/aws-sdk-net/blob/master/sdk/src/Services/IotData/Generated/_mobile/AmazonIotDataClient.cs
// https://github.com/aws/aws-iot-device-sdk-embedded-C/tree/master/src
// https://docs.aws.amazon.com/iot/latest/developerguide/device-shadow-mqtt.html

using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using uPLibrary.Networking.M2Mqtt.Messages;

namespace AwsIoT
{

    /// <summary>
    /// Implementation for accessing AWS IoT Device Shadows
    /// <para>
    /// AWS IoT-Data enables secure, bi-directional communication between Internet-connected
    /// things (such as sensors, actuators, embedded devices, or smart appliances) and the
    /// AWS cloud. It enables you to retrieve, update, and delete thing shadows. A thing shadow
    /// is a persistent representation of your things and their state in the AWS cloud.
    /// </para>
    /// </summary>
    public class AwsShadow
    {

        public const string ShadowTopicPrefix = "$aws/things/";
        public const string shadowTopicPostFix = "/shadow";
        //public readonly MqttClient _client;

        public ManualResetEvent UpdateAvailable = new ManualResetEvent(false);
        public ManualResetEvent RejecteAvailable = new ManualResetEvent(false);

        public AwsShadow()
        {
            //AwsMqtt.Client.MqttMsgSubscribed += _client_MqttMsgSubscribed;
        }

        /// <summary>
        /// Deletes the thing shadow for the specified thing.
        /// <para>
        /// For more information, see <a href="http://docs.aws.amazon.com/iot/latest/developerguide/API_DeleteThingShadow.html">DeleteThingShadow</a>
        /// in the <i>AWS IoT Developer Guide</i>.
        /// </para>
        /// </summary>
        /// <param name="request">Container for the necessary parameters to execute the DeleteThingShadow service method.</param>
        /// <returns>The response from the DeleteThingShadow service method, as returned by IotData.</returns>
        /// <exception cref="Amazon.IotData.Model.InternalFailureException">
        /// An unexpected error has occurred.
        /// </exception>
        /// <exception cref="Amazon.IotData.Model.InvalidRequestException">
        /// The request is not valid.
        /// </exception>
        /// <exception cref="Amazon.IotData.Model.MethodNotAllowedException">
        /// The specified combination of HTTP verb and URI is not supported.
        /// </exception>
        /// <exception cref="Amazon.IotData.Model.ResourceNotFoundException">
        /// The specified resource does not exist.
        /// </exception>
        /// <exception cref="Amazon.IotData.Model.ServiceUnavailableException">
        /// The service is temporarily unavailable.
        /// </exception>
        /// <exception cref="Amazon.IotData.Model.ThrottlingException">
        /// The rate exceeds the limit.
        /// </exception>
        /// <exception cref="Amazon.IotData.Model.UnauthorizedException">
        /// You are not authorized to perform this operation.
        /// </exception>
        /// <exception cref="Amazon.IotData.Model.UnsupportedDocumentEncodingException">
        /// The document encoding is not supported.
        /// </exception>
        public void DeleteThingShadow(string namedShadow = "")
        {
            var topic = $"{ShadowTopicPrefix}{AwsMqtt.ThingName}{shadowTopicPostFix}/delete";
            if (namedShadow != string.Empty)
            {
                topic = $"{ShadowTopicPrefix}{AwsMqtt.ThingName}{shadowTopicPostFix}/name/{namedShadow}/update";
            }
            AwsMqtt.Client.Subscribe(new string[] { $"{topic}/accepted", $"{topic}/rejected" }, new byte[] { MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE });
            AwsMqtt.Client.Publish(topic, new byte[0], MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);
            AwsMqtt.Client.Unsubscribe(new string[] { $"{topic}/accepted", $"{topic}/rejected" });
        }

        /// <summary>
        /// Gets the thing shadow for the specified thing.
        /// <para>
        /// For more information, see <a href="http://docs.aws.amazon.com/iot/latest/developerguide/API_GetThingShadow.html">GetThingShadow</a>
        /// in the <i>AWS IoT Developer Guide</i>.
        /// </para>
        /// </summary>
        /// <param name="request">Container for the necessary parameters to execute the GetThingShadow service method.</param>
        /// <returns>The response from the GetThingShadow service method, as returned by IotData.</returns>
        /// <exception cref="Amazon.IotData.Model.InternalFailureException">
        /// An unexpected error has occurred.
        /// </exception>
        /// <exception cref="Amazon.IotData.Model.InvalidRequestException">
        /// The request is not valid.
        /// </exception>
        /// <exception cref="Amazon.IotData.Model.MethodNotAllowedException">
        /// The specified combination of HTTP verb and URI is not supported.
        /// </exception>
        /// <exception cref="Amazon.IotData.Model.ResourceNotFoundException">
        /// The specified resource does not exist.
        /// </exception>
        /// <exception cref="Amazon.IotData.Model.ServiceUnavailableException">
        /// The service is temporarily unavailable.
        /// </exception>
        /// <exception cref="Amazon.IotData.Model.ThrottlingException">
        /// The rate exceeds the limit.
        /// </exception>
        /// <exception cref="Amazon.IotData.Model.UnauthorizedException">
        /// You are not authorized to perform this operation.
        /// </exception>
        /// <exception cref="Amazon.IotData.Model.UnsupportedDocumentEncodingException">
        /// The document encoding is not supported.
        /// </exception>
        public string GetThingShadow(string namedShadow = "")
        {
            

            var topic = $"{ShadowTopicPrefix}{AwsMqtt.ThingName}{shadowTopicPostFix}/get";
            if (namedShadow != string.Empty)
            {
                topic = $"{ShadowTopicPrefix}{AwsMqtt.ThingName}{shadowTopicPostFix}/name/{namedShadow}/update";
            }
            AwsMqtt.Client.Subscribe(new string[] { $"{topic}/accepted", $"{topic}/rejected" }, new byte[] { MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE });
            AwsMqtt.Client.Publish(topic, new byte[0], MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);
            AwsMqtt.Client.Unsubscribe(new string[] { $"{topic}/accepted", $"{topic}/rejected"});
            return "";
        }

        ///// <summary>
        ///// Publishes state information.
        ///// <para>
        ///// For more information, see <a href="http://docs.aws.amazon.com/iot/latest/developerguide/protocols.html#http">HTTP
        ///// Protocol</a> in the <i>AWS IoT Developer Guide</i>.
        ///// </para>
        ///// </summary>
        ///// <param name="request">Container for the necessary parameters to execute the Publish service method.</param>
        ///// <returns>The response from the Publish service method, as returned by IotData.</returns>
        ///// <exception cref="Amazon.IotData.Model.InternalFailureException">
        ///// An unexpected error has occurred.
        ///// </exception>
        ///// <exception cref="Amazon.IotData.Model.InvalidRequestException">
        ///// The request is not valid.
        ///// </exception>
        ///// <exception cref="Amazon.IotData.Model.MethodNotAllowedException">
        ///// The specified combination of HTTP verb and URI is not supported.
        ///// </exception>
        ///// <exception cref="Amazon.IotData.Model.UnauthorizedException">
        ///// You are not authorized to perform this operation.
        ///// </exception>
        //public void PublishThingShadow(string shadow)
        //{
        //    throw new SystemException("Not Implemented");
        //}

        /// <summary>
        /// Updates the thing shadow for the specified thing.
        /// <para>
        /// For more information, see <a href="http://docs.aws.amazon.com/iot/latest/developerguide/API_UpdateThingShadow.html">UpdateThingShadow</a>
        /// in the <i>AWS IoT Developer Guide</i>.
        /// </para>
        /// </summary>
        /// <param name="request">Container for the necessary parameters to execute the UpdateThingShadow service method.</param>
        /// <returns>The response from the UpdateThingShadow service method, as returned by IotData.</returns>
        /// <exception cref="Amazon.IotData.Model.ConflictException">
        /// The specified version does not match the version of the document.
        /// </exception>
        /// <exception cref="Amazon.IotData.Model.InternalFailureException">
        /// An unexpected error has occurred.
        /// </exception>
        /// <exception cref="Amazon.IotData.Model.InvalidRequestException">
        /// The request is not valid.
        /// </exception>
        /// <exception cref="Amazon.IotData.Model.MethodNotAllowedException">
        /// The specified combination of HTTP verb and URI is not supported.
        /// </exception>
        /// <exception cref="Amazon.IotData.Model.RequestEntityTooLargeException">
        /// The payload exceeds the maximum size allowed.
        /// </exception>
        /// <exception cref="Amazon.IotData.Model.ServiceUnavailableException">
        /// The service is temporarily unavailable.
        /// </exception>
        /// <exception cref="Amazon.IotData.Model.ThrottlingException">
        /// The rate exceeds the limit.
        /// </exception>
        /// <exception cref="Amazon.IotData.Model.UnauthorizedException">
        /// You are not authorized to perform this operation.
        /// </exception>
        /// <exception cref="Amazon.IotData.Model.UnsupportedDocumentEncodingException">
        /// The document encoding is not supported.
        /// </exception>
        public void UpdateThingShadow(string shadowData, string namedShadow = "")
        {
            var topic = $"{ShadowTopicPrefix}{AwsMqtt.ThingName}{shadowTopicPostFix}/update";
            if (namedShadow != string.Empty)
            {
                topic = $"{ShadowTopicPrefix}{AwsMqtt.ThingName}{shadowTopicPostFix}/name/{namedShadow}/update";
            }
            const string shadowUpdateHeader = "{\"state\":{\"reported\":";
            const string shadowUpdateFooter = "}}";
            string shadowJson = $"{shadowUpdateHeader}{shadowData}{shadowUpdateFooter}";
            AwsMqtt.Client.Subscribe(
                new string[] { 
                $"{topic}/accepted", 
                $"{topic}/rejected", 
                $"{topic}/documents", 
                $"{topic}/delta" 
                }, 
                new byte[] { 
                    MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, 
                    MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, 
                    MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, 
                    MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE 
                });
            AwsMqtt.Client.Publish(topic, Encoding.UTF8.GetBytes(shadowJson), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);

            Debug.WriteLine($"Sent: {shadowJson} on topic: {topic}");

            //TODO: should we handle the message received event in this lib?
            AwsMqtt.Client.Unsubscribe(new string[] { $"{topic}/accepted", $"{topic}/rejected", $"{topic}/documents", $"{topic}/delta" });


        }

        //private void _client_MqttMsgSubscribed(object sender, MqttMsgSubscribedEventArgs e)
        //{
        //    throw new NotImplementedException();
        //}
    }
}
