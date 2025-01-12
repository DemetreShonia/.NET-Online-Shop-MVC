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
                        ProductName = product.product.Name,
                        Price = product.product.ListPrice,
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

            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            ViewData["ProductCategoryId"] = new SelectList(_context.ProductCategories, "ProductCategoryId", "Name", product.ProductCategoryId);
            ViewData["ProductModelId"] = new SelectList(_context.ProductModels, "ProductModelId", "Name", product.ProductModelId);
            
            return View(product);
        }

        // POST: Products/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ProductId,Name,ProductNumber,Color,StandardCost,ListPrice,Size,Weight,ProductCategoryId,ProductModelId,SellStartDate,SellEndDate,DiscontinuedDate,ThumbNailPhoto,ThumbnailPhotoFileName,Rowguid,ModifiedDate")] Product product)
        {
            if (id != product.ProductId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(product);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(product.ProductId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Table));
            }
            ViewData["ProductCategoryId"] = new SelectList(_context.ProductCategories, "ProductCategoryId", "Name", product.ProductCategoryId);
            ViewData["ProductModelId"] = new SelectList(_context.ProductModels, "ProductModelId", "Name", product.ProductModelId);
            return View(product);
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
