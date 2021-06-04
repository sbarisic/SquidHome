using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Threading;
using System.IO;

namespace SqdHome {
	class ActionEvent {
		public string MQTT;
		public string Value;

		public XmlNodeList Nodes;

		public ActionEvent(string MQTT, string Value) {
			this.MQTT = MQTT;
			this.Value = Value;
		}

		public bool Matches(string MQTT, string Value) {
			if (this.MQTT == MQTT && this.Value == Value)
				return true;

			return false;
		}
	}

	public static class ActionsParser {
		//static Dictionary<string, string> DeviceNames = new Dictionary<string, string>();
		static List<ActionEvent> Events = new List<ActionEvent>();

		public static void Init() {
			string[] ActionsFiles = Directory.GetFiles("actions", "*.xml", SearchOption.AllDirectories);

			foreach (string ActionFile in ActionsFiles) {
				XmlDocument Doc = LoadActions(ActionFile);
				XmlNode Actions = Doc.GetElementsByTagName("Actions")[0];

				foreach (XmlElement E in Actions)
					ParseAction(E);
			}
		}

		static XmlDocument LoadActions(string Name) {
			XmlDocument Doc = new XmlDocument();
			Doc.Load(Name);
			return Doc;
		}

		static void ParseAction(XmlElement E) {
			if (E.Name == "DeviceName") {
				string DeviceID = E.GetAttribute("ID");
				string IsRoller = E.GetAttribute("IsRoller");

				/*if (DeviceNames.ContainsKey(DeviceID))
					DeviceNames.Remove(DeviceID);

				DeviceNames.Add(DeviceID, E.InnerText.Trim());*/

				HomeDevice Dev = SmartHome.GetOrCreateDevice(DeviceID, E.InnerText.Trim());

				if (!string.IsNullOrEmpty(IsRoller) && IsRoller == "true" && Dev is HomeDeviceRelay2 Rel2) {
					Rel2.Stop();
				}
			}

			if (E.Name == "Event") {
				ActionEvent Event = new ActionEvent(E.GetAttribute("MQTT"), E.GetAttribute("Value"));
				Event.Nodes = E.ChildNodes;
				Events.Add(Event);
			}

			if (E.Name == "Toggle") {
				HomeDevice Dev = SmartHome.GetDeviceByName(E.GetAttribute("Name"));
				bool Value = E.GetAttribute("Value") == "On";

				if (Dev != null) {
					Dev.Toggle(Value);
				}
			}

			if (E.Name == "WaitSeconds") {
				int Value = int.Parse(E.GetAttribute("Value"));
				XmlNodeList Children = E.ChildNodes;

				Thread WaitThread = new Thread(() => {
					Thread.Sleep(Value * 1000);

					foreach (XmlElement Child in Children)
						ParseAction(Child);
				});
				WaitThread.Start();
			}
		}

		/*public static string GetDeviceName(string DeviceID) {
			if (DeviceNames.ContainsKey(DeviceID))
				return DeviceNames[DeviceID];

			return DeviceID;
		}*/

		public static void TriggerEvent(string MQTT, string Value) {
			foreach (ActionEvent E in Events) {
				if (E.Matches(MQTT, Value)) {
					foreach (XmlElement Element in E.Nodes)
						ParseAction(Element);
				}
			}
		}
	}
}
