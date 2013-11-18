using System.Web.Mvc;
using SchoStack.Web;

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
}