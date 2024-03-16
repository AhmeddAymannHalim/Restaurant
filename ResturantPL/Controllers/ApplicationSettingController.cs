using Microsoft.AspNetCore.Mvc;
using Restaurant.Data.Models;
using Service.UnitOfWork;
using System.Drawing.Printing;

namespace ResturantPL.Controllers
{
    public class ApplicationSettingController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public ApplicationSettingController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            return View();
        }


        // Application Setting
        public async Task<IActionResult> ApplicationSetting()
        {
            ApplicationSetting setting = await _unitOfWork.ApplicationSettingRepository.GetLast(a => a.Id);
            return View(setting);
        }

        [HttpPost]
        public IActionResult ApplicationSetting(ApplicationSetting setting)
        {
            if (ModelState.IsValid)
            {
                _unitOfWork.ApplicationSettingRepository.Update(setting);
                return RedirectToAction("ApplicationSetting");
            }
            return View(setting);
        }



        // Restaurant Settings
        public async Task<IActionResult> RestaurantSetting()
        {
            IndexSettings setting = await _unitOfWork.IndexRepository.GetLast(a => a.Id);
            return View(setting);
        }


        [HttpPost]
        public IActionResult RestaurantSetting(IndexSettings setting)
        {
            if (ModelState.IsValid)
            {
                _unitOfWork.IndexRepository.Update(setting);
                return RedirectToAction("RestaurantSetting");
            }
            return View(setting);
        }




        // Printer Settings

        public async Task<IActionResult> PrinterSetting()
        {
            PrinterSetting setting = await _unitOfWork.PrinterSettingRepository.GetLast(a => a.Id);
            ViewBag.InstalledPrinters = GetInstalledPrinters(); // Set the list of installed printers
            return View(setting);
        }


        [HttpPost]
        public IActionResult PrinterSetting(PrinterSetting setting)
        {
            ViewBag.InstalledPrinters = GetInstalledPrinters();
            if (ModelState.IsValid)
            {
                _unitOfWork.PrinterSettingRepository.Update(setting);
                return RedirectToAction("PrinterSetting");
            }
            return View(setting);
        }





        public List<string> GetInstalledPrinters()
        {
            return PrinterSettings.InstalledPrinters.Cast<string>().ToList();
        }


    }
}
