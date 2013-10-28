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
            ExecuteResult(context);
        }
    }

    public class HandleActionResult<T, TRet> : ActionResult
    {

        public TRet _response { get; set; }
        private readonly T _model;
        private IInvoker _invoker;
        private Func<T, ActionResult> _success;
        private Func<T, ActionResult> _error;
        private readonly List<OnPredicate> _actions; 

        public HandleActionResult(T model, IInvoker invoker)
        {
            _model = model;
            _invoker = invoker;
            _actions = new List<OnPredicate>();
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

        public HandleActionResult<T, TRet> On(Func<TRet, bool> f1, Func<T, ActionResult> f2)
        {
            _actions.Add(new OnPredicate { Func1 = f1, Func2 = f2 });
            return this;
        }

        public override void ExecuteResult(ControllerContext context)
        {
            _response = _invoker.Execute<TRet>(_model);

            if (!context.Controller.ViewData.ModelState.IsValid)
            {
                if (_error != null)
                    _error(_model).ExecuteResult(context);
            }
            else
            {
                if (_success != null)
                    _success(_model).ExecuteResult(context);

                if (_actions.Count > 0)
                {
                    foreach (var onPredicate in _actions)
                    {
                        if (onPredicate.Func1(_response))
                            onPredicate.Func2(_model).ExecuteResult(context);
                    }
                }
            }

        }

        private class OnPredicate
        {
            public Func<TRet, bool> Func1;
            public Func<T, ActionResult> Func2;
        }
    }

}