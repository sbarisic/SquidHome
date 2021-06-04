using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace SqdHome {
	static class Logging {
		public static void Init() {
			if (!Directory.Exists("logs"))
				Directory.CreateDirectory("logs");
		}

		public static void WriteLine(string Text) {
			DateTime Now = DateTime.Now;
			string DateFormat = Now.ToString("ddMMyyyy");
			string TimeFormat = Now.ToString("HH:mm:ss.fff");

			File.AppendAllText(string.Format("logs/log_{0}.txt", DateFormat), string.Format("[{0}] {1}\r\n", TimeFormat, Text));

		}

		public static void WriteLine(string Fmt, params object[] Args) {
			WriteLine(string.Format(Fmt, Args));
		}
	}
}
