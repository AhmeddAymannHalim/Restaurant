using AutoMapper;
using Restaurant.Data.Models;
using Service.Repositories;
using Service.UnitOfWork;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ResturantPL.Helper;
using ResturantPL.ViewModels;
using System.Diagnostics;

namespace ResturantPL.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;

        public HomeController(ILogger<HomeController> logger, IMapper mapper, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            this._mapper = mapper;
            this._unitOfWork = unitOfWork;
        }



        #region index

        public async Task<IActionResult> Index()
        {

            var model = (await _unitOfWork.IndexRepository.GetAllAsync()).FirstOrDefault();
            if (model == null)
            {
                return RedirectToAction("RestaurantInformation");
            }
            return View(model);
        }

        

        
        
        #endregion



        #region Chefs

        public async Task<IActionResult> Chefs()
        {
            var chefItems = (IList<ChefItem>)await _unitOfWork.ChefItemRepository.GetAllAsync();

            return View(chefItems);
        }

        public IActionResult ChefAdd()
        {
            return View(new ChefItemViewModel());
        }


        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ChefAdd(ChefItemViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var ChefItem = _mapper.Map<ChefItem>(model);
                    ChefItem.ImageUrl = DocumentSetting.UploadFile(model.Image, "Images");
                    await _unitOfWork.ChefItemRepository.AddAsync(ChefItem);
                    return RedirectToAction("Chefs");
                }
                return View(model);
            }
            catch (Exception ex)
            {
                return View("", ex.Message);
            }
        }


        public async Task<IActionResult> ChefUpdate(int id)
        {
            var ChefItem = await _unitOfWork.ChefItemRepository.GetByIdAsync(id);
            var ChefItemViewModel = _mapper.Map<ChefItemViewModel>(ChefItem);
            return View(ChefItemViewModel);
        }


        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ChefUpdate(ChefItemViewModel model)
        {

            if (ModelState.IsValid)
            {
                var ChefItem = await _unitOfWork.ChefItemRepository.GetByIdAsync(model.Id);
                ChefItem.Name = model.Name;
                ChefItem.Jop = model.Jop;

                if (model.Image != null)
                {
                    ChefItem.ImageUrl = DocumentSetting.UploadFile(model.Image, "Images");
                }
                _unitOfWork.ChefItemRepository.Update(ChefItem);
                return RedirectToAction("Chefs");
            }
            return View(model);

        }


        public async Task<IActionResult> ChefDelete(int id)
        {
            var ChefItem = await _unitOfWork.ChefItemRepository.GetByIdAsync(id);
            var ChefItemViewModel = _mapper.Map<ChefItemViewModel>(ChefItem);
            return View(ChefItemViewModel);
        }



        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ChefDelete(ChefItemViewModel model)
        {
            if (ModelState.IsValid)
            {
                var ChefItem = await _unitOfWork.ChefItemRepository.GetByIdAsync(model.Id);
                DocumentSetting.DeleteFile(ChefItem.ImageUrl);

                _unitOfWork.ChefItemRepository.Delete(ChefItem);
                return RedirectToAction("Chefs");
            }
            return View(model);
        }



        #endregion

        #region Gallery

        public async Task<IActionResult> Gallery()
        {
            var GalleryItems = (IList<GalleryItem>)await _unitOfWork.GalleryItemRepository.GetAllAsync();
            return View(GalleryItems);
        }

        public IActionResult GalleryAdd()
        {
            return View(new GalleryItemViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> GalleryAdd(GalleryItemViewModel model)
        {

            if (ModelState.IsValid)
            {
                var GalleryItem = _mapper.Map<GalleryItem>(model);
                GalleryItem.PictureUrl = DocumentSetting.UploadFile(model.Image, "Images");
                await _unitOfWork.GalleryItemRepository.AddAsync(GalleryItem);
                return RedirectToAction("Gallery");
            }
            return View(model);
        }



        public async Task<IActionResult> GalleryDelete(int id)
        {
            var GalleryItem = await _unitOfWork.GalleryItemRepository.GetByIdAsync(id);
            var GalleryItemViewModel = _mapper.Map<GalleryItemViewModel>(GalleryItem);
            return View(GalleryItemViewModel);
        }
        [HttpPost]
        public async Task<IActionResult> GalleryDelete(GalleryItemViewModel model)
        {
            if (ModelState.IsValid)
            {

                var GalleryItem = await _unitOfWork.GalleryItemRepository.GetByIdAsync(model.Id);

                DocumentSetting.DeleteFile(GalleryItem.PictureUrl);

                _unitOfWork.GalleryItemRepository.Delete(GalleryItem);
                return RedirectToAction("Gallery");
            }
            return View(model);
        }

        #endregion




        #region Error

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { ErrorMessage = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        #endregion



    }
}