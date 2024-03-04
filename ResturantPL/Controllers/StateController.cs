using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Restaurant.Data.Models;
using Service.UnitOfWork;
using ResturantPL.ViewModels;
using ResturantPL.Helper;

namespace ResturantPL.Controllers
{
    public class StateController : Controller
    {
        //private readonly ApplicationDbContext _context;
        //public StateController(ApplicationDbContext context)
        //{
        //    _context = context;
        //}

        private IUnitOfWork _unitOfWork;

        IList<State> states;
        public StateController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;

            states = new List<State>()
            {
                new State() {StateId=1, StateName="حافز"},
                new State() {StateId = 2, StateName = "خصم"}

            };
        }

        public async Task<ActionResult> Index()
        {
            //var states = _context.States.ToList();
            var states = await _unitOfWork.StateRepository.GetAllAsync();
            return View(states);
        }

        //// for datatable - ajax js
        //public IActionResult GetData()
        //{
        //    // for datatable - ajax js
        //    var states = _unitOfWork.StateRepository.GetAll(Includeword: "Employee, State");
        //    return Json(new { data = states });
        //}

        [HttpGet]
        public IActionResult Create()
        {

            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(State state)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // _context.States.Add(state);
                    await _unitOfWork.StateRepository.AddAsync(state);

                    // _context.SaveChanges();
                    _unitOfWork.Complete();

                    return RedirectToAction("Index");
                }
                return View(state);
            }
            catch (Exception ex)
            {
                return View("Error", ErrorMessage.Error(ex.Message));
            }
        }


        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            if (id == 0)
            {
                NotFound();
            }
            var stateIndb = await _unitOfWork.StateRepository.GetByIdAsync(id);

            return View(stateIndb);

        }


        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Edit(State state)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    //_context.States.Update(state);
                    _unitOfWork.StateRepository.Update(state);

                    // _context.SaveChanges();
                    _unitOfWork.Complete();

                    return RedirectToAction("Index");
                }
                return View(state);
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

        ////    //var stateIndb = _context.States.Find(id);  
        ////    var stateIndb = await _unitOfWork.StateRepository.GetByIdAsync(id);

        ////    return View(stateIndb);
        ////}

        //[HttpDelete]
        ////[HttpPost]
        ////[ValidateAntiForgeryToken]
        //public IActionResult Delete(int? id) 
        //{
        //    //var stateIndb = _context.States.Find(id);
        //    var stateIndb = _unitOfWork.StateRepository.GetFirstorDefault(x => x.Id == id);

        //    if (stateIndb == null)  
        //    {
        //        //NotFound();
        //        return Json(new { success = false, message = "Error While Deleting" });
        //    }

        //    //_context.States.Remove(stateIndb);
        //    _unitOfWork.StateRepository.Delete(stateIndb);

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

            var stateIndb = await _unitOfWork.StateRepository.GetByIdAsync(id);

            return View(stateIndb);
        }

        [HttpPost, ValidateAntiForgeryToken]
        //public async Task<IActionResult> DeleteState(int id)   
        public IActionResult DeleteState(int? id)
        {
            //var stateIndb = await _unitOfWork.StateRepository.GetByIdAsync(id);
            var stateIndb = _unitOfWork.StateRepository.GetFirstorDefault(x => x.StateId == id);

            if (stateIndb == null)
            {
                NotFound();
            }

            _unitOfWork.StateRepository.Delete(stateIndb);

            _unitOfWork.Complete();

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            if (id == 0)
            {
                NotFound();
            }

            var stateIndb = await _unitOfWork.StateRepository.GetByIdAsync(id);

            return View(stateIndb);
        }
    }
}
