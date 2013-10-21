using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HandleActionRefactor.Controllers
{
    public class HomeController : BaseController
    {
        public ActionResult Index()
        {
            var vm = new HomeViewModel();   
            return View(vm);
        }

        //[HttpPost]
        //public ActionResult Index(HomeInputModel inputModel)
        //{
        //    if (!ModelState.IsValid)
        //        return Index();

        //    var result = Invoker.Execute<HomeResponseModel>(inputModel);
        //    if (result.GotoAbout)
        //        return RedirectToAction("About");

        //    return RedirectToAction("Index");
        //}

        [HttpPost]
        public ActionResult Index(HomeInputModel inputModel)
        {
            return Handle(inputModel)
                .Returning<HomeResponseModel>()
                //.On(x => x.GotoAbout, _ => RedirectToAction("About"))
                .OnSuccess(_ => RedirectToAction("Index"));
            //.OnError(_ => Index());
        }

        public ActionResult About()
        {
            return View();
        }
    }
}
