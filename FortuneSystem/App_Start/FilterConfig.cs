using FortuneSystem.Controllers;
using System.Web;
using System.Web.Mvc;

namespace FortuneSystem
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            //filters.Add(new SessionExpireFilterAttribute());
            filters.Add(new ErrorHandler.AiHandleErrorAttribute());
        }
    }
}
