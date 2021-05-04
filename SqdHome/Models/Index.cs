using System;
using System.Collections.Generic;

namespace SqdHome.Models {
	public class Devices {
		public List<HomeDevice> SmartDevices {
			get {
				return SmartHome.GetDevices();
			}
		}

		public Devices() {
		}
	}
}