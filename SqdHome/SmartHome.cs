using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebSocketSharp;
using WebSocketSharp.Server;
using System.Threading;

namespace SqdHome {
	static class SmartHome {
		static WebSocketServer WSS;
		static List<HomeDevice> Devices = new List<HomeDevice>();

		public static void Init() {
			//Discovery.Init();
			MQTT.Init();

			StartWebSocketServer();

			Thread UpdateThread = new Thread(Update);
			UpdateThread.IsBackground = true;
			UpdateThread.Start();
		}

		static void Update() {
			while (true) {
				// TODO

				Thread.Sleep(500);
			}
		}

		static void StartWebSocketServer() {
			WSS = new WebSocketServer(8081);
			WSS.AddWebSocketService<SmartHomeWebsocket>("/ws");
			WSS.Start();
		}

		public static void RegisterDevice(HomeDevice Device) {
			Devices.Add(Device);

			if (WSS.IsListening) {
				WSS.WebSocketServices["/ws"].Sessions.Broadcast(
						JSON.Serialize(new {
							EventName = "ws_refresh_page"
						})
					);
			}
		}

		public static List<HomeDevice> GetDevices() {
			return Devices;
		}

		public static HomeDevice GetDevice(string ID) {
			for (int i = 0; i < Devices.Count; i++) {
				if (Devices[i].ID == ID)
					return Devices[i];
			}

			return null;
		}

		public static HomeDevice GetOrCreateDevice(string ID) {
			string[] ModelID = ID.Split(new[] { '-' });
			HomeDevice Device = GetDevice(ID);

			if (Device == null) {
				switch (ModelID[0]) {
					case "shelly1":
						Device = new HomeDeviceRelay(ID, ID);
						break;

					case "shellydw":
						Device = new HomeDeviceDoor(ID, ID);
						break;

					default:
						throw new NotImplementedException();
				}

				RegisterDevice(Device);
			}

			return Device;
		}

		public static void BroadcastChange(HomeDevice Dev) {
			WSS.WebSocketServices["/ws"].Sessions.Broadcast(
				JSON.Serialize(new {
					EventName = "ws_set_inner",
					ID = string.Format("sd-{0}", Dev.ID),
					ClassName = "id-value",
					Inner = Dev.Value.ToString()
				})
			);
		}
	}

	class SmartHomeWebsocket : WebSocketBehavior {
		protected override void OnMessage(MessageEventArgs e) {
			JSONMsgBase Msg = JSON.Deserialize<JSONMsgBase>(e.Data);

			switch (Msg.EventName) {
				case "Toggle":
					Msg = JSON.Deserialize<JSONMsgToggle>(e.Data);
					break;

				default:
					throw new NotImplementedException();
			}

			if (Msg is JSONMsgToggle ToggleMsg) {
				HomeDevice Dev = SmartHome.GetDevice(ToggleMsg.Args.ID);
				Dev.Toggle(ToggleMsg.Args.Value);

				SmartHome.BroadcastChange(Dev);
			} else
				throw new NotImplementedException();
		}
	}

	public class JSONMsgBase {
		public string EventName {
			get; set;
		}
	}

	public class JSONMsgToggle : JSONMsgBase {
		public class ArgsType {
			public string ID {
				get; set;
			}

			public bool Value {
				get; set;
			}
		}

		public ArgsType Args {
			get; set;
		}
	}
}
