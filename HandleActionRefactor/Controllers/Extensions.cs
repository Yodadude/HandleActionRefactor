using System;
using System.Web.Mvc;

namespace HandleActionRefactor.Controllers
{
    public static class Extensions
    {

        public static HandleActionResultBuilder<T> OnSuccessWithMessage<T>(this HandleActionResultBuilder<T> builder,
                                                                           Func<ActionResult> action, string message)
        {

            return builder.OnSuccess(context =>
                                         {
                                             context.Controller.TempData.Add("message", message);
                                             return action();
                                         });

        }

        public static HandleActionResultBuilder<T, TRet> OnSuccessWithMessage<T, TRet>(this HandleActionResultBuilder<T, TRet> builder, 
                                                                                       Func<TRet, ActionResult> action, string message)
        {

            return builder.OnSuccess((model, context) => {
                                                             context.Controller.TempData.Add("message", message);
                                                             return action(model);
            });

        }


        public static HandleActionResultBuilder<T> OnErrorAjax<T>(this HandleActionResultBuilder<T> builder,
                                                                   Func<ActionResult> ajaxAction, Func<ActionResult> action)
        {

            return builder.OnError(context =>
            {
                if (context.HttpContext.Request.IsAjaxRequest() && action != null)
                {
                    return builder.OnError(ajaxAction);
                }

                return builder.OnError(action);
            });

        }
    }
}