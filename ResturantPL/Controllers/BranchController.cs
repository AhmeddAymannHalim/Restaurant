using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Restaurant.Data.Models;
using Service.UnitOfWork;
using ResturantPL.Helper;
using ResturantPL.ViewModels;

namespace ResturantPL.Controllers
{
    public class BranchController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public BranchController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            this._unitOfWork = unitOfWork;
            this._mapper = mapper;
        }
        public IActionResult Index()
        {
            return View(_unitOfWork.BrancheRepository.GetAll());
        }


        public IActionResult Details(int id)
        {
            Branche branche = GetBranch(id);
            if (branche != null)
            {
                BranchViewModel viewModel = _mapper.Map<BranchViewModel>(branche);
                foreach (var item in branche.Employees)
                {
                    viewModel.EmployeeList.Add(item);
                }
                return View(viewModel);
            }
            else
            {
                return NotFound();
            }
        }



        public IActionResult AddBranch()
        {
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> AddBranch(BranchViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    Branche branche = _mapper.Map<Branche>(model);
                    await _unitOfWork.BrancheRepository.AddAsync(branche);
                    return RedirectToAction("Index");
                }
                return View(model);
            }
            catch (Exception ex)
            {
                return View("Error", ErrorMessage.Error(ex.Message));
            }
        }


        public IActionResult UpdateBranch(int id)
        {
            try
            {
                Branche branche = GetBranch(id);
                if (branche != null)
                {
                    BranchViewModel viewModel = _mapper.Map<BranchViewModel>(branche);
                    return View(viewModel);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                return View("Error", ErrorMessage.Error(ex.Message));
            }
        }


        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult UpdateBranch(BranchViewModel model)
        {
            try
            {

                if (ModelState.IsValid)
                {
                    Branche branche = _mapper.Map<Branche>(model);
                    _unitOfWork.BrancheRepository.Update(branche);
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
            Branche branche = GetBranch(id);
            if (branche != null)
            {
                try
                {
                    _unitOfWork.BrancheRepository.Delete(branche);
                    TempData["success"] = "تمت العملية بنجاح";

                }
                catch (Exception)
                {
                    TempData["faild"] = "Error";
                    return View("Error", ErrorMessage.Error("لا يمكنك حذف الفرع لارتباطه بالموظفين"));
                }
            }
            return RedirectToAction("Index");
        }




        public Branche GetBranch(int id)
        {
            Branche branche = _unitOfWork.BrancheRepository.GetById(a => a.Id == id, new string[] { "Employees" });
            return branche;
        }


    }

}

