
using System.Text;
using System.Text.Json;
using Microsoft.Azure.Devices;
using Microsoft.Extensions.Configuration;

namespace Services.AzureIotHub
{
    public class HostSender(IConfiguration configuration)
    {
        private readonly ServiceClient _serviceClient = ServiceClient.CreateFromConnectionString(configuration["AzureIotHub:Service"], TransportType.Amqp);

        /// <summary>
        /// Send msg to hub from host
        /// </summary>
        /// <param name="deviceId">device Id for identity</param>
        /// <param name="obj">json parameter message</param>
        /// <returns></returns>
        public async Task SendMessageAsync(string deviceId, object? obj)
        {
            var serializedMsg = JsonSerializer.Serialize(obj);
            var msg = CreateMessage(serializedMsg);
            await _serviceClient.SendAsync(deviceId, msg);
        }

        /// <summary>
        /// Invoke method that connected device has assign to hub
        /// </summary>
        /// <param name="deviceId">device Id for identity</param>
        /// <param name="methodName">method that connected device support</param>
        /// <param name="parameters">args json for method</param>
        /// <param name="responseTimeOut">time out for not response </param>
        /// <param name="connectionTimeout">timout for waiting device connecting the hub</param>
        /// <returns></returns>
        public async Task InvokeMethodAsync(string deviceId, string methodName, object? parameters, double responseTimeOut = 10.0, double connectionTimeout = 5.0)
        {
            var serializedMsg = JsonSerializer.Serialize(parameters);
            var directMethodInvoke = new CloudToDeviceMethod(methodName, 
                responseTimeout: TimeSpan.FromSeconds(responseTimeOut), 
                connectionTimeout: TimeSpan.FromSeconds(connectionTimeout));
            //* set message
            directMethodInvoke.SetPayloadJson(serializedMsg);
            try
            {
                await _serviceClient.InvokeDeviceMethodAsync(deviceId, directMethodInvoke);
            }
            catch (Microsoft.Azure.Devices.Common.Exceptions.DeviceNotFoundException)
            {
                throw;
            }
        }

        private Message CreateMessage(string msg)
        {
            var encodedMsg = Encoding.UTF8.GetBytes(msg);
            var azureIoTMsg = new Message(encodedMsg);
            return azureIoTMsg;
        }

        
    }
}
