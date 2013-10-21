using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HandleActionRefactor
{

    public interface IActionHandler<T>
    {
        ActionResult Returning(T input);
        ActionResult On(Func<T> func);
        ActionResult OnSuccess(Func<T> func);
        ActionResult OnError(Func<T> func);
    }

    //public class ActionHandler<T> : IActionHandler<T>
    //{
    //    private T _model;
    //    private ActionResult result;

    //    public ActionHandler(T model)
    //    {
    //        _model = model;
    //        result = new ContentResult();
    //    }

    //    public ActionResult Returning(T input)
    //    {
    //        return result;
    //    }

    //    public ActionResult On(Func<T> func)
    //    {
    //        return func(_model) as ActionResult;
    //    }

    //    public ActionResult OnSuccess(Func<T> func)
    //    {
    //        return func(_model) as ActionResult;
    //    }

    //    public ActionResult OnError(Func<T> func)
    //    {
    //        return func(_model) as ActionResult;
    //    }
    //}
}