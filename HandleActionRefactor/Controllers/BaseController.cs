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

        public HandleActionResultBuilder<T> Handle<T>(T model)
        {
            return new HandleActionResultBuilder<T>(model, Invoker);
        }
    }


    public class HandleActionResultBuilder<T>
    {
        private readonly T _model;
        private Func<ActionResult> _error;
        private Func<ActionResult> _success;

        private readonly IInvoker _invoker;

        public HandleActionResultBuilder(T model, IInvoker invoker)
        {
            _model = model;
            _invoker = invoker;
        }

        public HandleActionResultBuilder<T, TRet> Returning<TRet>()
        {
            return new HandleActionResultBuilder<T, TRet>(_model, _invoker, _success, _error);
        }

        public HandleActionResultBuilder<T> OnError(Func<ActionResult> func)
        {
            _error = func;
            return this;
        }

        public HandleActionResultBuilder<T> OnSuccess(Func<ActionResult> func)
        {
            _success = func;
            return this;
        }

        public static implicit operator HandleActionResult<T>(HandleActionResultBuilder<T> builder)
        {
            return new HandleActionResult<T>(builder);
        }

        public class HandleActionResult<T> : ActionResult
        {
            private HandleActionResultBuilder<T> _builder;

            public HandleActionResult(HandleActionResultBuilder<T> builder)
            {
                _builder = builder;
            }

            public override void ExecuteResult(ControllerContext context)
            {
                if (!context.Controller.ViewData.ModelState.IsValid)
                {
                    if (_builder._error != null)
                        _builder._error().ExecuteResult(context);
                }
                else
                {
                    if (_builder._success != null)
                    {
                        _builder._success(context).ExecuteResult(context);
                    }
                }
            }
        }
    }

    public class HandleActionResultBuilder<T, TRet>
    {
        private TRet _response { get; set; }
        protected readonly T _model;
        private IInvoker _invoker;
        private Func<TRet, ControllerContext, ActionResult> _success;
        private Func<ActionResult> _error;
        private readonly List<OnPredicate> _actions;

        public HandleActionResultBuilder(T model, IInvoker invoker, Func<ActionResult> success, Func<ActionResult> error)
        {

            _model = model;
            _invoker = invoker;
            _error = error;
            _success = (a,b) => success();
            _actions = new List<OnPredicate>();
        }

        public HandleActionResultBuilder<T, TRet> Returning(TRet response)
        {
            _response = response;
            return this;
        }

        public HandleActionResultBuilder<T, TRet> OnSuccess(Func<TRet, ActionResult> func)
        {
            _success = (a,b) => func(a);
            return this;
        }

        public HandleActionResultBuilder<T, TRet> OnSuccess(Func<TRet, ControllerContext, ActionResult> func)
        {
            _success = func;
            return this;
        }

        public HandleActionResultBuilder<T, TRet> OnError(Func<ActionResult> func)
        {
            _error = func;
            return this;
        }

        public HandleActionResultBuilder<T, TRet> On(Func<TRet, bool> condition, Func<TRet, ActionResult> action)
        {
            _actions.Add(new OnPredicate { On = condition, Do = action });
            return this;
        }

        public static implicit operator HandleActionResult<T, TRet>(HandleActionResultBuilder<T, TRet> builder)
        {
            return new HandleActionResult<T, TRet>(builder);
        }

        public class HandleActionResult<T, TRet> : ActionResult
        {
            private readonly HandleActionResultBuilder<T, TRet> _builder;

            public HandleActionResult(HandleActionResultBuilder<T, TRet> builder)
            {
                _builder = builder;
            }

            public override void ExecuteResult(ControllerContext context)
            {
                _builder._response = _builder._invoker.Execute<TRet>(_builder._model);

                if (!context.Controller.ViewData.ModelState.IsValid)
                {
                    if (_builder._error != null)
                        _builder._error().ExecuteResult(context);
                }
                else
                {
                    var action = _builder._actions.FirstOrDefault(x => x.On(_builder._response));

                    if (action != null)
                    {
                        action.Do(_builder._response).ExecuteResult(context);
                        return;
                    }

                    if (_builder._success != null)
                    {
                        _builder._success(_builder._response,context).ExecuteResult(context);
                    }
                }

            }
        }

        private class OnPredicate
        {
            public Func<TRet, bool> On;
            public Func<TRet, ActionResult> Do;
        }
    }

    public static class Extensions
    {
        public static HandleActionResultBuilder<T, TRet> OnSuccessWithMessage<T, TRet>(this HandleActionResultBuilder<T, TRet> builder, 
            Func<TRet, ActionResult> action, string message)
        {

            builder.OnSuccess((a,b) => { 
                b.Controller.TempData.Add("message", message);
                return action(a);
            });
            return null;
        }
    }
}