using System;
using System.Web.Mvc;
using SchoStack.Web;
using System.Collections.Generic;

namespace HandleActionRefactor.Controllers
{
    public class BaseController : Controller
    {
        public IInvoker Invoker { get; set; }

        public HandleActionResult<T> Handle<T>(T model)
        {
            return new HandleActionResult<T>(model, Invoker);
        }
    }

    public class HandleActionResult<T> : ActionResult
    {
        private readonly T _model;
        private readonly IInvoker _invoker;

        public HandleActionResult(T model, IInvoker invoker)
        {
            _model = model;
            _invoker = invoker;
        }

        public HandleActionResult<T, TRet> Returning<TRet>()
        {
            return new HandleActionResult<T, TRet>(_model, _invoker);
        }

        public override void ExecuteResult(ControllerContext context)
        {

        }
    }

    public class HandleActionResult<T, TRet> : ActionResult
    {

        private readonly T _model;
        private IInvoker _invoker;
        private Func<T, ActionResult> _success;
        private Func<T, ActionResult> _error;
        private Dictionary<Func<TRet, bool>, Func<T, ActionResult>> _actions; 

        public HandleActionResult(T model, IInvoker invoker)
        {
            _model = model;
            _invoker = invoker;
            _actions = new Dictionary<Func<TRet, bool>, Func<T, ActionResult>>();
        }

        public HandleActionResult<T, TRet> OnSuccess(Func<T, ActionResult> func)
        {
            _success = func;
            return this;
        }

        public HandleActionResult<T, TRet> OnError(Func<T, ActionResult> func)
        {
            _error = func;
            return this;
        }

        public HandleActionResult<T, TRet> On(Func<TRet, bool> func1, Func<T, ActionResult> func2)
        {
            _actions.Add(func1, func2);
            return this;
        }

        public override void ExecuteResult(ControllerContext context)
        {
            _invoker.Execute<TRet>(_model);

            //if (_error != null)
            //    _error(_model);
            //else if (_actions.Count > 0)
            //{
                
            //}
            //else if (_success != null)
            _success(_model).ExecuteResult(context);
        }
    }

}