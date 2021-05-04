using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqdHome {
	static class SmartHome {
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
		bool RelayValue;

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
