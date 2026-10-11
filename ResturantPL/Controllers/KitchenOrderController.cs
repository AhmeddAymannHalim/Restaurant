using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Restaurant.DAL.Models;
using Restaurant.DAL.UnitOfWork;

namespace ResturantPL.Controllers
{
    // Kitchen *operations* (orders waiting in the kitchen). The kitchen definition itself lives in Controllers/Definitions.
    [Authorize]
    public class KitchenOrderController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public KitchenOrderController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
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
                    .ThenInclude(o => o.MenuItem)
                 )
                 .Where(a => a.Complete == false)
                .OrderByDescending(a => a.OrderDate)
                .ToList();

            return View(orders);
        }

        public IActionResult KitchenOrderDetails(int id)
        {
            Order orderInvoice = _unitOfWork.OrderRepository.GetById(a => a.Id == id,
               new string[] { "Seller", "Receiver", "Table", "Customer", "Employee", "OrderMenuItems" },
               new string[] { "OrderMenuItems.MenuItem" });

            if (orderInvoice != null)
            {
                return View(orderInvoice);
            }

            return View(new Order());
        }

        [HttpPost]
        public IActionResult CompleteOrder(int id)
        {
            var order = _unitOfWork.OrderRepository.GetById(id);
            if (order is null)
            {
                return NotFound();
            }

            order.Complete = true;
            _unitOfWork.OrderRepository.Update(order);

            return Json(true);
        }
    }
}
