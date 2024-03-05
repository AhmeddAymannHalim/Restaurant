using Microsoft.AspNetCore.Mvc;
using Restaurant.Data.Models;
using Service.UnitOfWork;

namespace ResturantPL.Controllers
{
    public class KitchenController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public KitchenController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            var kitchens = _unitOfWork.kitchenRepository.GetAll();
           

            return View(kitchens);
        }
        public IActionResult Details(int id)
        {
            var kitchen = _unitOfWork.kitchenRepository.GetById(id);
            return View(kitchen);
        }

        public IActionResult Create(int id)
        {
            var kitchen = _unitOfWork.kitchenRepository.GetById(id);
            if (kitchen is not  null) {
                return View(kitchen);
            }
            else
            {
                return View(new kitchen());
            }
           
        }
        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Create(kitchen model)
        {
            if (!ModelState.IsValid)
                return View(model);

            if (model.Id != 0)
                _unitOfWork.kitchenRepository.Update(model);

            _unitOfWork.kitchenRepository.AddAsync(model);
            TempData["success"] = "تمت العملية بنجاح";
            return RedirectToAction(nameof(Index));

        }
        public IActionResult Delete(int id)
        {
            var kitchen = _unitOfWork.kitchenRepository.Find(k=>k.Id==id);
            if (kitchen is not null)
            {
                try
                {
                    _unitOfWork.kitchenRepository.Delete(kitchen);
                    TempData["success"] = "تمت العملية بنجاح";
                  
                }
                catch (Exception ex)
                {
                    TempData["faild"] = "Error";
                }
                
            }                          
            return RedirectToAction(nameof(Index));
        }

    }
}
