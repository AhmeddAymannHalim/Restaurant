using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Restaurant.Data.Models;
using Service.UnitOfWork;
using ResturantPL.ViewModels;
using ResturantPL.Helper;

namespace ResturantPL.Controllers
{
    public class ScroungeController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public ScroungeController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ActionResult> Index()
        {
            var scrounges = await _unitOfWork.ScroungeRepository.GetAllAsync();

            return View(scrounges);
        }

        // for datatable - ajax js
        public IActionResult GetData()
        {
            // for datatable - ajax js
            var scrounges = _unitOfWork.ScroungeRepository.GetAll(Includeword: "Employee, State");
            return Json(new { data = scrounges });
        }

        [HttpGet]
        public IActionResult Create()
        {
            ScroungeViewModel scroungeViewModel = new ScroungeViewModel()
            {
                Scrounge = new Scrounge(),
                EmployeeList = _unitOfWork.EmployeeRepository.GetAll().Select(x => new SelectListItem
                {
                    Text = x.FirstName,
                    Value = x.EmployeeId.ToString()
                }),
                StateList = _unitOfWork.StateRepository.GetAll().Select(x => new SelectListItem
                {
                    Text = x.StateName,
                    Value = x.StateId.ToString()
                })
            };
            return View(scroungeViewModel);
        }


        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ScroungeViewModel scroungeViewModel)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // _context.Scrounges.Add(scrounge);
                    await _unitOfWork.ScroungeRepository.AddAsync(scroungeViewModel.Scrounge);

                    // _context.SaveChanges();
                    _unitOfWork.Complete();

                    return RedirectToAction("Index");
                }
                return View(scroungeViewModel.Scrounge);
            }
            catch (Exception ex)
            {
                return View("Error", ErrorMessage.Error(ex.Message));
            }
        }



        [HttpGet]
        public IActionResult Edit(int? id)
        {
            if (id == null || id == 0)
            {
                NotFound();
            }

            ScroungeViewModel scroungeViewModel = new ScroungeViewModel()
            {

                Scrounge = _unitOfWork.ScroungeRepository.GetFirstorDefault(x => x.Id == id),
                EmployeeList = _unitOfWork.EmployeeRepository.GetAll().Select(x => new SelectListItem
                {
                    Text = x.FirstName,
                    Value = x.EmployeeId.ToString()
                }),
                StateList = _unitOfWork.StateRepository.GetAll().Select(x => new SelectListItem
                {
                    Text = x.StateName,
                    Value = x.StateId.ToString()
                })
            };
            return View(scroungeViewModel);

        }



        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Edit(ScroungeViewModel scroungeViewModel)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    //_context.Scrounges.Update(scrounge);
                    _unitOfWork.ScroungeRepository.Update(scroungeViewModel.Scrounge);

                    // _context.SaveChanges();
                    _unitOfWork.Complete();

                    return RedirectToAction("Index");
                }
                return View(scroungeViewModel.Scrounge);
            }
            catch (Exception ex)
            {
                return View("Error", ErrorMessage.Error(ex.Message));
            }
        }

        ////[HttpGet]
        ////public async Task<IActionResult> Delete(int id)
        ////{
        ////    if (id == null || id == 0)
        ////    {
        ////        NotFound();
        ////    }

        ////    //var scroungeIndb = _context.Scrounges.Find(id);  
        ////    var scroungeIndb = await _unitOfWork.ScroungeRepository.GetByIdAsync(id);

        ////    return View(scroungeIndb);
        ////}

        //[HttpDelete]
        ////[HttpPost]
        ////[ValidateAntiForgeryToken]
        //public IActionResult Delete(int? id) 
        //{
        //    //var scroungeIndb = _context.Scrounges.Find(id);
        //    var scroungeIndb = _unitOfWork.ScroungeRepository.GetFirstorDefault(x => x.Id == id);

        //    if (scroungeIndb == null)  
        //    {
        //        //NotFound();
        //        return Json(new { success = false, message = "Error While Deleting" });
        //    }

        //    //_context.Scrounges.Remove(scroungeIndb);
        //    _unitOfWork.ScroungeRepository.Delete(scroungeIndb);

        //    //_context.SaveChanges();
        //    _unitOfWork.Complete();

        //    //return RedirectToAction("Index");
        //    return Json(new { success = true, message = "file has been Deleted" }); 
        //}




        ///////////////////////////////////////////////////////////////////////////////////////
        ///

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            if (id == 0)
            {
                NotFound();
            }

            var scroungeIndb = await _unitOfWork.ScroungeRepository.GetByIdAsync(id);

            return View(scroungeIndb);
        }


        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult DeleteScrounge(int? id)
        {
            try
            {
                var scroungeIndb = _unitOfWork.ScroungeRepository.GetFirstorDefault(x => x.Id == id);

                if (scroungeIndb == null)
                {
                    NotFound();
                }

                _unitOfWork.ScroungeRepository.Delete(scroungeIndb);

                _unitOfWork.Complete();

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                return View("Error", ErrorMessage.Error(ex.Message));
            }
        }



        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            if (id == 0)
            {
                NotFound();
            }

            var scroungeIndb = await _unitOfWork.ScroungeRepository.GetByIdAsync(id);

            return View(scroungeIndb);
        }
    }
}
