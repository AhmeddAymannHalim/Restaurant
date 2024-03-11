using Microsoft.AspNetCore.Mvc;
using Restaurant.Data.Models;
using ResturantPL.Helper;
using Service.UnitOfWork;

namespace ResturantPL.Controllers
{
    public class TableController : Controller
    {

        private readonly IUnitOfWork _unitOfWork;
        public TableController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ActionResult> Index()
        {
            var tables = await _unitOfWork.TableRepository.GetAllAsync();

            return View(tables);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Table table)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    await _unitOfWork.TableRepository.AddAsync(table);
                    _unitOfWork.Complete();

                    return RedirectToAction("Index");
                }
                return View(table);
            }
            catch (Exception ex)
            {
                return View("Error", ErrorMessage.Error(ex.Message));
            }
        }



        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var table = await _unitOfWork.TableRepository.GetByIdAsync(id);
            if (table != null)
            {
                return View(table);
            }
            return NotFound();
        }



        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Edit(Table table)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _unitOfWork.TableRepository.Update(table);

                    _unitOfWork.Complete();

                    return RedirectToAction("Index");
                }
                return View(table);
            }
            catch (Exception ex)
            {
                return View("Error", ErrorMessage.Error(ex.Message));
            }
        }



        public IActionResult Delete(int id)
        {
            var table = _unitOfWork.TableRepository.GetById(id);
            if (table != null)
            {
                try
                {
                    _unitOfWork.TableRepository.Delete(table);
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
