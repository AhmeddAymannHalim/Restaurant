using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Newtonsoft.Json;
using Restaurant.Data.Models;
using Service.UnitOfWork;
using ResturantPL.ViewModels;
using System.Security.Claims;
using ResturantPL.Helper;
using Service.Repositories;
using Rotativa.AspNetCore;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace ResturantPL.Controllers
{
    public class OrderController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUnitOfWork _unitOfWork;

        public OrderController(UserManager<ApplicationUser> userManager, IUnitOfWork unitOfWork)
        {
            this._userManager = userManager;
            this._unitOfWork = unitOfWork;
        }


        public ApplicationUser CurrentUser()
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            ApplicationUser currentUser = _userManager.FindByIdAsync(userId).Result;
            return currentUser;
        }


        public IActionResult Index()
        {
            //if (HttpContext.Session.GetString("IsAuthenticated") == "true")
            //{
            var userId = CurrentUser().Id;
            ViewBag.CurrentUser = CurrentUser();

            var hallsSummary = new ShiftSummaryViewModel
            {
                NumberOfOrder = _unitOfWork.OrderRepository
                     .GetAll()
                     .Count(a => a.SellerId == userId && a.ReceiverId == userId && a.Cash && a.Type == OrderType.Hall && a.FinishShift == false),
                TotalAmount = _unitOfWork.OrderRepository
                     .GetAll()
                     .Where(a => a.SellerId == userId && a.ReceiverId == userId && a.Cash && a.Type == OrderType.Hall && a.FinishShift == false)
                     .Sum(a => a.TotalPrice)
            };
            var deliverySummary = new ShiftSummaryViewModel
            {
                NumberOfOrder = _unitOfWork.OrderRepository
                     .GetAll()
                     .Count(a => a.SellerId == userId && a.ReceiverId == userId && a.Cash && a.Type == OrderType.Delivery && a.FinishShift == false),
                TotalAmount = _unitOfWork.OrderRepository
                     .GetAll()
                     .Where(a => a.SellerId == userId && a.ReceiverId == userId && a.Cash && a.Type == OrderType.Delivery && a.FinishShift == false)
                     .Sum(a => a.TotalPrice)
            };

            var deliveryOnlineSummary = new ShiftSummaryViewModel
            {
                NumberOfOrder = _unitOfWork.OrderRepository
                     .GetAll()
                     .Count(a => a.SellerId == userId && a.ReceiverId == userId && a.Cash && a.Type == OrderType.Delivery_Online && a.FinishShift == false),
                TotalAmount = _unitOfWork.OrderRepository
                     .GetAll()
                     .Where(a => a.SellerId == userId && a.ReceiverId == userId && a.Cash && a.Type == OrderType.Delivery_Online && a.FinishShift == false)
                     .Sum(a => a.TotalPrice)
            };

            var takeAwaySummary = new ShiftSummaryViewModel
            {
                NumberOfOrder = _unitOfWork.OrderRepository
                    .GetAll()
                    .Count(a => a.SellerId == userId && a.ReceiverId == userId && a.Cash && a.Type == OrderType.TakeAway && a.FinishShift == false),
                TotalAmount = _unitOfWork.OrderRepository
                    .GetAll()
                    .Where(a => a.SellerId == userId && a.ReceiverId == userId && a.Cash && a.Type == OrderType.TakeAway && a.FinishShift == false)
                    .Sum(a => a.TotalPrice)
            };


            var pendingHalls = new ShiftSummaryViewModel
            {
                NumberOfOrder = _unitOfWork.OrderRepository
                     .GetAll()
                     .Count(a => a.SellerId == userId && a.Cash == false && a.Type == OrderType.Hall && a.FinishShift == false),
                TotalAmount = _unitOfWork.OrderRepository
                     .GetAll()
                     .Where(a => a.SellerId == userId && a.Cash == false && a.Type == OrderType.Hall && a.FinishShift == false)
                     .Sum(a => a.TotalPrice)
            };
            var pendingDelivery = new ShiftSummaryViewModel
            {
                NumberOfOrder = _unitOfWork.OrderRepository
                    .GetAll()
                    .Count(a => a.SellerId == userId && a.Cash == false && a.Type == OrderType.Delivery && a.FinishShift == false),
                TotalAmount = _unitOfWork.OrderRepository
                    .GetAll()
                    .Where(a => a.SellerId == userId && a.Cash == false && a.Type == OrderType.Delivery && a.FinishShift == false)
                    .Sum(a => a.TotalPrice)
            };
            var pendingOnlineDelivery = new ShiftSummaryViewModel
            {
                NumberOfOrder = _unitOfWork.OrderRepository
                   .GetAll()
                   .Count(a => a.SellerId == userId && a.Cash == false && a.Type == OrderType.Delivery_Online && a.FinishShift == false),
                TotalAmount = _unitOfWork.OrderRepository
                   .GetAll()
                   .Where(a => a.SellerId == userId && a.Cash == false && a.Type == OrderType.Delivery_Online && a.FinishShift == false)
                   .Sum(a => a.TotalPrice)
            };


            var totalPending = new ShiftSummaryViewModel
            {
                TotalAmount = pendingHalls.TotalAmount + pendingDelivery.TotalAmount + pendingOnlineDelivery.TotalAmount,
                NumberOfOrder = pendingHalls.NumberOfOrder + pendingDelivery.NumberOfOrder + pendingOnlineDelivery.NumberOfOrder
            };


            var lastPendingHalls = new ShiftSummaryViewModel
            {
                NumberOfOrder = _unitOfWork.OrderRepository
                   .GetAll()
                   .Count(a => a.SellerId != userId && a.Cash == false && a.Type == OrderType.Hall && a.FinishShift == false),
                TotalAmount = _unitOfWork.OrderRepository
                   .GetAll()
                   .Where(a => a.SellerId != userId && a.Cash == false && a.Type == OrderType.Hall && a.FinishShift == false)
                   .Sum(a => a.TotalPrice)
            };
            var lastPendingDelivery = new ShiftSummaryViewModel
            {
                NumberOfOrder = _unitOfWork.OrderRepository
                    .GetAll()
                    .Count(a => a.SellerId != userId && a.Cash == false && a.Type == OrderType.Delivery && a.FinishShift == false),
                TotalAmount = _unitOfWork.OrderRepository
                    .GetAll()
                    .Where(a => a.SellerId != userId && a.Cash == false && a.Type == OrderType.Delivery && a.FinishShift == false)
                    .Sum(a => a.TotalPrice)
            };

            var lastPendingOnlineDelivery = new ShiftSummaryViewModel
            {
                NumberOfOrder = _unitOfWork.OrderRepository
                    .GetAll()
                    .Count(a => a.SellerId != userId && a.Cash == false && a.Type == OrderType.Delivery_Online && a.FinishShift == false),
                TotalAmount = _unitOfWork.OrderRepository
                    .GetAll()
                    .Where(a => a.SellerId != userId && a.Cash == false && a.Type == OrderType.Delivery_Online && a.FinishShift == false)
                    .Sum(a => a.TotalPrice)
            };
            var totalLastPending = new ShiftSummaryViewModel
            {
                TotalAmount = lastPendingHalls.TotalAmount + lastPendingDelivery.TotalAmount + lastPendingOnlineDelivery.TotalAmount,
                NumberOfOrder = lastPendingHalls.NumberOfOrder + lastPendingDelivery.NumberOfOrder + lastPendingOnlineDelivery.NumberOfOrder
            };


            var totalSummary = new ShiftSummaryViewModel
            {
                TotalAmount = hallsSummary.TotalAmount + deliverySummary.TotalAmount + takeAwaySummary.TotalAmount + deliveryOnlineSummary.TotalAmount,
                NumberOfOrder = hallsSummary.NumberOfOrder + deliverySummary.NumberOfOrder + takeAwaySummary.NumberOfOrder + deliveryOnlineSummary.NumberOfOrder
            };
            var selledLastPending = new ShiftSummaryViewModel
            {
                NumberOfOrder = _unitOfWork.OrderRepository
                   .GetAll()
                   .Count(a => a.SellerId != userId && a.ReceiverId == userId && a.Cash == true && a.FinishShift == false),
                TotalAmount = _unitOfWork.OrderRepository
                   .GetAll()
                   .Where(a => a.SellerId != userId && a.ReceiverId == userId && a.Cash == true && a.FinishShift == false)
                   .Sum(a => a.TotalPrice)
            };
            var totalMoney = new ShiftSummaryViewModel
            {
                TotalAmount = totalSummary.TotalAmount + selledLastPending.TotalAmount,
                NumberOfOrder = totalSummary.NumberOfOrder + selledLastPending.NumberOfOrder
            };


            ViewBag.HallsSummary = hallsSummary;
            ViewBag.DeliverySummary = deliverySummary;
            ViewBag.DeliveryOnlineSummary = deliveryOnlineSummary; ///
            ViewBag.TakeAwaySummary = takeAwaySummary;

            ViewBag.PendingHalls = pendingHalls;
            ViewBag.PendingDelivery = pendingDelivery;
            ViewBag.PendingOnlineDelivery = pendingOnlineDelivery;
            ViewBag.TotalPending = totalPending;


            ViewBag.LastPendingHalls = lastPendingHalls;
            ViewBag.LastPendingDelivery = lastPendingDelivery;
            ViewBag.LastPendingOnlineDelivery = lastPendingOnlineDelivery;
            ViewBag.TotalLastPending = totalLastPending;


            ViewBag.TotalSummary = totalSummary;
            ViewBag.SelledLastPending = selledLastPending;
            ViewBag.TotalMoney = totalMoney;

            return View();
            //}
            //else
            //{
            //    return RedirectToAction("Login", "Shift");
            //}
        }


      
        public IActionResult ExportOrderPdf(int orderId)
        {
            var order = _unitOfWork.OrderRepository.GetById(a => a.Id == orderId,
                new string[] { "Seller", "Receiver", "Table", "Customer", "Employee", "OrderMenuItems" },
                new string[] { "OrderMenuItems.MenuItem" });

            // Use Rotativa to convert the view to PDF
            //var viewAsPdf = new ViewAsPdf
            //{
            //    Model = order,
            //    ViewName = "_Details", // Replace with your view name
            //    FileName = $"Order_{order.CustomerName}.pdf" // Customize filename as needed
            //};
            return new ViewAsPdf("_Details", order);
            //return viewAsPdf;
        }

        

        public IActionResult Details(int id)
        {
            Order orderInvoice = _unitOfWork.OrderRepository.GetById(a => a.Id == id,
                new string[] { "Seller", "Receiver", "Table", "Customer", "Employee", "OrderMenuItems" },
                new string[] { "OrderMenuItems.MenuItem" });

            if (orderInvoice != null)
            {
                return View(orderInvoice);
            }
            return NotFound();
        }


        #region Orders Details


        //public IActionResult AllOrders()
        //{
        //    var orders = _unitOfWork.OrderRepository
        //        .GetAll(
        //            a => a.Include(a => a.Seller),
        //            a => a.Include(a => a.Receiver),
        //            a => a.Include(a => a.Table),
        //            a => a.Include(a => a.Employee),
        //            a => a.Include(a => a.OrderMenuItems))
        //        .Where(a => a.Cash && a.FinishShift)
        //        .OrderByDescending(a => a.OrderDate);
        //    //.OrderBy(a => a.OrderDate.Hour)
        //    // Order by the day of the month from the Date property
        //    //.ThenBy(a => a.SellerId).ThenBy(a=> a.ReceiverId);

        //    return View(orders);
        //}

        public IActionResult AllOrders(int page = 1, int pageSize = 10)
        {
            var orders = _unitOfWork.OrderRepository
                .GetAll(
                    a => a.Include(a => a.Seller),
                    a => a.Include(a => a.Receiver),
                    a => a.Include(a => a.Table),
                    a => a.Include(a => a.Employee),
                    a => a.Include(a => a.OrderMenuItems))
                .Where(a => a.Cash && a.FinishShift)
                .OrderByDescending(a => a.OrderDate)
                .ToList();

            var count = orders.Count;
            var paginatedOrders = orders.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            var paginatedList = new PaginatedList<Order>(paginatedOrders, count, page, pageSize);

            return View(paginatedList);
        }
        public IActionResult OrdersByDay(DateTime selectedDate)
        {
            // Filter orders based on the selected date
            var orders = _unitOfWork.OrderRepository.GetAll(
                    a => a.Include(a => a.Seller),
                    a => a.Include(a => a.Receiver),
                    a => a.Include(a => a.Table),
                    a => a.Include(a => a.Employee),
                    a => a.Include(a => a.OrderMenuItems))
                .Where(a => a.Cash == true && a.FinishShift == true && a.OrderDate.Date == selectedDate.Date)
                .OrderBy(a => a.SellerId)
                .ThenBy(a => a.OrderDate);

            // Pass the filtered orders to the view
            return View(orders);
        }

        public IActionResult HallOrders()
        {
            var orders = _unitOfWork.OrderRepository.
                GetAll(a => a.Include(a => a.Seller), a => a.Include(a => a.Table))
                .Where(a => a.Cash == true && a.Type == OrderType.Hall && a.SellerId == CurrentUser().Id && a.FinishShift == false);

            return View(orders);
        }


        public IActionResult DeliveryOrders()
        {
            var orders = _unitOfWork.OrderRepository
                .GetAll(a => a.Include(a => a.Seller), a => a.Include(a => a.Receiver), a => a.Include(a => a.Employee))
                .Where(a => a.Cash == true && a.Type == OrderType.Delivery && a.SellerId == CurrentUser().Id && a.FinishShift == false);

            return View(orders);
        }


        public IActionResult DeliveryOnlineOrders()
        {
            var orders = _unitOfWork.OrderRepository
                .GetAll(a => a.Include(a => a.Seller), a => a.Include(a => a.Receiver), a => a.Include(a => a.Employee))
                .Where(a => a.Cash == true && a.Type == OrderType.Delivery_Online && a.SellerId == CurrentUser().Id && a.FinishShift == false);

            return View(orders);
        }

        public IActionResult TakeAwayOrders()
        {
            var orders = _unitOfWork.OrderRepository
                .GetAll(a => a.Include(a => a.Seller))
                .Where(a => a.Cash == true && a.Type == OrderType.TakeAway && a.SellerId == CurrentUser().Id && a.FinishShift == false);

            return View(orders);
        }




        public IActionResult PendingHall()
        {
            var orders = _unitOfWork.OrderRepository
                .GetAll(a => a.Include(a => a.Seller), a => a.Include(a => a.Table))
                .Where(a => a.Cash == false && a.Type == OrderType.Hall);

            return View(orders);
        }


        public IActionResult PendingDelivery()
        {
            var orders = _unitOfWork.OrderRepository
                .GetAll(a => a.Include(a => a.Seller), a => a.Include(a => a.Customer), a => a.Include(a => a.Employee))
                .Where(a => a.Cash == false && a.Type == OrderType.Delivery);

            return View(orders);
        }
   
        public IActionResult PendingOnlineDelivery()
        {
            var orders = _unitOfWork.OrderRepository
                .GetAll(a => a.Include(a => a.Seller), a => a.Include(a => a.Customer), a => a.Include(a => a.Employee))
                .Where(a => a.Cash == false && a.Type == OrderType.Delivery_Online);

            return View(orders);
        }




        #endregion



        public async Task<IActionResult> Create()
        {
            try
            {
                if (HttpContext.Session.GetString("IsAuthenticated") == "true")
                {

                    ViewBag.CurrentUser = CurrentUser();

                    var menuItems = _unitOfWork.MenuItemRepository.GetAll(a => a.Include(a => a.MenuSection));
                    ViewBag.MenuItems = menuItems;

                    Order lastOrder = await _unitOfWork.OrderRepository.GetLast(item => item.Id);

                    if (lastOrder == null)
                    {
                        ViewBag.OrderCode = 1;
                    }
                    else
                    {
                        int orderCode = int.Parse(lastOrder.OrderCode) + 1;
                        ViewBag.OrderCode = orderCode;
                    }

                    var customers = new SelectList(_unitOfWork.CustomerRepository.GetAll(), "Id", "Name");
                    ViewBag.Customers = customers;

                    var tables = new SelectList(_unitOfWork.TableRepository.GetAll().Where(a => a.IsPending == false), "Id", "Name");
                    ViewBag.Tables = tables;

                    var tayar = new SelectList(_unitOfWork.EmployeeRepository.GetAll().Where(a => a.UserType == "طيار"), "EmployeeId", "FullName");
                    ViewBag.Tayars = tayar;

                    return View();

                }
                else
                {
                    return RedirectToAction("Login", "Shift");
                }
            }
            catch (Exception ex)
            {
                return View("Error", ErrorMessage.Error(ex.Message));
            }
        }



        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(OrderViewModel viewModel)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    Order Order = new Order()
                    {
                        // OrderCode = viewModel.OrderCode,
                        Service = viewModel.Service,
                        Tax = viewModel.Tax,
                        Discount = viewModel.Discount,
                        TotalPrice = viewModel.TotalPrice,
                        Type = viewModel.Type,
                        OrderDate = DateTime.Now,
                        SellerId = CurrentUser().Id,

                        Cash = viewModel.Cash,
                        Complete = true,
                        FinishShift = false,
                        CustomerName = string.IsNullOrEmpty(viewModel.CustomerName) ? "عميل نقدي" : viewModel.CustomerName,
                        CustomerId = viewModel.CustomerId,
                    };


                    // Check the code if exist
                    string startingCode = viewModel.OrderCode;
                    string nextCode = startingCode;
                    while (_unitOfWork.OrderRepository.Find(a => a.OrderCode == nextCode) != null)
                    {
                        nextCode = (int.Parse(nextCode) + 1).ToString();
                    }
                    Order.OrderCode = nextCode;



                    if (viewModel.Cash == false && viewModel.Type == OrderType.Delivery)
                    {
                        Order.ReceiverId = null;
                        Order.EmployeeId = viewModel.EmployeeId;
                        Order.Address = viewModel.Address;           // لو الطلب دليفري
                        Order.PhoneNumber = viewModel.PhoneNumber;   // لو الطلب دليفري
                        Order.MobileNumber = viewModel.MobileNumber; // لو الطلب دليفري
                    }
                    else if (viewModel.Cash == false && viewModel.Type == OrderType.Hall)
                    {
                        Restaurant.Data.Models.Table table = _unitOfWork.TableRepository.GetById(viewModel.TableId);
                        Order.TableId = viewModel.TableId;
                        table.IsPending = true;
                        await _unitOfWork.TableRepository.UpdateAsync(table);
                    }
                    else
                    {
                        Order.ReceiverId = CurrentUser().Id;
                        Order.Cash = true;
                    }

                    await _unitOfWork.OrderRepository.AddAsync(Order);


                    foreach (var orderItem in viewModel.OrderMenuItems)
                    {
                        OrderMenuItem orderMenuItem = new OrderMenuItem()
                        {
                            MenuItemId = orderItem.MenuItemId,
                            OrderId = Order.Id,
                            Quantity = orderItem.Quantity,
                            PriceForUnit = orderItem.PriceForUnit,
                            Price = orderItem.Price,
                        };

                        await _unitOfWork.OrderMenuItemRepository.AddAsync(orderMenuItem);

                        IEnumerable<MenuItemComponent> components = _unitOfWork.MenuItemComponentRepository.GetAll().Where(a => a.MenuItemId == orderMenuItem.MenuItemId);

                        foreach (var component in components)
                        {
                            StoreProduct storeproduct = _unitOfWork.StoreProductRepository.GetById(component.StoreProductId);
                            storeproduct.Quantity -= (orderMenuItem.Quantity * component.Quantity);
                            await _unitOfWork.StoreProductRepository.UpdateAsync(storeproduct);
                        }

                    }

                    return RedirectToAction("Details", new { id = Order.Id });
                }

                return View(viewModel);
            }
            catch (Exception ex)
            {
                return View("Error", ErrorMessage.Error(ex.Message));
            }
        }
       


        public IActionResult Move(int id)
        {
            // Get the order
            var orderFrom = _unitOfWork.OrderRepository.GetById(a => a.Id == id, new string[] { "Table", "OrderMenuItems" }, new string[] { "OrderMenuItems.MenuItem" });

            // Get the pending tables to move to it
            var tables = _unitOfWork.TableRepository.GetAll().Where(a => a.IsPending == true && a.Id != orderFrom.TableId);
            ViewBag.Tables = new SelectList(tables, "Id", "Name");

            if (orderFrom != null)
            {
                return View(orderFrom);
            }
            return NotFound();
        }


        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Move(MoveViewModel viewModel)
        {
            try
            {
                if (ModelState.IsValid)
                {

                    var OrderFrom = _unitOfWork.OrderRepository.GetById(viewModel.Id);
                    var OrderTo = _unitOfWork.OrderRepository.GetById(a => a.TableId == viewModel.OrderTableToId && a.Cash == false);
                    var OrderMenuItem = _unitOfWork.OrderMenuItemRepository.GetById(viewModel.OrderMenuItemId);

                    // order from 
                    OrderFrom.TotalPrice -= OrderMenuItem.Price;
                    await _unitOfWork.OrderRepository.UpdateAsync(OrderFrom);


                    // order to 
                    OrderTo.TotalPrice += OrderMenuItem.Price;
                    await _unitOfWork.OrderRepository.UpdateAsync(OrderTo);

                    // change the OrderId in OrderMenuItem
                    OrderMenuItem.OrderId = OrderTo.Id;
                    await _unitOfWork.OrderMenuItemRepository.UpdateAsync(OrderMenuItem);

                    return RedirectToAction("PendingHall");
                }

                return View(viewModel);
            }
            catch (Exception ex)
            {
                return View("Error", ErrorMessage.Error(ex.Message));
            }
        }


        public IActionResult Merge(int id)
        {
            var orderFrom = _unitOfWork.OrderRepository.GetById(a => a.Id == id, new string[] { "OrderMenuItems" }, new string[] { "OrderMenuItems.MenuItem" });

            var tables = _unitOfWork.TableRepository.GetAll().Where(a => a.IsPending == true && a.Id != orderFrom.TableId);
            ViewBag.Tables = new SelectList(tables, "Id", "Name");

            if (orderFrom != null)
            {
                return View(orderFrom);
            }
            return NotFound();
        }



        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Merge(MergeViewModel viewModel)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var OrderFrom = _unitOfWork.OrderRepository.GetById(viewModel.Id);
                    var OrderTo = _unitOfWork.OrderRepository.GetById(a => a.TableId == viewModel.OrderTableToId && a.Cash == false);
                    var OrderMenuItems = _unitOfWork.OrderMenuItemRepository.GetAll().Where(a => a.OrderId == OrderFrom.Id);

                    // Move all prices from the order to the next from
                    OrderTo.TotalPrice += OrderFrom.TotalPrice;
                    OrderTo.Tax += OrderFrom.Tax;
                    OrderTo.Service += OrderFrom.Service;
                    OrderTo.Discount += OrderFrom.Discount;


                    await _unitOfWork.OrderRepository.UpdateAsync(OrderTo);


                    foreach (var item in OrderFrom.OrderMenuItems)
                    {
                        item.OrderId = OrderTo.Id;
                        await _unitOfWork.OrderMenuItemRepository.UpdateAsync(item);
                    }

                    // Delete all prices from the first order and pay it
                    OrderFrom.TotalPrice = 0;
                    OrderFrom.Service = 0;
                    OrderFrom.Tax = 0;
                    OrderFrom.Discount = 0;
                    OrderFrom.Cash = true;
                    OrderFrom.ReceiverId = CurrentUser().Id;
                    await _unitOfWork.OrderRepository.UpdateAsync(OrderFrom);

                    Restaurant.Data.Models.Table table = _unitOfWork.TableRepository.GetById(OrderFrom.TableId);
                    table.IsPending = false;
                    await _unitOfWork.TableRepository.UpdateAsync(table);


                    return RedirectToAction("PendingHall");

                }

                return View(viewModel);
            }
            catch (Exception ex)
            {
                return View("Error", ErrorMessage.Error(ex.Message));
            }
        }


        public async Task<IActionResult> Pay(int id)
        {
            try
            {
                Order order = _unitOfWork.OrderRepository.GetById(id);
                if (order != null)
                {
                    if (order.Cash == false && order.Type == OrderType.Hall)
                    {
                        Restaurant.Data.Models.Table table = _unitOfWork.TableRepository.GetById(order.TableId);
                        table.IsPending = false;

                        await _unitOfWork.TableRepository.UpdateAsync(table);
                    }

                    order.Cash = true;
                    order.ReceiverId = CurrentUser().Id;
                }

                _unitOfWork.OrderRepository.Update(order);

                return RedirectToAction("Details", new { id = order.Id });
            }
            catch (Exception ex)
            {
                return View("Error", ErrorMessage.Error(ex.Message));
            }
        }



        public IActionResult CustomerInfo(int customerId)
        {
            Customer cutomer = _unitOfWork.CustomerRepository.GetById(customerId);
            if (cutomer != null)
            {
                return Json(cutomer);
            }
            return BadRequest();
        }


        // Pending online order
        public IActionResult PendingOrders()
        {
            var orders = _unitOfWork.OnlineOrderRepository.GetAll().Where(a => a.IsPending == true);

            //int pendingOrderCount = _orderService.GetPendingOrderCount();
            //ViewBag.PendingOrderCount = pendingOrderCount;
            return View(orders);
        }


        public async Task<IActionResult> AddOrder(int id)
        {
            try
            {
                OnlineOrder onlineOrder = await _unitOfWork.OnlineOrderRepository.GetByIdAsync(id);
                TempData["OnlineOrderId"] = onlineOrder.Id;

                ViewBag.OnlineOrder = onlineOrder;

                var orderItems = _unitOfWork.OnlineOrderMenuItemRepository.GetAll(
                    a => a.Include(a => a.OnlineOrder),
                    a => a.Include(a => a.MenuItem))
                    .Where(a => a.OnlineOrderId == onlineOrder.Id);

                ViewBag.OnlineItems = orderItems;

                ViewBag.CurrentUser = CurrentUser();

                var menuItems = _unitOfWork.MenuItemRepository.GetAll(a => a.Include(a => a.MenuSection));
                ViewBag.MenuItems = menuItems;

                Order lastOrder = await _unitOfWork.OrderRepository.GetLast(item => item.Id);

                if (lastOrder == null)
                {
                    ViewBag.OrderCode = 1;
                }
                else
                {
                    int orderCode = int.Parse(lastOrder.OrderCode) + 1;
                    ViewBag.OrderCode = orderCode;
                }

                var customers = new SelectList(_unitOfWork.CustomerRepository.GetAll(), "Id", "Name");
                ViewBag.Customers = customers;


                var tayar = new SelectList(_unitOfWork.EmployeeRepository.GetAll().Where(a => a.UserType == "طيار"), "EmployeeId", "FullName");
                ViewBag.Tayars = tayar;

                return View(onlineOrder);
            }
            catch (Exception ex)
            {
                return View("Error", ErrorMessage.Error(ex.Message));
            }

        }


        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> AddOrder(OrderViewModel viewModel)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    Order Order = new Order()
                    {
                        OrderCode = viewModel.OrderCode,
                        Service = viewModel.Service,
                        Tax = viewModel.Tax,
                        Discount = viewModel.Discount,
                        TotalPrice = viewModel.TotalPrice,
                        Type = viewModel.Type,
                        OrderDate = DateTime.Now,
                        SellerId = CurrentUser().Id,

                        Cash = viewModel.Cash,
                        Complete = true,
                        FinishShift = false,
                        CustomerName = viewModel.CustomerName,
                        CustomerId = viewModel.CustomerId,
                    };

                    if (viewModel.Cash == false && viewModel.Type == OrderType.Delivery_Online)
                    {
                        Order.ReceiverId = null;
                        Order.EmployeeId = viewModel.EmployeeId;
                        Order.Address = viewModel.Address;           // لو الطلب دليفري
                        Order.PhoneNumber = viewModel.PhoneNumber;   // لو الطلب دليفري
                        Order.MobileNumber = viewModel.MobileNumber; // لو الطلب دليفري
                    }

                    await _unitOfWork.OrderRepository.AddAsync(Order);


                    foreach (var orderItem in viewModel.OrderMenuItems)
                    {
                        OrderMenuItem orderMenuItem = new OrderMenuItem()
                        {
                            MenuItemId = orderItem.MenuItemId,
                            OrderId = Order.Id,
                            Quantity = orderItem.Quantity,
                            PriceForUnit = orderItem.PriceForUnit,
                            Price = orderItem.Price,
                        };

                        await _unitOfWork.OrderMenuItemRepository.AddAsync(orderMenuItem);

                        IEnumerable<MenuItemComponent> components = _unitOfWork.MenuItemComponentRepository.GetAll().Where(a => a.MenuItemId == orderMenuItem.MenuItemId);

                        foreach (var component in components)
                        {
                            StoreProduct storeproduct = _unitOfWork.StoreProductRepository.GetById(component.StoreProductId);
                            storeproduct.Quantity -= (orderMenuItem.Quantity * component.Quantity);
                            await _unitOfWork.StoreProductRepository.UpdateAsync(storeproduct);
                        }
                    }

                    int onlineOrderId = (int)TempData["OnlineOrderId"];
                    OnlineOrder onlineOrder = await _unitOfWork.OnlineOrderRepository.GetByIdAsync(onlineOrderId);
                    onlineOrder.IsPending = false;
                    _unitOfWork.OnlineOrderRepository.Update(onlineOrder);

                    return RedirectToAction("Details", new { id = Order.Id });
                }

                return View(viewModel);
            }
            catch (Exception ex)
            {
                return View("Error", ErrorMessage.Error(ex.Message));
            }
        }


        public IActionResult CancelOnlineOrder(int id)
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
      


        [HttpPost, ValidateAntiForgeryToken, ActionName("CancelOnlineOrder")]
        public IActionResult CancelOnlineOrderConfirm(int id)
        {
            try
            {
                OnlineOrder onlineOrder = _unitOfWork.OnlineOrderRepository.GetById(a => a.Id == id,
                    new string[] { "OnlineOrderMenuItems" },
                    new string[] { "OnlineOrderMenuItems.MenuItem" });

                if (onlineOrder != null)
                {
                    _unitOfWork.OnlineOrderRepository.Delete(onlineOrder);
                    return RedirectToAction("PendingOrders");
                }
                return BadRequest();
            }
            catch (Exception ex)
            {
                return View("Error", ErrorMessage.Error(ex.Message));
            }
        }


    }
}
