using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Restaurant.Data.Models;
using ResturantPL.Helper;
using ResturantPL.ViewModels;
using Service.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc.Abstractions;

namespace ResturantPL.Controllers
{
    public class StoreConvertController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public StoreConvertController(IUnitOfWork unitOfWork)
        {
            this._unitOfWork = unitOfWork;
        }


        public IActionResult Index()
        {
            var converts = _unitOfWork.StoreConvertRepository.GetAll(
                a => a.Include(a => a.FromStore),
                a => a.Include(a => a.ToStore),
                a => a.Include(a => a.StoreConvertProducts));

            return View(converts);
        }

        public IActionResult Details(int id)
        {
            StoreConvert convert = _unitOfWork.StoreConvertRepository.
                GetById(a => a.Id == id, new string[] { "FromStore", "ToStore", "StoreConvertProducts" }, new string[] { "StoreConvertProducts.StoreProduct" });
            if (convert != null)
            {
                return View(convert);
            }
            return NotFound();
        }


        public async Task<IActionResult> Create()
        {
            ViewBag.Stores = new SelectList(_unitOfWork.StoreRepository.GetAll().ToList(), "Id", "Name");
            StoreConvert convert = await _unitOfWork.StoreConvertRepository.GetLast(item => item.Id);

            if (convert == null)
            {
                ViewBag.Convert = 1;
            }
            else
            {
                int convertCode = int.Parse(convert.ConvertCode) + 1;
                ViewBag.Convert = convertCode;
            }

            return View();
        }


        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(StoreConvertViewModel model)
        {
            try
            {
                if (model.StoreConvertProducts.Count() < 1)
                {
                    return RedirectToAction("Create");
                }
                if (ModelState.IsValid)
                {
                    


                    StoreConvert convert = new StoreConvert()
                    {
                        ConvertCode = model.ConvertCode,
                        ConvertDate = DateTime.Now,
                        FromStoreId = model.FromStoreId,
                        ToStoreId = model.ToStoreId,
                    };
                    await _unitOfWork.StoreConvertRepository.AddAsync(convert);

                    foreach (var item in model.StoreConvertProducts)
                    {
                        StoreConvertProduct productConverts = new StoreConvertProduct()
                        {
                            StoreConvertId = convert.Id,
                            StoreProductId = item.StoreProductId,
                            Quantity = item.Quantity,
                        };
                        await _unitOfWork.StoreConvertProductRepository.AddAsync(productConverts);

                        StoreProduct fromStoreProduct = await _unitOfWork.StoreProductRepository.GetByIdAsync(productConverts.StoreProductId);
                        if (fromStoreProduct != null)
                        {
                            fromStoreProduct.Quantity -= productConverts.Quantity;
                        }
                        await _unitOfWork.StoreProductRepository.UpdateAsync(fromStoreProduct);


                        StoreProduct toStoreProduct = _unitOfWork.StoreProductRepository.GetById(a => a.StoreId == convert.ToStoreId && a.ProductId == fromStoreProduct.ProductId);
                        if (toStoreProduct != null)
                        {
                            toStoreProduct.Quantity += productConverts.Quantity;
                        }
                        await _unitOfWork.StoreProductRepository.UpdateAsync(toStoreProduct);
                    }

                    return RedirectToAction("Details", new { id = convert.Id });
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
            var convert = _unitOfWork.StoreConvertRepository.GetById(id);
            if (convert != null)
            {
                try
                {
                    _unitOfWork.StoreConvertRepository.Delete(convert);
                    TempData["success"] = "تمت العملية بنجاح";
                }
                catch (Exception ex)
                {
                    TempData["faild"] = "Error";
                    return View("Error", ErrorMessage.Error(ex.Message));
                }
            }
            return RedirectToAction("Index");
        }





        public IActionResult GetProductsByStore(int storeId)
        {
            var products = _unitOfWork.StoreProductRepository.GetAll().Where(a => a.StoreId == storeId);
            return Json(products);
        }



        public IActionResult GetStoresByStoreId(int storeId)
        {
            var stores = _unitOfWork.StoreRepository.GetAll().Where(a => a.Id != storeId);
            return Json(stores);
        }


       
        public IActionResult GetQuantityByProduct(int productId)
        {
            var product = _unitOfWork.StoreProductRepository.GetById(productId);
            return Json(product);
        }






        //public IActionResult ExportToPdf(int id)
        //{
        //    var model = _unitOfWork.StoreConvertRepository.GetById(id);

        //    var htmlContent = "<h1>Store Convert Details</h1>"; // Replace with your actual HTML content

        //    var doc = new HtmlToPdfDocument()
        //    {
        //        GlobalSettings = {
        //        ColorMode = ColorMode.Color,
        //        Orientation = Orientation.Portrait,
        //        PaperSize = PaperKind.A4Plus,
        //    },
        //        Objects = {
        //        new ObjectSettings()
        //        {
        //            HtmlContent = htmlContent,
        //        }
        //    }
        //    };

        //    var pdfBytes = _pdfConverter.Convert(doc);
        //    return File(pdfBytes, "application/pdf", "StoreConvert.pdf");
        //}



        //// Action method to export to Excel
        //public async Task<IActionResult> ExportToExcel(int id)
        //{
        //    var model = await _unitOfWork.StoreConvertRepository.GetByIdAsync(id);
        //    var stream = new MemoryStream();
        //    using (var package = new ExcelPackage(stream))
        //    {
        //        var worksheet = package.Workbook.Worksheets.Add("StoreConvert");
        //        worksheet.Cells.LoadFromCollection(model.StoreConvertProducts, true);
        //        package.Save();
        //    }
        //    stream.Position = 0;
        //    string excelName = $"StoreConvert_{id}.xlsx";
        //    return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", excelName);
        //}




    }
}
