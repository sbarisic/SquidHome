using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoordinateSharp;

namespace SqdHome {
	static class SunPosition {
		static Coordinate Coord;

		public static void Update() {
			Coord = new Coordinate(45.897413, 16.840135, DateTime.UtcNow);
		}

		public static DateTime GetDawn() {
			return Coord.CelestialInfo.AdditionalSolarTimes.CivilDawn.Value.ToLocalTime();
		}

		public static DateTime GetSunrise() {
			return Coord.CelestialInfo.SunRise.Value.ToLocalTime();
		}

		public static int SecondIntervalDawnToSunrise() {
			TimeSpan Interval = GetSunrise() - GetDawn();
			return (int)Interval.TotalSeconds;
		}

		static int CalcPercent(DateTime A, DateTime B, DateTime Now) {
			int Seconds = (int)(Now - A).TotalSeconds;
			int IntervalSec = (int)(B - A).TotalSeconds;
			return Seconds / IntervalSec;
		}

		public static bool IsDawnToSunrise(out int Percent) {
			DateTime A = GetDawn();
			DateTime B = GetSunrise();
			DateTime Now = DateTime.Now;
			Percent = 0;

			if (A < Now && B > Now) {
				Percent = CalcPercent(A, B, Now);
				return true;
			}

			return false;
		}

		public static DateTime GetSunset() {
			return Coord.CelestialInfo.SunSet.Value.ToLocalTime();
		}

		public static DateTime GetDusk() {
			return Coord.CelestialInfo.AdditionalSolarTimes.CivilDusk.Value.ToLocalTime();
		}

		public static int SecondIntervalSunsetToDusk() {
			TimeSpan Interval = GetDusk() - GetSunset();
			return (int)Interval.TotalSeconds;
		}

		public static bool IsSunsetToDusk(out int Percent) {
			DateTime A = GetSunset();
			DateTime B = GetDusk();
			DateTime Now = DateTime.Now;
			Percent = 0;

			if (A < Now && B > Now) {
				Percent = CalcPercent(A, B, Now);
				return true;
			}

			return false;
		}
	}
}
