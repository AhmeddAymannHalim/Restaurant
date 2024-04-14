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
                    string nameWithoutSpaces = model.Name.Replace(" ", "");

                    bool nameExist = _unitOfWork.JobRepository.Any(t => t.Name.Replace(" ", "") == nameWithoutSpaces);
                    if (nameExist)
                    {
                        ModelState.AddModelError("Name", "هذا القسم موجود بالفعل من فضلك قم بتغيير الاسم");
                        return View(model);
                    }
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
            var obj = _unitOfWork.MenuSectionRepository.GetById(id);
            if (obj != null)
            {
                try
                {
                    _unitOfWork.MenuSectionRepository.Delete(obj);
                    return Json(new { success = true });
                }
                catch (Exception)
                {
                    return Json(new { success = false, errorMessage = "نأسف حدث خطأ اثناء الحذف , ربما يكون مرتبطا بشئ آخر " });
                }
            }
            return Json(new { success = true });
        }

    }
}
