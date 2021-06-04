using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace SqdHome {
	public class HomeDevice {
		DevProperty[] Properties;

		public bool ShouldSendUpdateRequest {
			get; set;
		}

		public DateTime LastUpdate {
			get; private set;
		}

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
			get; protected set;
		}

		public bool IsRoller {
			get; protected set;
		}

		public HomeDevice(string ID, string Name) {
			this.ID = ID;
			this.Name = Name;

			CanToggle = false;
			IsRoller = false;

			Properties = DevProperty.GetAllDeviceProperties(this);
			ShouldSendUpdateRequest = true;
		}

		public virtual void Toggle(bool On) {
			//SmartHome.BroadcastChange(this, "Toggle");
		}

		public virtual void ForceUpdate() {
			MQTT.Publish(string.Format("shellies/{0}/command", ID), "update");
		}

		public void LocalUpdate(out bool ShouldRemove) {
			ShouldRemove = false;

			if ((DateTime.Now - LastUpdate).TotalSeconds > 120) {
				ShouldRemove = true;
				return;
			}
		}

		public virtual void ReceiveUpdateProperty(string Name, string StrValue) {
			LastUpdate = DateTime.Now;

			for (int i = 0; i < Properties.Length; i++) {
				if (Properties[i].PropertyName == Name) {
					object Value = Utils.ParseType(Properties[i].PropertyType, StrValue);

					if (Properties[i].Get() != Value) {
						Properties[i].Set(Value);

						SmartHome.BroadcastChange(this, Name);
					}
				}
			}
		}
	}

	public class HomeDeviceRelay : HomeDevice {
		public override object Value => RelayValue == "on";

		[DeviceProperty(Name = "relay/0")]
		public string RelayValue {
			get; set;
		}

		[DeviceProperty(Name = "relay/input")]
		public string RelayInput {
			get; set;
		}

		public HomeDeviceRelay(string ID, string Name) : base(ID, Name) {
			CanToggle = true;
		}

		public override void Toggle(bool On) {
			MQTT.Publish(string.Format("shellies/{0}/relay/0/command", ID), On ? "on" : "off");

			if (Program.TEST) {
				RelayValue = On ? "on" : "off";
				SmartHome.BroadcastChange(this, "relay/0");
			}
		}
	}

	public class HomeDeviceRelay2 : HomeDevice {
		public override object Value {
			get {
				if (IsRoller) {
					return string.Format("{0} - {1} %", RollerState, RollerPosition);
				}

				return string.Format("{0}, {1}", RelayInput0, RelayInput1);
			}
		}

		[DeviceProperty(Name = "input/0")]
		public string RelayInput0 {
			get; set;
		}

		[DeviceProperty(Name = "input/1")]
		public string RelayInput1 {
			get; set;
		}

		[DeviceProperty(Name = "roller/0/pos")]
		public int RollerPosition {
			get; set;
		}

		[DeviceProperty(Name = "roller/0")]
		public string RollerState {
			get; set;
		}

		public HomeDeviceRelay2(string ID, string Name) : base(ID, Name) {
		}

		public void Calibrate() {
			IsRoller = true;
			MQTT.Publish(string.Format("shellies/{0}/roller/0/command", ID), "rc");
		}

		public void Open() {
			IsRoller = true;
			MQTT.Publish(string.Format("shellies/{0}/roller/0/command", ID), "open");

			if (Program.TEST) {
				RollerState = "open";
				SmartHome.BroadcastChange(this, "roller/0");
			}
		}

		public void Close() {
			IsRoller = true;
			MQTT.Publish(string.Format("shellies/{0}/roller/0/command", ID), "close");

			if (Program.TEST) {
				RollerState = "close";
				SmartHome.BroadcastChange(this, "roller/0");
			}
		}

		public void Stop() {
			IsRoller = true;
			MQTT.Publish(string.Format("shellies/{0}/roller/0/command", ID), "stop");

			if (Program.TEST) {
				RollerState = "stop";
				SmartHome.BroadcastChange(this, "roller/0");
			}

		}

		public void SetRollerPosition(int Pos) {
			IsRoller = true;

			if (Pos < 0)
				Pos = 0;

			if (Pos > 100)
				Pos = 100;

			MQTT.Publish(string.Format("shellies/{0}/roller/0/command/pos", ID), Pos.ToString());
		}

		public override void ReceiveUpdateProperty(string Name, string StrValue) {
			base.ReceiveUpdateProperty(Name, StrValue);

			if (Name.Contains("roller")) {
				IsRoller = true;
				CanToggle = false;
			}
		}
	}

	public class HomeDeviceDoor : HomeDevice {
		public override object Value {
			get {
				if (State == "open")
					return true;

				return false;
			}
		}

		[DeviceProperty(Name = "sensor/state")]
		public string State {
			get; set;
		}

		[DeviceProperty(Name = "sensor/tilt")]
		public int Tilt {
			get; set;
		}

		[DeviceProperty(Name = "sensor/vibration")]
		public bool Vibration {
			get; set;
		}

		[DeviceProperty(Name = "sensor/lux")]
		public float Lux {
			get; set;
		}

		[DeviceProperty(Name = "sensor/battery")]
		public float Battery {
			get; set;
		}

		[DeviceProperty(Name = "sensor/temperature")]
		public float Temperature {
			get; set;
		}

		public HomeDeviceDoor(string ID, string Name) : base(ID, Name) {
		}
	}
}
