using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
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
        public async Task<IActionResult> Create(Restaurant.Data.Models.Table table)
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
