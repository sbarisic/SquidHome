using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;

namespace SqdHome {
	public static class Utils {
		public static object ParseType(Type T, string StrValue) {
			if (T == typeof(string))
				return StrValue;

			if (T == typeof(int))
				return int.Parse(StrValue);

			if (T == typeof(float))
				return float.Parse(StrValue, CultureInfo.InvariantCulture);

			if (T == typeof(bool)) {
				if (StrValue.ToLower() == "true")
					return true;
				else if (StrValue.ToLower() == "false")
					return false;
				else
					throw new InvalidOperationException();
			}

			throw new NotImplementedException();
		}

		public static float Lerp(float A, float B, float Amt) {
			return A * (1 - Amt) + B * Amt;
		}
	}
}
