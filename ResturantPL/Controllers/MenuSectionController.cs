using Microsoft.AspNetCore.Mvc;
using Restaurant.Data.Models;
using Service.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using ResturantPL.Helper;

namespace ResturantPL.Controllers
{
    public class MenuSectionController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public MenuSectionController(IUnitOfWork unitOfWork)
        {
            this._unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            return View(_unitOfWork.MenuSectionRepository.GetAll(a => a.Include(a => a.MenuItems).ThenInclude(a => a.Store)));
        }


        public IActionResult Details(int id)
        {
            MenuSection section = _unitOfWork.MenuSectionRepository.GetById(a => a.Id == id, new string[] { "MenuItems" }, new string[] { "MenuItems.Store" });
            if (section != null)
            {
                return View(section);
            }
            return NotFound();
        }


        public IActionResult Create()
        {
            return View();
        }


        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MenuSection model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    await _unitOfWork.MenuSectionRepository.AddAsync(model);

                    return RedirectToAction("Index");
                }
                return View(model);
            }
            catch (Exception ex)
            {
                return View("Error", ErrorMessage.Error(ex.Message));
            }
        }


        public IActionResult Edit(int id)
        {
            MenuSection section = _unitOfWork.MenuSectionRepository
                .GetById(a => a.Id == id, new string[] { "MenuItems" }, new string[] { "MenuItems.Store" });
            if (section != null)
            {
                return View(section);
            }
            return NotFound();
        }


        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Edit(MenuSection model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _unitOfWork.MenuSectionRepository.Update(model);

                    return RedirectToAction("Index");
                }
                return View(model);
            }
            catch (Exception ex)
            {
                return View("Error", ErrorMessage.Error(ex.Message));
            }
        }


        public IActionResult Delete(int id)
        {
            var section = _unitOfWork.MenuSectionRepository.GetById(id);
            if (section != null)
            {
                try
                {
                    _unitOfWork.MenuSectionRepository.Delete(section);
                    TempData["success"] = "تمت العملية بنجاح";
                }
                catch (Exception ex)
                {
                    TempData["faild"] = "Error";
                    return View("Error", ErrorMessage.Error(ex.Message));
                }
            }
            return RedirectToAction("Index");
        }

    }
}
