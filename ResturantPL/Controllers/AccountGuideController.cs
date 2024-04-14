using Microsoft.AspNetCore.Mvc;
using Service.UnitOfWork;

namespace ResturantPL.Controllers
{
    public class AccountGuideController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public AccountGuideController(IUnitOfWork unitOfWork)
        {
            this._unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}
