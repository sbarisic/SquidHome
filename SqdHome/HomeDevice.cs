﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace SqdHome {
	public class HomeDevice {
		DevProperty[] Properties;

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
			get;
		}

		public bool IsRoller {
			get;
		}

		public HomeDevice(string ID, string Name, bool CanToggle, bool IsRoller) {
			this.ID = ID;
			this.Name = Name;
			this.CanToggle = CanToggle;
			this.IsRoller = IsRoller;

			Properties = DevProperty.GetAllDeviceProperties(this);
		}

		public virtual void Toggle(bool On) {
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

		public void ReceiveUpdateProperty(string Name, string StrValue) {
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

		public HomeDeviceRelay(string ID, string Name) : base(ID, Name, true, false) {
		}

		public override void Toggle(bool On) {
			MQTT.Publish(string.Format("shellies/{0}/relay/0/command", ID), On ? "on" : "off");

			if (Program.TEST) {
				RelayValue = On ? "on" : "off";
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

		public HomeDeviceRelay2(string ID, string Name) : base(ID, Name, false, true) {
		}

		public void Calibrate() {
			MQTT.Publish(string.Format("shellies/{0}/roller/0/command", ID), "rc");
		}

		public void Open() {
			MQTT.Publish(string.Format("shellies/{0}/roller/0/command", ID), "open");
		}

		public void Close() {
			MQTT.Publish(string.Format("shellies/{0}/roller/0/command", ID), "close");
		}

		public void Stop() {
			MQTT.Publish(string.Format("shellies/{0}/roller/0/command", ID), "stop");

		}

		public void SetRollerPosition(int Pos) {
			if (Pos < 0)
				Pos = 0;

			if (Pos > 100)
				Pos = 100;

			MQTT.Publish(string.Format("shellies/{0}/roller/0/command/pos", ID), Pos.ToString());
		}
	}

	public class HomeDeviceDoor : HomeDevice {
		public override object Value => false;

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

		public HomeDeviceDoor(string ID, string Name) : base(ID, Name, false, false) {
		}
	}
}
