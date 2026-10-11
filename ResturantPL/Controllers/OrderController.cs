using Microsoft.AspNetCore.Identity;
using Shared.Helpers;
using Shared.ViewModels.Commons;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Newtonsoft.Json;
using Restaurant.DAL.Models;
using Restaurant.DAL.UnitOfWork;
using ResturantPL.ViewModels;
using System.Security.Claims;
using ResturantPL.Helper;
using Restaurant.DAL.Repositories;
using Rotativa.AspNetCore;
using Microsoft.AspNetCore.SignalR;
using ResturantPL.Hubs;
using System.Linq.Expressions;
using System.Globalization;
using Microsoft.AspNetCore.Authorization;


namespace ResturantPL.Controllers
{

    public class OrderController : Controller
    {
        private readonly IHubContext<OrderHub> _orderHubContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUnitOfWork _unitOfWork;

        public OrderController(UserManager<ApplicationUser> userManager, IUnitOfWork unitOfWork, IHubContext<OrderHub> orderHubContext)
        {
            this._userManager = userManager;
            this._unitOfWork = unitOfWork;
            _orderHubContext = orderHubContext;
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
                   .Count(a => a.SellerId != userId && a.ReceiverId == userId && a.Cash && a.FinishShift == false),
                TotalAmount = _unitOfWork.OrderRepository
                   .GetAll()
                   .Where(a => a.SellerId != userId && a.ReceiverId == userId && a.Cash && a.FinishShift == false)
                   .Sum(a => a.TotalPrice)
            };
            var remainForPending = new ShiftSummaryViewModel
            {
                TotalAmount = totalPending.TotalAmount + totalLastPending.TotalAmount,
                NumberOfOrder = totalPending.NumberOfOrder + totalLastPending.NumberOfOrder
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
            ViewBag.RemainForPending = remainForPending;
            ViewBag.TotalMoney = totalMoney;

            return View();
            //}
            //else
            //{
            //    return RedirectToAction("Login", "Shift");
            //}
        }

