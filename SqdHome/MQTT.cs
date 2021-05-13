using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MQTTnet;
using MQTTnet.Server;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using MQTTnet.Client.Connecting;
using MQTTnet.Client.Receiving;
using MQTTnet.Protocol;
using System.Threading;

namespace SqdHome {
	public static class MQTT {
		static IMqttServer Server;
		static IMqttClient Client;

		public static void Init() {
			Server = new MqttFactory().CreateMqttServer();

			IMqttServerOptions SrvOpt = new MqttServerOptionsBuilder().WithDefaultEndpointPort(1883).Build();
			Server.StartAsync(SrvOpt);

			Client = new MqttFactory().CreateMqttClient();
			Client.UseApplicationMessageReceivedHandler(OnMessageReceived);
			Client.UseConnectedHandler(OnConnected);

			IMqttClientOptions CliOpt = new MqttClientOptionsBuilder().WithClientId("SquidHome").WithTcpServer("127.0.0.1", 1883).WithCleanSession().Build();
			Client.ConnectAsync(CliOpt, CancellationToken.None);
		}

		static void Subscribe(string Topic) {
			Task.Run(async () => {
				await Client.SubscribeAsync(Topic);
			});
		}

		public static void Publish(string Topic, string Payload, MqttQualityOfServiceLevel QoS = MqttQualityOfServiceLevel.ExactlyOnce, bool Retain = false) {
			Task.Run(async () => {
				await Client.PublishAsync(Topic, Payload, QoS, Retain);
			});
		}

		static void OnConnected(MqttClientConnectedEventArgs E) {
			Console.WriteLine("MQTT Connected");
			Subscribe("#");
			Test.SimulateMQTT();
		}

		static void OnMessageReceived(MqttApplicationMessageReceivedEventArgs E) {
			string Topic = E.ApplicationMessage.Topic;
			string Payload = E.ApplicationMessage.ConvertPayloadToString();

			if (Topic.StartsWith("shellies/"))
				ParseShelly(Topic, Payload);

			Console.WriteLine("{0}: {1}", Topic, Payload);
		}

		static void ParseShelly(string Topic, string Payload) {
			string[] Top = Topic.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
			string ID = Top[1];
			string SubTopic = "";

			if (ID == "announce")
				return;

			if (Topic.Count(C => C == '/') >= 2)
				SubTopic = Topic.Substring(Topic.IndexOf('/', Topic.IndexOf('/') + 1) + 1);

			HomeDevice Dev = SmartHome.GetOrCreateDevice(ID);

			if (Dev != null)
				Dev.ReceiveUpdateProperty(SubTopic, Payload);
		}
	}
}
