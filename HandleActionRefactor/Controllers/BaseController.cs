using System;
using System.Web.Mvc;
using SchoStack.Web;
using System.Collections.Generic;
using System.Linq;

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

        public HandleActionResult<T, TRet> On(Func<TRet, bool> f1, Func<TRet, ActionResult> f2)
        {
            _actions.Add(new OnPredicate { On = f1, Do = f2 });
            return this;
        }

        new private void ExecuteResult(ControllerContext context)
        {
            _response = _invoker.Execute<TRet>(_model);

            if (!context.Controller.ViewData.ModelState.IsValid)
            {
                if (_error != null)
                    _error(_model).ExecuteResult(context);
            }
            else
            {
                var action = _actions.Single(x => x.On(_response));

                if (action != null)
                {
                    action.Do(_response);
                    return;
                }

                if (_success != null)
                    _success(_model).ExecuteResult(context);
            }

        }

        private class OnPredicate
        {
            public Func<TRet, bool> On;
            public Func<TRet, ActionResult> Do;
        }
    }

}