        public async Task<IActionResult> Details(int id)
        {
            ViewBag.Printers = await _unitOfWork.PrinterSettingRepository.GetLast(a => a.Id);
            var settings = await _unitOfWork.ApplicationSettingRepository.GetLast(a => a.Id);


            ViewBag.NumberOfCustomerPrint = settings.NumberOfCustomerPrint;
            ViewBag.PrintAfterOrder = settings.PrintAfterOrder;
            ViewBag.PrintForTakeawayOrder = settings.PrintForTakeawayOrder;
            // ViewBag.PrintForKitchen = settings.PrintForKitchen;
            // ViewBag.NumberOfKitchenPrint = settings.NumberOfKitchenPrint;


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



        public IActionResult Delete(int id)
        {
            var obj = _unitOfWork.OrderRepository.GetById(id);
            if (obj != null)
            {
                try
                {
                    _unitOfWork.OrderRepository.Delete(obj);
                    return Json(new { success = true });
                }
                catch (Exception)
                {
                    return Json(new { success = false, errorMessage = "نأسف حدث خطأ اثناء الحذف , ربما يكون مرتبطا بشئ آخر " });
                }
            }
            return Json(new { success = true });
        }




        #endregion


        public async Task<IActionResult> Create()
        {
            try
            {
                if (HttpContext.Session.GetString("IsAuthenticated") == "true")
                {


                    ViewBag.Settings = await _unitOfWork.ApplicationSettingRepository.GetLast(a => a.Id);



                    ViewBag.CurrentUser = CurrentUser();

                    ViewBag.MenuItems = _unitOfWork.MenuItemRepository.GetAll(a => a.Include(a => a.MenuSection));

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

                    ViewBag.Customers = new SelectList(_unitOfWork.CustomerRepository.GetAll(), "Id", "Name");

                    ViewBag.Tables = new SelectList(_unitOfWork.TableRepository.GetAll().Where(a => a.IsPending == false), "Id", "Name");

                    ViewBag.Tayars = new SelectList(_unitOfWork.EmployeeRepository.GetAll().Where(a => a.UserType == "طيار"), "EmployeeId", "FullName");

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
                if (viewModel.OrderMenuItems.Count() < 1)
                {
                    ViewBag.MenuItems = _unitOfWork.MenuItemRepository.GetAll(a => a.Include(a => a.MenuSection));
                    ViewBag.MenuItems = _unitOfWork.MenuItemRepository.GetAll(a => a.Include(a => a.MenuSection));
                    ViewBag.Settings = await _unitOfWork.ApplicationSettingRepository.GetLast(a => a.Id);
                    ViewBag.CurrentUser = CurrentUser();
                    ViewBag.Customers = new SelectList(_unitOfWork.CustomerRepository.GetAll(), "Id", "Name");
                    ViewBag.Tables = new SelectList(_unitOfWork.TableRepository.GetAll().Where(a => a.IsPending == false), "Id", "Name");
                    ViewBag.Tayars = new SelectList(_unitOfWork.EmployeeRepository.GetAll().Where(a => a.UserType == "طيار"), "EmployeeId", "FullName");

                    Order lastOrder2 = await _unitOfWork.OrderRepository.GetLast(item => item.Id);
                    if (lastOrder2 == null)
                    {
                        ViewBag.OrderCode = 1;
                    }
                    else
                    {
                        int orderCode = int.Parse(lastOrder2.OrderCode) + 1;
                        ViewBag.OrderCode = orderCode;
                    }


                    return View(viewModel);
                }


                ViewBag.MenuItems = _unitOfWork.MenuItemRepository.GetAll(a => a.Include(a => a.MenuSection));
                ViewBag.MenuItems = _unitOfWork.MenuItemRepository.GetAll(a => a.Include(a => a.MenuSection));
                ViewBag.Settings = await _unitOfWork.ApplicationSettingRepository.GetLast(a => a.Id);
                ViewBag.CurrentUser = CurrentUser();
                ViewBag.Customers = new SelectList(_unitOfWork.CustomerRepository.GetAll(), "Id", "Name");
                ViewBag.Tables = new SelectList(_unitOfWork.TableRepository.GetAll().Where(a => a.IsPending == false), "Id", "Name");
                ViewBag.Tayars = new SelectList(_unitOfWork.EmployeeRepository.GetAll().Where(a => a.UserType == "طيار"), "EmployeeId", "FullName");

                Order lastOrder1 = await _unitOfWork.OrderRepository.GetLast(item => item.Id);
                if (lastOrder1 == null)
                {
                    ViewBag.OrderCode = 1;
                }
                else
                {
                    int orderCode = int.Parse(lastOrder1.OrderCode) + 1;
                    ViewBag.OrderCode = orderCode;
                }



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
                        Complete = false,
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
                        Order.Table = null;
                    }
                    else if (viewModel.Cash == false && viewModel.Type == OrderType.Hall)
                    {
                        if (viewModel.TableId == null)
                        {
                            return View(viewModel);
                        }

                        Restaurant.DAL.Models.Table table = _unitOfWork.TableRepository.GetById(viewModel.TableId);
                        Order.TableId = viewModel.TableId;
                        Order.EmployeeId = null;
                        table.IsPending = true;
                        await _unitOfWork.TableRepository.UpdateAsync(table);
                    }
                    else
                    {
                        Order.ReceiverId = CurrentUser().Id;
                        Order.Cash = true;
                        Order.Table = null;
                        Order.EmployeeId = null;

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
                    await _orderHubContext.Clients.All.SendAsync("RefreshOrders");
                    return RedirectToAction("Details", new { id = Order.Id });
                }
                // Notify clients that a new order has been added
               // await _orderHubContext.Clients.All.SendAsync("RefreshOrders");
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

                    Restaurant.DAL.Models.Table table = _unitOfWork.TableRepository.GetById(OrderFrom.TableId);
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
                        Restaurant.DAL.Models.Table table = _unitOfWork.TableRepository.GetById(order.TableId);
                        table.IsPending = false;

                        await _unitOfWork.TableRepository.UpdateAsync(table);
                    }

                    order.Cash = true;
                    order.ReceiverId = CurrentUser().Id;
                    order.OrderDate = DateTime.Now;
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

        public async Task<IActionResult> GetTaxPercentage()
        {
            ApplicationSetting setting = await _unitOfWork.ApplicationSettingRepository.GetLast(a => a.Id);

            return Json(setting);
        }







        // Pending online order
        public IActionResult PendingOrders()
        {
            var orders = _unitOfWork.OnlineOrderRepository.GetAll().Where(a => a.IsPending == true);


            return View(orders);
        }


        public async Task<IActionResult> AddOrder(int id)
        {
            try
            {
                ViewBag.Settings = await _unitOfWork.ApplicationSettingRepository.GetLast(a => a.Id);

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
                    ViewBag.Settings = await _unitOfWork.ApplicationSettingRepository.GetLast(a => a.Id);

                    if (viewModel.OrderMenuItems.Count() < 1)
                    {
                        return View(viewModel);
                    }

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
