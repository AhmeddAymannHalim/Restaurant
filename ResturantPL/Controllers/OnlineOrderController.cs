using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Restaurant.Data.Models;
using ResturantPL.Helper;
using ResturantPL.Hubs;
using ResturantPL.ViewModels;
using Service.Repositories;
using Service.UnitOfWork;
using System.Security.Claims;

namespace ResturantPL.Controllers
{
    public class OnlineOrderController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHubContext<OrderHub> _orderHubContext;

        public OnlineOrderController(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager, IHubContext<OrderHub> orderHubContext)
        {
            this._unitOfWork = unitOfWork;
            this._userManager = userManager;
            this._orderHubContext = orderHubContext;
        }


        public ApplicationUser CurrentUser()
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            ApplicationUser currentUser = _userManager.FindByIdAsync(userId).Result;
            return currentUser;
        }


        public IActionResult Menu()
        {
            var MenuItems = _unitOfWork.MenuItemRepository.GetAll(a => a.Include(a => a.MenuSection));

            return View(MenuItems);
        }


        public IActionResult Basket()
        {
            return View();
        }


        public IActionResult MyOrders()
        {
            return View();
        }


        [Authorize]
        public async Task<IActionResult> Create(OnlineOrderViewModel viewModel)
        {
            try
            {
                if (viewModel.OnlineOrderMenuItems.Count()<1)
                {
                    return RedirectToAction("Basket");
                    //return View("Error", ErrorMessage.Error("برجاء اضافة منتجات الي السلة لطلبها"));
                }

                if (ModelState.IsValid)
                {
                    OnlineOrder order = new OnlineOrder()
                    {
                        CustomerName = viewModel.CustomerName,
                        CustomerId = CurrentUser().Id,
                        Address = viewModel.Address,
                        MobileNumber = viewModel.MobileNumber,
                        PhoneNumber = viewModel.PhoneNumber,
                        OrderDate = DateTime.Now,
                        TotalPrice = viewModel.TotalPrice,
                        IsPending = true,
                    };
                    await _unitOfWork.OnlineOrderRepository.AddAsync(order);

                    foreach (var item in viewModel.OnlineOrderMenuItems)
                    {
                        MenuItem menuItem = _unitOfWork.MenuItemRepository.GetById(item.MenuItemId);
                        OnlineOrderMenuItem onlinemenuItem = new OnlineOrderMenuItem()
                        {
                            OnlineOrderId = order.Id,
                            MenuItemId = item.MenuItemId,
                            Quantity = item.Quantity,
                            PriceForUnit = menuItem.Price,
                            Price = (menuItem.Price * item.Quantity)
                        };

                        if (onlinemenuItem.Quantity > 0)
                        {
                            await _unitOfWork.OnlineOrderMenuItemRepository.AddAsync(onlinemenuItem);
                        }
                    }
                    await _orderHubContext.Clients.All.SendAsync("RefreshOnlineOrders");
                    return RedirectToAction("PendingOrders");
                }
                return RedirectToAction("Menu");
            }
            catch (Exception ex)
            {
                return View("Error", ErrorMessage.Error(ex.Message));
            }

        }


        public IActionResult PendingOrders()
        {
            var orders = _unitOfWork.OnlineOrderRepository.GetAll()
                .Where(a => a.CustomerId == CurrentUser().Id);

            return View(orders);
        }


        public IActionResult Details(int id)
        {
            OnlineOrder onlineOrder = _unitOfWork.OnlineOrderRepository.GetById(a => a.Id == id,
                new string[] { "OnlineOrderMenuItems" },
                new string[] { "OnlineOrderMenuItems.MenuItem" });

            if (onlineOrder != null)
            {
                return View(onlineOrder);
            }
            return NotFound();
        }


        public IActionResult Delete(int id)
        {
            var onlineOrder = _unitOfWork.OnlineOrderRepository.GetById(id);
            if (onlineOrder != null)
            {
                try
                {
                    _unitOfWork.OnlineOrderRepository.Delete(onlineOrder);
                    TempData["success"] = "تمت العملية بنجاح";
                }
                catch (Exception ex)
                {
                    TempData["faild"] = "Error";
                    return View("Error", ErrorMessage.Error(ex.Message));
                }
            }
            return RedirectToAction("PendingOrders");
        }


    }
}
