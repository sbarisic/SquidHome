using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using System.Text.Json;
using Newtonsoft.Json;

namespace SqdHome {
	public static class JSON {
		public static string Serialize(object Obj) {
			return JsonConvert.SerializeObject(Obj);

			//return JsonSerializer.Serialize(Obj);
		}

		public static T Deserialize<T>(string JSON) {
			//return JsonSerializer.Deserialize<T>(JSON);

			return JsonConvert.DeserializeObject<T>(JSON);
		}
	}
}
