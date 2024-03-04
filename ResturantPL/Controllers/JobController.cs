using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Restaurant.Data.Models;
using Service.UnitOfWork;
using ResturantPL.ViewModels;
using ResturantPL.Helper;

namespace ResturantPL.Controllers
{
    public class JobController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public JobController(IUnitOfWork unitOfWork, IMapper mapper, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            this._unitOfWork = unitOfWork;
            this._mapper = mapper;
            this._userManager = userManager;
            this._roleManager = roleManager;
        }

        public IActionResult Jobs()
        {
            var jobs = _unitOfWork.JobRepository.GetAll(a => a.Include(a => a.Employees));
            var roles = _roleManager.Roles;
            ViewBag.Roles = roles;

            return View(jobs);

        }


        public IActionResult DetailsJob(int id)
        {
            Job job = GetJob(id);
            if (job != null)
            {
                return View(job);
            }
            return NotFound();
        }


        [HttpGet]
        public IActionResult AddJob()
        {
            return View();

        }


        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> AddJob(Job model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    await _unitOfWork.JobRepository.AddAsync(model);

                    return RedirectToAction("Jobs");
                }
                return View(model);
            }
            catch (Exception ex)
            {
                return View("Error", ErrorMessage.Error(ex.Message));
            }
        }




        [HttpGet]
        public IActionResult UpdateJob(int id)
        {
            Job job = GetJob(id);

            if (job != null)
            {
                return View(job);
            }
            return NotFound();
        }



        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateJob(Job model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    Job job = GetJob(model.JobId);
                    if (job != null)
                    {
                        return NotFound();
                    }
                    await _unitOfWork.JobRepository.UpdateAsync(job);

                    return RedirectToAction("Jobs");
                }
                return View(model);
            }
            catch (Exception ex)
            {
                return View("Error", ErrorMessage.Error(ex.Message));
            }
        }



        public IActionResult DeleteJob(int id)
        {
            Job job = GetJob(id);
            if (job != null)
            {
                return View(job);
            }
            return NotFound();
        }


        [HttpPost, ValidateAntiForgeryToken, ActionName("DeleteJob")]
        public IActionResult ConfirmDeleteJob(int id)
        {
            try
            {
                Job job = GetJob(id);
                if (job != null)
                {
                    _unitOfWork.JobRepository.Delete(job);
                    return RedirectToAction("Jobs");
                }
                return BadRequest();
            }
            catch (Exception ex)
            {
                return View("Error", ErrorMessage.Error(ex.Message));
            }
        }

        public Job GetJob(int id)
        {
            Job job = _unitOfWork.JobRepository.GetById(a => a.JobId == id, new string[] { "Employees" }, new string[] { "Employees.SalaryType", "Employees.Branche" });
            return job;
        }


        public async Task<IActionResult> AddRole(int id)
        {
            Job job = GetJob(id);
            if (job != null)
            {
                IdentityRole role = new IdentityRole()
                {
                    Name = job.Name,
                };
                await _roleManager.CreateAsync(role);

                return RedirectToAction("Jobs");
            }
            return RedirectToAction("Jobs");
        }


        public async Task<IActionResult> RemoveRole(int id)
        {
            Job job = GetJob(id);
            if (job != null)
            {
                IdentityRole role = _roleManager.Roles.Where(a => a.Name == job.Name).FirstOrDefault();

                await _roleManager.DeleteAsync(role);

                return RedirectToAction("Jobs");
            }
            return RedirectToAction("Jobs");
        }

    }
}
