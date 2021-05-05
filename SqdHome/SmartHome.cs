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
			// TODO: Initialize devices here
			for (int i = 0; i < 4; i++) {
				HomeDeviceRelay LightDevice = new HomeDeviceRelay(Guid.NewGuid().ToString(), "Light " + i.ToString());
				LightDevice.Toggle(false);

				Devices.Add(LightDevice);
			}

			Devices.Add(new HomeDeviceThermostat());
			Devices.Add(new HomeDeviceDoor());

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

	public class HomeDevice {
		public string ID {
			get;
		}

		public string Name {
			get;
		}

		public virtual object Value {
			get;
		}

		public bool CanToggle {
			get;
		}

		public HomeDevice(string ID, string Name, bool CanToggle) {
			this.ID = ID;
			this.Name = Name;
			this.CanToggle = CanToggle;
		}

		public virtual void Toggle(bool On) {
		}
	}

	public class HomeDeviceRelay : HomeDevice {
		public bool RelayValue {
			get; private set;
		}

		public override object Value => RelayValue;

		public HomeDeviceRelay(string ID, string Name) : base(ID, Name, true) {
		}

		public override void Toggle(bool On) {
			RelayValue = On;
		}
	}

	public class HomeDeviceThermostat : HomeDevice {
		public override object Value => "23 °C";

		public HomeDeviceThermostat() : base(Guid.NewGuid().ToString(), "Thermostat", false) {
		}
	}

	public class HomeDeviceDoor : HomeDevice {
		public override object Value => false;

		public HomeDeviceDoor() : base(Guid.NewGuid().ToString(), "Door", false) {
		}
	}
}
