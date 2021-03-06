using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using SchoStack.Web;

namespace HandleActionRefactor.Controllers
{
    public class HandleActionResultBuilder<T>
    {
        private readonly T _model;
        private Func<ControllerContext, ActionResult> _error;
        private Func<ControllerContext, ActionResult> _success;
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

        public HandleActionResultBuilder<T> OnError(Func<ActionResult> errorFunc)
        {
            _error = _ => errorFunc();
            return this;
        }

        public HandleActionResultBuilder<T> OnError(Func<ControllerContext, ActionResult> errorFunc)
        {
            _error = errorFunc;
            return this;
        }

        public HandleActionResultBuilder<T> OnSuccess(Func<ActionResult> successFunc)
        {
            _success = _ => successFunc();
            return this;
        }

        public HandleActionResultBuilder<T> OnSuccess(Func<ControllerContext, ActionResult> successFunc)
        {
            _success = successFunc;
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
                        _builder._error(context).ExecuteResult(context);
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
        private readonly IInvoker _invoker;
        private Func<TRet, ControllerContext, ActionResult> _success;
        private Func<ControllerContext, ActionResult> _error;
        private readonly List<OnPredicate> _actions;

        public HandleActionResultBuilder(T model, IInvoker invoker, Func<ControllerContext, ActionResult> success, Func<ControllerContext, ActionResult> error)
        {

            _model = model;
            _invoker = invoker;
            _error = error;
            _success = (a, b) => success(b);
            _actions = new List<OnPredicate>();
        }

        public HandleActionResultBuilder<T, TRet> Returning(TRet response)
        {
            _response = response;
            return this;
        }

        public HandleActionResultBuilder<T, TRet> OnSuccess(Func<TRet, ActionResult> successFunc)
        {
            _success = (response, context) => successFunc(response);
            return this;
        }

        public HandleActionResultBuilder<T, TRet> OnSuccess(Func<TRet, ControllerContext, ActionResult> successFunc)
        {
            _success = successFunc;
            return this;
        }

        public HandleActionResultBuilder<T, TRet> OnError(Func<ActionResult> errorFunc)
        {
            _error = _ => errorFunc();
            return this;
        }

        public HandleActionResultBuilder<T, TRet> OnError(Func<ControllerContext, ActionResult> errorFunc)
        {
            _error = errorFunc;
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
                        _builder._error(context).ExecuteResult(context);
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
                        _builder._success(_builder._response, context).ExecuteResult(context);
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
}