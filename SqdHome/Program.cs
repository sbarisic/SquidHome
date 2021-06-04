using System;
using System.Collections.Generic;
using System.Linq;
using Nancy;
using Nancy.Conventions;
using Nancy.Hosting.Self;
using SqdHome.Models;
using System.Windows.Forms;
using System.Threading;

namespace SqdHome {
	public static class Program {
		public static bool DEBUG_NAMES = false;
		public static bool TEST = false;

		[STAThread]
		static void Main(string[] args) {
			//Test();
			//return;

			Tasks.Init();
			SmartHome.Init();
			ActionsParser.Init();
			MQTT.Init();

			NancyHost Host = new NancyHost(new Uri("http://localhost:8080"));
			Host.Start();

			//Application.EnableVisualStyles();
			//Application.Run(new MainForm());

			while (true) {
				Tasks.Tick();
				Thread.Sleep(250);
			}
		}

		static void Test() {
			SunPosition.Update();

			SunPosition.IsSunsetToDusk(out int Perc);
		}
	}

	public class MainModule : NancyModule {
		public MainModule() {
			Get("/", args => {
				return Response.AsRedirect("~/devices");
			});

			Get("/devices", args => {
				return new Devices();
			});

			Get("/editor", args => {
				return new Editor();
			});

			/*Post("/devices", args => {
				if (Request.Form.Action == null)
					return Response.AsRedirect("~/devices");

				string[] Action = ((string)Request.Form.Action).Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
				HomeDevice Device = SmartHome.GetDevice(Action[1]);

				if (Device == null)
					return Response.AsRedirect("~/devices");

				switch (Action[0]) {
					case "ToggleOn":
						Device.Toggle(true);
						break;

					case "ToggleOff":
						Device.Toggle(false);
						break;

					default:
						break;
				}

				return Response.AsRedirect("~/devices");
			});*/
		}
	}
}
