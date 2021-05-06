using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Makaretu.Dns;

namespace SqdHome {
	public static class Discovery {
		static MulticastService MDNS;
		static ServiceDiscovery Disc;

		public static void Init() {
			MDNS = new MulticastService();
			Disc = new ServiceDiscovery();

			Disc.ServiceDiscovered += ServiceDiscovered;
			MDNS.AnswerReceived += MDNS_AnswerReceived;

			MDNS.Start();
			Disc.QueryAllServices();
		}

		static void ServiceDiscovered(object Sender, DomainName E) {
			Console.WriteLine("mDNS: {0}", E.ToString());

			MDNS.SendQuery(E, type: DnsType.PTR);
		}

		static void MDNS_AnswerReceived(object sender, MessageEventArgs e) {
			// Is this an answer to a service instance details?
			var servers = e.Message.Answers.OfType<SRVRecord>();
			foreach (var server in servers) {
				Console.WriteLine($"host '{server.Target}' for '{server.Name}'");

				// Ask for the host IP addresses.
				MDNS.SendQuery(server.Target, type: DnsType.A);
				MDNS.SendQuery(server.Target, type: DnsType.AAAA);
			}

			// Is this an answer to host addresses?
			var addresses = e.Message.Answers.OfType<AddressRecord>();
			foreach (var address in addresses) {
				Console.WriteLine($"host '{address.Name}' at {address.Address}");
			}

		}
	}
}
