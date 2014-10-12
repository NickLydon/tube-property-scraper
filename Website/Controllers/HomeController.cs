using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Ajax;
using System.Threading.Tasks;

namespace Website.Controllers
{
	public class HomeController : Controller
	{
		public async Task<ActionResult> Index ()
		{
			var mvcName = typeof(Controller).Assembly.GetName ();
			var isMono = Type.GetType ("Mono.Runtime") != null;

			ViewData ["Version"] = mvcName.Version.Major + "." + mvcName.Version.Minor;
			ViewData ["Runtime"] = isMono ? "Mono" : ".NET";

			return View ();
		}
	}
}