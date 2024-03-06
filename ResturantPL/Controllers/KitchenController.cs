using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Restaurant.Data.Models;
using ResturantPL.Hubs;
using Service.UnitOfWork;

namespace ResturantPL.Controllers
{
    [Authorize]
    public class KitchenController : Controller
    {
        private readonly IHubContext<OrderHub> _orderHubContext;
        private readonly IUnitOfWork _unitOfWork;
       
        public KitchenController(IUnitOfWork unitOfWork, IHubContext<OrderHub> orderHubContext)
        {
            _unitOfWork = unitOfWork;
            _orderHubContext = orderHubContext;
        }

        public IActionResult Index()
        {
            var kitchens = _unitOfWork.kitchenRepository.GetAll();           
            return View(kitchens);
        }
        public IActionResult KitchenOrders()
        {
           var orders = _unitOfWork.OrderRepository
               .GetAll
               (
                   a => a.Include(a => a.Seller),
                   a => a.Include(a => a.Receiver),
                   a => a.Include(a => a.Table),
                   a => a.Include(a => a.Employee),                  
                   a => a.Include(a => a.OrderMenuItems)
                   .ThenInclude(o=>o.MenuItem)                  
                )
                .Where(a => a.Complete == false)
               .OrderByDescending(a => a.OrderDate)
               .ToList();
            return View(orders);
        }
        public IActionResult Details(int id)
        {
            var kitchen = _unitOfWork.kitchenRepository.GetById(id);
            return View(kitchen);
        }

        public IActionResult CreateAsync(int id)
        {
            var kitchen = _unitOfWork.kitchenRepository.GetById(id);
            if (kitchen is not  null) {
                //await _orderHubContext.Clients.All.SendAsync("RefreshOrders");
              
                return View(kitchen);
            }
            else
            {
                return View(new kitchen());
            }
           
        }
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAsync(kitchen model)
        {
            if (!ModelState.IsValid)
                return View(model);

            if (model.Id != 0)
                _unitOfWork.kitchenRepository.Update(model);

             await _unitOfWork.kitchenRepository.AddAsync(model);
            await _orderHubContext.Clients.All.SendAsync("RefreshOrders");
            TempData["success"] = "تمت العملية بنجاح";
            return RedirectToAction(nameof(Index));

        }
        public IActionResult Delete(int id)
        {
            var kitchen = _unitOfWork.kitchenRepository.Find(k=>k.Id==id);
            if (kitchen is not null)
            {
                try
                {
                    _unitOfWork.kitchenRepository.Delete(kitchen);
                    TempData["success"] = "تمت العملية بنجاح";
                  
                }
                catch (Exception ex)
                {
                    TempData["faild"] = "Error";
                }
                
            }                          
            return RedirectToAction(nameof(Index));
        }

    }
}
