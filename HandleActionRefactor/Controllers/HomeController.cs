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

        //[HttpPost]
        //public ActionResult Index(HomeInputModel inputModel)
        //{

        //    return Handle(inputModel)
        //        .Returning<HomeResponseModel>()
        //        .On(x => x.GotoAbout, _ => RedirectToAction("About"))
        //        .OnSuccess(_ => RedirectToAction("Index"))
        //        .OnError(_ => Index());
        //}


        //[HttpPost]
        //public ActionResult Index(HomeInputModel inputModel)
        //{

        //    //if (!ModelState.IsValid)
        //    //    return Index();

        //    //Invoker.Execute(inputModel);

        //    //return RedirectToAction("ABout");

        //    return Handle(inputModel)
        //        .OnError(() => Index())
        //        .OnSuccess(() => RedirectToAction("About"));

        //}

        [HttpPost]
        public ActionResult Index(HomeInputModel inputModel)
        {

            return Handle(inputModel)
                //.OnError(() => Index())
                .OnErrorAjax(() => { return Json( new {status= "error"});}, () => Index())
                //.OnSuccessWithMessage(() => RedirectToAction("About"), "Tigers 2014")
                .Returning<HomeResponseModel>()
                .OnSuccessWithMessage(x => RedirectToAction("About"), "Tigers 2014")
                //.OnSuccess(() => RedirectToAction("Index"))
                ;
        }



        public ActionResult About()
        {
            return View();
        }
    }
}
