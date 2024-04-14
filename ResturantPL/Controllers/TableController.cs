using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Restaurant.Data.Models;
using ResturantPL.Helper;
using Service.UnitOfWork;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

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
        public async Task<IActionResult> Create(Restaurant.Data.Models.Table table)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    string nameWithoutSpaces = table.Name.Replace(" ", "");

                    bool tableExists = _unitOfWork.TableRepository.Any(t => t.Name.Replace(" ", "") == nameWithoutSpaces);

                    if (tableExists)
                    {
                        ModelState.AddModelError("Name", "هذا الاسم موجود بالفعل");
                        return View(table);
                    }


                    await _unitOfWork.TableRepository.AddAsync(table);
                    _unitOfWork.Complete();

                    return RedirectToAction("Create");
                }
                return View(table);
            }
            catch (Exception ex)
            {
                return View("Error", ErrorMessage.Error(ex.Message));
            }
        }

        public IActionResult AddRange()
        {
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> AddRange(int tableNumber, byte personNumber)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Get the last table from the repository
                    Restaurant.Data.Models.Table lastTable = await _unitOfWork.TableRepository.GetLast(a => a.Id);

                    int number = 1; 

                    if (lastTable != null)
                    {
                        int firstSpaceIndex = lastTable.Name.IndexOf(' ');
                        int secondSpaceIndex = lastTable.Name.IndexOf(' ', firstSpaceIndex + 1);
                        // Extract the substring after the space
                        string numberString = lastTable.Name.Substring(secondSpaceIndex + 1);

                        if (int.TryParse(numberString, out number))
                        {
                            number++;
                        }
                        else
                        {
                            return View("Error", ErrorMessage.Error("فشلت عملية اضافة الطاولات"));
                        }
                    }

                    for (int i = 0; i < tableNumber; i++)
                    {
                        Restaurant.Data.Models.Table newTable = new Restaurant.Data.Models.Table()
                        {
                            Name = $"طاولة رقم {number}",
                            NumOfPersons = personNumber
                        };

                        await _unitOfWork.TableRepository.AddAsync(newTable);

                        number++;
                    }

                    return RedirectToAction("Index");
                }

                return View();
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
        public IActionResult Edit(Restaurant.Data.Models.Table table)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    bool tableExists = _unitOfWork.TableRepository.Any(t => t.Name == table.Name);

                    if (tableExists)
                    {
                        ModelState.AddModelError("Name", "هذا الاسم موجود بالفعل");
                        return View(table);
                    }



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
            var obj = _unitOfWork.TableRepository.GetById(id);
            if (obj != null)
            {
                try
                {
                    _unitOfWork.TableRepository.Delete(obj);
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
