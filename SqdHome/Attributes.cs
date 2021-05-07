using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace SqdHome {
	[AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
	sealed class DevicePropertyAttribute : Attribute {
		public string Name {
			get; set;
		}

		public DevicePropertyAttribute() {

		}
	}

	class DevProperty {
		HomeDevice Owner;
		PropertyInfo Prop;

		public string PropertyName {
			get; private set;
		}

		public void Set(string StrValue) {
			Prop.SetValue(Owner, Utils.ParseType(Prop.PropertyType, StrValue));
		}

		public static DevProperty[] GetAllDeviceProperties(HomeDevice Dev) {
			List<DevProperty> DevProps = new List<DevProperty>();

			IEnumerable<PropertyInfo> Props = Dev.GetType().GetProperties().Where(P => P.GetCustomAttribute<DevicePropertyAttribute>() != null);

			foreach (PropertyInfo Prop in Props) {
				DevProperty DevProp = new DevProperty() {
					Owner = Dev,
					Prop = Prop,
					PropertyName = Prop.GetCustomAttribute<DevicePropertyAttribute>().Name
				};
			}

			return DevProps.ToArray();
		}
	}
}
