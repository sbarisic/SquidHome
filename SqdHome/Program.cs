using System;
using System.Collections.Generic;
using System.Linq;
using Nancy;
using Nancy.Conventions;
using Nancy.Hosting.Self;
using SqdHome.Models;

namespace SqdHome {
	public static class Program {
		public static bool TEST = true;

		static void Main(string[] args) {
			SmartHome.Init();

			NancyHost Host = new NancyHost(new Uri("http://localhost:8080"));
			Host.Start();

			Console.ReadLine();
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
