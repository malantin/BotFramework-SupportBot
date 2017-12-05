using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SupportBot.Portal.Models;
using SupportBot.Services;

namespace SupportBot.Portal.Controllers
{
    public class QnAMakerServicesController : Controller
    {
        private readonly SupportBotContext _context;

        public QnAMakerServicesController(SupportBotContext context)
        {
            _context = context;
        }

        // GET: QnAMakerServices
        public async Task<IActionResult> Index()
        {
            return View(await _context.QnAMakerService.ToListAsync());
        }

        // GET: QnAMakerServices/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var qnAMakerService = await _context.QnAMakerService
                .SingleOrDefaultAsync(m => m.Id == id);
            if (qnAMakerService == null)
            {
                return NotFound();
            }

            return View(qnAMakerService);
        }

        // GET: QnAMakerServices/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: QnAMakerServices/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,ServiceId,BaseAddress,Key,DisplayName,DisplayCategory")] QnAMakerService qnAMakerService)
        {
            if (ModelState.IsValid)
            {
                _context.Add(qnAMakerService);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(qnAMakerService);
        }

        // GET: QnAMakerServices/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var qnAMakerService = await _context.QnAMakerService.SingleOrDefaultAsync(m => m.Id == id);
            if (qnAMakerService == null)
            {
                return NotFound();
            }
            return View(qnAMakerService);
        }

        // POST: QnAMakerServices/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("Id,ServiceId,BaseAddress,Key,DisplayName,DisplayCategory")] QnAMakerService qnAMakerService)
        {
            if (id != qnAMakerService.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(qnAMakerService);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!QnAMakerServiceExists(qnAMakerService.Id))
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
            return View(qnAMakerService);
        }

        // GET: QnAMakerServices/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var qnAMakerService = await _context.QnAMakerService
                .SingleOrDefaultAsync(m => m.Id == id);
            if (qnAMakerService == null)
            {
                return NotFound();
            }

            return View(qnAMakerService);
        }

        // POST: QnAMakerServices/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var qnAMakerService = await _context.QnAMakerService.SingleOrDefaultAsync(m => m.Id == id);
            _context.QnAMakerService.Remove(qnAMakerService);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool QnAMakerServiceExists(string id)
        {
            return _context.QnAMakerService.Any(e => e.Id == id);
        }
    }
}
