using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OBSS.Data;
using OBSS.Models;
using Microsoft.AspNetCore.Authorization;

namespace OBSS.Controllers
{
    public class SalesDetailsController : Controller
    {
        private readonly OBSSContext _context;

        public SalesDetailsController(OBSSContext context)
        {
            _context = context;
        }

        // GET: SalesDetails
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            var oBSSContext = _context.SalesDetails.Include(s => s.Book).Include(s => s.Sale);
            return View(await oBSSContext.ToListAsync());
        }

        // GET: SalesDetails/Details/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var salesDetail = await _context.SalesDetails.Include(s => s.Book).Include(s => s.Sale).FirstOrDefaultAsync(m => m.SaleId == id);
            
            if (salesDetail == null)
            {
                return NotFound();
            }

            return View(salesDetail);
        }

        // GET: SalesDetails/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            ViewData["BookId"] = new SelectList(_context.Books, "BookId", "BookId");
            ViewData["SaleId"] = new SelectList(_context.Sales, "SaleId", "SaleId");
            return View();
        }

        // POST: SalesDetails/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("SaleId,DetailId,BookId,Quantity,Price")] SalesDetail salesDetail)
        {
            if (ModelState.IsValid)
            {
                _context.Add(salesDetail);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["BookId"] = new SelectList(_context.Books, "BookId", "BookId", salesDetail.BookId);
            ViewData["SaleId"] = new SelectList(_context.Sales, "SaleId", "SaleId", salesDetail.SaleId);
            return View(salesDetail);
        }

        // GET: SalesDetails/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var salesDetail = await _context.SalesDetails.FindAsync(id);
            if (salesDetail == null)
            {
                return NotFound();
            }
            ViewData["BookId"] = new SelectList(_context.Books, "BookId", "BookId", salesDetail.BookId);
            ViewData["SaleId"] = new SelectList(_context.Sales, "SaleId", "SaleId", salesDetail.SaleId);
            return View(salesDetail);
        }

        // POST: SalesDetails/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("SaleId,DetailId,BookId,Quantity,Price")] SalesDetail salesDetail)
        {
            if (id != salesDetail.SaleId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(salesDetail);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SalesDetailExists(salesDetail.SaleId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["BookId"] = new SelectList(_context.Books, "BookId", "BookId", salesDetail.BookId);
            ViewData["SaleId"] = new SelectList(_context.Sales, "SaleId", "SaleId", salesDetail.SaleId);
            return View(salesDetail);
        }

        // GET: SalesDetails/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var salesDetail = await _context.SalesDetails.Include(s => s.Book).Include(s => s.Sale).FirstOrDefaultAsync(m => m.SaleId == id);
            if (salesDetail == null)
            {
                return NotFound();
            }

            return View(salesDetail);
        }

        // POST: SalesDetails/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var salesDetail = await _context.SalesDetails.FindAsync(id);
            if (salesDetail != null)
            {
                _context.SalesDetails.Remove(salesDetail);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: SalesDetails/DownloadReport
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DownloadReport()
        {
            var salesDetails = await _context.SalesDetails.Include(s => s.Book).Include(s => s.Sale).ToListAsync();

            var sb = new StringBuilder();
            sb.AppendLine("DetailId,SaleId,BookId,BookTitle,Quantity,Price,SaleDate");

            foreach (var detail in salesDetails)
            {
                sb.AppendLine($"{detail.DetailId}," + $"{detail.SaleId}," + $"{detail.BookId}," +$"{detail.Book?.BookTitle}," + $"{detail.Quantity}," +$"{detail.Price}," + $"{detail.Sale?.SaleDate:yyyy-MM-dd}");
            }

            var bytes = Encoding.UTF8.GetBytes(sb.ToString());
            return File(bytes, "text/csv", "SalesDetailsReport.csv");
        }

        private bool SalesDetailExists(int id)
        {
            return _context.SalesDetails.Any(e => e.SaleId == id);
        }
    }
}
