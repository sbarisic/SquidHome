using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace SqdHome {
	public static class Test {

		static void SpawnThread(Action A) {
			Thread T = new Thread(() => {
				while (true)
					A();
			});
			T.IsBackground = true;
			T.Start();
		}

		static void SendMQTT_Shelly1(string ID, bool SW) {
			MQTT.Publish(string.Format("shellies/shelly1-{0}/input/0", ID), SW ? "on" : "off");
			Thread.Sleep(25);
			MQTT.Publish(string.Format("shellies/shelly1-{0}/relay/0", ID), SW ? "on" : "off");
		}


		static int Counter1;

		static void SendMQTT_Shelly25(string ID, bool SW) {
			Counter1++;
			bool Shelly25_Bool1 = true;

			if (Counter1 > 2) {
				Shelly25_Bool1 = false;

				if (Counter1 > 4) {
					Counter1 = 0;
				}
			}

			MQTT.Publish(string.Format("shellies/shellyswitch25-{0}/input/{1}", ID, Shelly25_Bool1 ? "1" : "0"), SW ? "on" : "off");
			Thread.Sleep(25);
			MQTT.Publish(string.Format("shellies/shellyswitch25-{0}/relay/{1}", ID, Shelly25_Bool1 ? "1" : "0"), SW ? "on" : "off");
		}

		static void SendMQTT_ShellyDW(string ID, bool State) {
			MQTT.Publish(string.Format("shellies/shellydw2-{0}/sensor/state", ID), State ? "open" : "closed");
		}

		public static void SimulateMQTT() {
			if (!Program.TEST)
				return;

			SendMQTT_Shelly1("1234", false);
			SendMQTT_Shelly25("7865", false);
			SendMQTT_ShellyDW("4512", false);

			/*SpawnThread(() => {
				Thread.Sleep(5000);
				SendMQTT_Shelly1("1234", true);

				Thread.Sleep(2000);
				SendMQTT_Shelly1("1234", false);
			});*/

			SpawnThread(() => {
				Thread.Sleep(4000);
				SendMQTT_ShellyDW("4512", true);

				Thread.Sleep(4000);
				SendMQTT_ShellyDW("4512", false);
			});

			/*SpawnThread(() => {
				Thread.Sleep(3500);
				SendMQTT_Shelly25("7865", true);

				Thread.Sleep(3500);
				SendMQTT_Shelly25("7865", false);
			});*/
		}
	}
}
