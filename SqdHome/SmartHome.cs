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
		static List<HomeDevice> RemoveList = new List<HomeDevice>();

		public static void Init() {
			StartWebSocketServer();

			Tasks.Register("local_update", LocalUpdate).ScheduleRepeat(2);
			Tasks.Register("global_update", GlobalUpdate).ScheduleRepeat(1);
		}

		static void LocalUpdate(Task This) {
			RemoveList.Clear();

			foreach (HomeDevice Dev in Devices) {
				Dev.LocalUpdate(out bool ShouldRemove);

				if (ShouldRemove)
					RemoveList.Add(Dev);
			}

			foreach (HomeDevice RemoveDev in RemoveList) {
				Devices.Remove(RemoveDev);
			}
		}

		static void GlobalUpdate(Task This) {
			foreach (var D in Devices) {
				if (D.ShouldSendUpdateRequest) {
					D.ShouldSendUpdateRequest = false;
					D.ForceUpdate();
				}
			}
		}

		static void StartWebSocketServer() {
			Console.WriteLine("Starting WSS");

			WSS = new WebSocketServer(8081);
			WSS.AddWebSocketService<SmartHomeWebsocket>("/ws");
			WSS.Start();
		}

		public static void RegisterDevice(HomeDevice Device) {
			Devices.Add(Device);

			if (WSS != null && WSS.IsListening) {
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

		public static HomeDevice GetDeviceByName(string Name) {
			for (int i = 0; i < Devices.Count; i++) {
				if (Devices[i].Name == Name)
					return Devices[i];
			}

			return null;
		}

		public static HomeDevice GetOrCreateDevice(string ID, string Name) {
			string[] ModelID = ID.Split(new[] { '-' });
			HomeDevice Device = GetDevice(ID);

			if (Device == null) {
				switch (ModelID[0]) {
					case "shelly1":
						Device = new HomeDeviceRelay(ID, Name);
						break;

					case "shellydw2":
						Device = new HomeDeviceDoor(ID, Name);
						break;

					case "shelly25":
					case "shellyswitch25":
						Device = new HomeDeviceRelay2(ID, Name);
						break;

					default:
						throw new NotImplementedException();
				}

				RegisterDevice(Device);
			}

			return Device;
		}

		public static void BroadcastChange(HomeDevice Dev, string Name) {
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
				case "Command":
					Msg = JSON.Deserialize<JSONMsgCommand>(e.Data);
					break;

				default:
					throw new NotImplementedException();
			}

			if (Msg is JSONMsgCommand Command) {
				HomeDevice Dev = SmartHome.GetDevice(Command.Args.ID);

				switch (Command.Args.Value) {
					case "ToggleOn":
						Dev.Toggle(true);
						break;

					case "ToggleOff":
						Dev.Toggle(false);
						break;

					case "Open":
						((HomeDeviceRelay2)Dev).Open();
						break;

					case "Stop":
						((HomeDeviceRelay2)Dev).Stop();
						break;

					case "Close":
						((HomeDeviceRelay2)Dev).Close();
						break;

					default:
						throw new NotImplementedException();
				}
			} else
				throw new NotImplementedException();
		}
	}

	public class JSONMsgBase {
		public string EventName {
			get; set;
		}
	}

	public class JSONMsgCommand : JSONMsgBase {
		public class ArgsType {
			public string ID {
				get; set;
			}

			public string Value {
				get; set;
			}
		}

		public ArgsType Args {
			get; set;
		}
	}
}
