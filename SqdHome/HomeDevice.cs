using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqdHome {
	public class HomeDevice {
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

		public HomeDevice(string ID, string Name, bool CanToggle) {
			this.ID = ID;
			this.Name = Name;
			this.CanToggle = CanToggle;
		}

		public virtual void Toggle(bool On) {
		}

		public virtual void ForceUpdate() {
			MQTT.Publish(string.Format("shellies/{0}/command", ID), "update");
		}

		public void ReceiveUpdateProperty(string Name, string Value) {
			LastUpdate = DateTime.Now;
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

		public HomeDeviceRelay(string ID, string Name) : base(ID, Name, true) {
		}

		public override void Toggle(bool On) {
			MQTT.Publish(string.Format("shellies/{0}/relay/command", ID), On ? "on" : "off");
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

		public HomeDeviceDoor(string ID, string Name) : base(ID, Name, false) {
		}
	}
}
