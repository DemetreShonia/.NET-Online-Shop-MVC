using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DataAccessLayer.Data;
using DataAccessLayer.Models;
using PresentationLayer.Models;
using static NuGet.Packaging.PackagingConstants;
using Microsoft.IdentityModel.Tokens;

namespace PresentationLayer.Controllers
{
    public class ProductsController : Controller
    {
        private readonly CompanyDbContext _context;

        public ProductsController(CompanyDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Table()
        {
            var products = await _context.Products
                .Join(
                    _context.ProductCategories,
                    product => product.ProductCategoryId,
                    category => category.ProductCategoryId,
                    (product, category) => new { product, category }
                )
                .GroupJoin(
                    _context.SalesOrderDetails, 
                    product => product.product.ProductId,
                    order => order.ProductId,
                    (product, orders) => new ProductViewModel
                    {
                        ProductId = product.product.ProductId,
                        Name = product.product.Name,
                        ListPrice = product.product.ListPrice,
                        ProductCategoryId = product.category.ProductCategoryId,
                        CategoryName = product.category.Name,
                        NumberOfOrders = orders.Count()  
                    })
                .ToListAsync();

            return View(products);
        }


        //public async Task<IActionResult> Table()
        //{
        //    var companyDbContext = _context.Products.Include(p => p.ProductCategory).Include(p => p.ProductModel);
        //    return View(await companyDbContext.ToListAsync());
        //}

        // GET: Products/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.ProductCategory)
                .Include(p => p.ProductModel)
                .FirstOrDefaultAsync(m => m.ProductId == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // GET: Products/Create
        public IActionResult Create()
        {
            ViewData["ProductCategoryId"] = new SelectList(_context.ProductCategories, "ProductCategoryId", "Name");
            ViewData["ProductModelId"] = new SelectList(_context.ProductModels, "ProductModelId", "Name");
            return View();
        }

        // POST: Products/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ProductId,Name,ProductNumber,Color,StandardCost,ListPrice,Size,Weight,ProductCategoryId,ProductModelId,SellStartDate,SellEndDate,DiscontinuedDate,ThumbNailPhoto,ThumbnailPhotoFileName,Rowguid,ModifiedDate")] Product product)
        {
            if (ModelState.IsValid)
            {
                _context.Add(product);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Table));
            }
            ViewData["ProductCategoryId"] = new SelectList(_context.ProductCategories, "ProductCategoryId", "Name", product.ProductCategoryId);
            ViewData["ProductModelId"] = new SelectList(_context.ProductModels, "ProductModelId", "Name", product.ProductModelId);
            return View(product);
        }

        // GET: Products/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.ProductCategory)
                .FirstOrDefaultAsync(p => p.ProductId == id);

            if (product == null)
            {
                return NotFound();
            }

            var orders = await _context.SalesOrderDetails
                .Where(o => o.ProductId == product.ProductId)
                .CountAsync();

            var productViewModel = new ProductViewModel
            {
                ProductId = product.ProductId,
                Name = product.Name,
                ProductNumber = product.ProductNumber,
                ListPrice = product.ListPrice,
                Size = product.Size,
                ProductCategoryId = product.ProductCategoryId,
                ProductModelId = product.ProductModelId,
                CategoryName = product.ProductCategory?.Name ?? string.Empty,
                NumberOfOrders = orders,
                ThumbnailPhotoFileName = product.ThumbnailPhotoFileName
            };

            ViewData["ProductCategoryId"] = new SelectList(_context.ProductCategories, "ProductCategoryId", "Name", product.ProductCategoryId);
            ViewData["ProductModelId"] = new SelectList(_context.ProductModels, "ProductModelId", "Name", product.ProductModelId);

            return View(productViewModel);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ProductViewModel productViewModel)
        {
            if (id != productViewModel.ProductId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var product = await _context.Products.FindAsync(id);
                    if (product == null)
                    {
                        return NotFound();
                    }

                    // Update product properties from view model
                    product.Name = productViewModel.Name ?? "Unnamed Product";  // Default name if null
                    product.ProductNumber = productViewModel.ProductNumber ?? $"P{DateTime.Now.Ticks}";
                    product.ListPrice = productViewModel.ListPrice;
                    product.Size = productViewModel.Size;
                    product.ProductCategoryId = productViewModel.ProductCategoryId;
                    product.ProductModelId = productViewModel.ProductModelId;

                    // Handle photo upload
                    if (productViewModel.Photo != null)
                    {
                        string folder = "images/products";
                        string filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", folder);

                        if (!Directory.Exists(filePath))
                        {
                            Directory.CreateDirectory(filePath);
                        }

                        // Delete old photo if exists
                        if (!string.IsNullOrEmpty(product.ThumbnailPhotoFileName))
                        {
                            string oldFilePath = Path.Combine(filePath, product.ThumbnailPhotoFileName);
                            if (System.IO.File.Exists(oldFilePath))
                            {
                                System.IO.File.Delete(oldFilePath);
                            }
                        }

                        // Save new photo
                        string fileName = Guid.NewGuid().ToString() + Path.GetExtension(productViewModel.Photo.FileName);
                        string fileSavePath = Path.Combine(filePath, fileName);

                        using (var stream = new FileStream(fileSavePath, FileMode.Create))
                        {
                            await productViewModel.Photo.CopyToAsync(stream);
                        }

                        product.ThumbnailPhotoFileName = fileName;
                    }

                    _context.Update(product);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Table));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(productViewModel.ProductId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            ViewData["ProductCategoryId"] = new SelectList(_context.ProductCategories, "ProductCategoryId", "Name", productViewModel.ProductCategoryId);
            ViewData["ProductModelId"] = new SelectList(_context.ProductModels, "ProductModelId", "Name", productViewModel.ProductModelId);
            return View(productViewModel);
        }

        // GET: Products/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.ProductCategory)
                .Include(p => p.ProductModel)
                .FirstOrDefaultAsync(m => m.ProductId == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                _context.Products.Remove(product);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Table));
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.ProductId == id);
        }
    }
}
