using Nancy;
using Nancy.Conventions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SquidHome {
	public class CustomBootstrapper : DefaultNancyBootstrapper {
		protected override void ConfigureConventions(NancyConventions nancyConventions) {
			nancyConventions.StaticContentsConventions.Add(StaticContentConventionBuilder.AddDirectory("/", "Content"));

			base.ConfigureConventions(nancyConventions);

			//nancyConventions.StaticContentsConventions.Add(StaticContentConventionBuilder.AddDirectory("/", "main"));
		}
	}
}
