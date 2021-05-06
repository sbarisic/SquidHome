using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqdHome {
	[AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
	sealed class DevicePropertyAttribute : Attribute {
		public string Name {
			get; set;
		}

		public DevicePropertyAttribute() {

		}
	}
}
