using Shared.Helpers;
using Microsoft.AspNetCore.Mvc;
using Restaurant.DAL.Models;
using ResturantPL.Helper;
using ResturantPL.ViewModels;
using Restaurant.DAL.UnitOfWork;
using System.Drawing.Printing;
using System.Xml.Linq;

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
            IndexSettingsViewModel model = new IndexSettingsViewModel()
            {
                Id = setting.Id,
                ArName = setting.ArName,
                EnName = setting.EnName,
                Description = setting.Description,
                Email = setting.Email,
                Phone = setting.Phone,
                StartDay = setting.StartDay,
                EndDay = setting.EndDay,
                StartHour = setting.StartHour,
                EndHour = setting.EndHour,
                VedioUrl = setting.VedioUrl,
                ImageUrl = setting.ImageUrl,
                Locaion = setting.Locaion
            };

            return View(model);
        }


        [HttpPost]
        public IActionResult RestaurantSetting(IndexSettingsViewModel setting)
        {
            if (ModelState.IsValid)
            {
                IndexSettings old = _unitOfWork.IndexRepository.GetById(setting.Id);
                old.Id = setting.Id;
                old.ArName = setting.ArName;
                old.EnName = setting.EnName;
                old.Description = setting.Description;
                old.Email = setting.Email;
                old.Phone = setting.Phone;
                old.StartDay = setting.StartDay;
                old.EndDay = setting.EndDay;
                old.StartHour = setting.StartHour;
                old.EndHour = setting.EndHour;
                old.Locaion = setting.Locaion;

                if (setting.Image != null)
                {
                    if (old.ImageUrl != null)
                    {
                        DocumentSetting.DeleteFile(old.ImageUrl);
                    }
                    old.ImageUrl = DocumentSetting.UploadFile(setting.Image, "Images");
                }
                if (setting.Video != null)
                {
                    if (old.VedioUrl != null)
                    {
                        DocumentSetting.DeleteFile(old.VedioUrl);
                    }
                    old.VedioUrl = DocumentSetting.UploadFile(setting.Video, "Videos");
                }

                _unitOfWork.IndexRepository.Update(old);
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
