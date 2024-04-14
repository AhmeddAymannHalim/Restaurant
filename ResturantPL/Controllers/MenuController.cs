using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Restaurant.Data.Models;
using Service.UnitOfWork;
using ResturantPL.Helper;
using ResturantPL.ViewModels;
using System.Xml.Linq;

namespace ResturantPL.Controllers
{
    public class MenuController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public MenuController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            this._unitOfWork = unitOfWork;
            this._mapper = mapper;
        }


        public IActionResult Index()
        {
            var MenuItems = _unitOfWork.MenuItemRepository.GetAll(a => a.Include(a => a.MenuSection));

            return View(MenuItems);
        }


        public IActionResult Details(int id)
        {
            MenuItem item = _unitOfWork.MenuItemRepository.GetById(a => a.Id == id, new string[] { "Store", "Components", "MenuSection" }, new string[] { "Components.StoreProduct" });
            if (item != null)
            {
                return View(item);
            }
            return NotFound();
        }


        public IActionResult CreateMenuItem()
        {
            ViewBag.MenuSections = new SelectList(_unitOfWork.MenuSectionRepository.GetAll(), "Id", "Name");
            ViewBag.Stores = new SelectList(_unitOfWork.StoreRepository.GetAll().ToList(), "Id", "Name");
            return View();
        }





        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateMenuItem(MenuItemViewModel model)
        {
            try
            {
                if (model.MenuItemComponents.Count() < 1)
                {
                    return RedirectToAction("CreateMenuItem");
                }

                if (ModelState.IsValid)
                {
                    MenuItem menuItem = new MenuItem()
                    {
                        Name = model.Name,
                        MenuSectionId = model.MenuSectionId,
                        Size = model.Size == Size.regular ? null : model.Size,
                        Description = model.Description,
                        ImageUrl = DocumentSetting.UploadFile(model.Image, "Menu Images"),
                        StoreId = model.StoreId,
                        TotalCost = model.TotalCost,
                        Price = model.Price,
                        Benefit = model.Benefit,
                        IsReady = true,
                    };

                    await _unitOfWork.MenuItemRepository.AddAsync(menuItem);

                    foreach (var item in model.MenuItemComponents)
                    {
                        MenuItemComponent component = new MenuItemComponent()
                        {
                            MenuItemId = menuItem.Id,
                            StoreProductId = item.StoreProductId,
                            Quantity = item.Quantity,
                            Cost = item.Cost
                        };
                        await _unitOfWork.MenuItemComponentRepository.AddAsync(component);
                    }


                    return RedirectToAction("Details", new { id = menuItem.Id });
                }
                return View(model);
            }
            catch (Exception ex)
            {
                return View("Error", ErrorMessage.Error(ex.Message));
            }
        }

        public IActionResult AddSize(int id)
        {
            ViewBag.MenuSections = new SelectList(_unitOfWork.MenuSectionRepository.GetAll(), "Id", "Name");
            ViewBag.Stores = new SelectList(_unitOfWork.StoreRepository.GetAll().ToList(), "Id", "Name");
            var item = _unitOfWork.MenuItemRepository.GetById(id);
            if (item != null)
            {
                ViewBag.ItemDetail = item;
            }
            return View();
        }





        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> AddSize(MenuItemViewModel model)
        {
            try
            {
                ViewBag.MenuSections = new SelectList(_unitOfWork.MenuSectionRepository.GetAll(), "Id", "Name");
                ViewBag.Stores = new SelectList(_unitOfWork.StoreRepository.GetAll().ToList(), "Id", "Name");

                if (model.MenuItemComponents.Count() < 1)
                {
                    return RedirectToAction("AddSize");
                }




                if (ModelState.IsValid)
                {
                    MenuItem menuItem = new MenuItem()
                    {
                        Name = model.Name,
                        MenuSectionId = model.MenuSectionId,
                        Size = model.Size == Size.regular ? null : model.Size,
                        Description = model.Description,
                        ImageUrl = DocumentSetting.UploadFile(model.Image, "Menu Images"),
                        StoreId = model.StoreId,
                        TotalCost = model.TotalCost,
                        Price = model.Price,
                        Benefit = model.Benefit,
                        IsReady = true,
                    };

                    await _unitOfWork.MenuItemRepository.AddAsync(menuItem);

                    foreach (var item in model.MenuItemComponents)
                    {
                        MenuItemComponent component = new MenuItemComponent()
                        {
                            MenuItemId = menuItem.Id,
                            StoreProductId = item.StoreProductId,
                            Quantity = item.Quantity,
                            Cost = item.Cost
                        };
                        await _unitOfWork.MenuItemComponentRepository.AddAsync(component);
                    }


                    return RedirectToAction("Details", new { id = menuItem.Id });
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
            MenuItem item = _unitOfWork.MenuItemRepository.GetById(a => a.Id == id, new string[] { "Store", "Components", "MenuSection" }, new string[] { "Components.StoreProduct" });
            if (item != null)
            {
                return View(item);
            }
            return NotFound();
        }


        [HttpPost, ValidateAntiForgeryToken, ActionName("Delete")]
        public IActionResult ConfirmDelete(int id)
        {
            try
            {
                MenuItem item = _unitOfWork.MenuItemRepository.GetById(id);
                if (item != null)
                {
                    _unitOfWork.MenuItemRepository.Delete(item);
                    return RedirectToAction("Index");
                }
                return BadRequest();
            }
            catch (Exception ex)
            {
                return View("Error", ErrorMessage.Error(ex.Message));
            }
        }




        [HttpGet]
        public IActionResult GetProductsByStore(int storeId)
        {
            var products = _unitOfWork.StoreProductRepository.GetAll().Where(a => a.StoreId == storeId);
            return Json(products);
        }

        [HttpGet]
        public IActionResult GetPriceByProduct(int productId)
        {
            var product = _unitOfWork.StoreProductRepository.GetById(productId);
            return Json(product);
        }

        [HttpGet]
        public IActionResult CheckSize(string name, string size)
        {
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(size))
            {
                return BadRequest("Name and size parameters are required.");
            }

            if (!Enum.TryParse(typeof(Size), size, out object sizeEnum))
            {
                return BadRequest("Invalid size parameter.");
            }

            var exist = _unitOfWork.MenuItemRepository.Any(m => m.Name == name && m.Size == (Size)sizeEnum);

            return Json(exist);
        }


    }
}
