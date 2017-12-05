using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SupportBot.Models;
using SupportBot.Portal.Models;

namespace SupportBot.Portal.Controllers
{
    public class QnAPairsController : Controller
    {
        private readonly SupportBotContext _context;

        public QnAPairsController(SupportBotContext context)
        {
            _context = context;
        }

        // GET: QnAPairs
        public async Task<IActionResult> Index()
        {
            return View(await _context.QnAPair.ToListAsync());
        }

        // GET: QnAPairs/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var qnAPair = await _context.QnAPair
                .SingleOrDefaultAsync(m => m.Id == id);
            if (qnAPair == null)
            {
                return NotFound();
            }

            return View(qnAPair);
        }

        // GET: QnAPairs/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: QnAPairs/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,DateAdded,Question,Answer")] QnAPair qnAPair)
        {
            if (ModelState.IsValid)
            {
                _context.Add(qnAPair);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(qnAPair);
        }

        // GET: QnAPairs/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var qnAPair = await _context.QnAPair.SingleOrDefaultAsync(m => m.Id == id);
            if (qnAPair == null)
            {
                return NotFound();
            }
            return View(qnAPair);
        }

        // POST: QnAPairs/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,DateAdded,Question,Answer")] QnAPair qnAPair)
        {
            if (id != qnAPair.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(qnAPair);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!QnAPairExists(qnAPair.Id))
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
            return View(qnAPair);
        }

        // GET: QnAPairs/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var qnAPair = await _context.QnAPair
                .SingleOrDefaultAsync(m => m.Id == id);
            if (qnAPair == null)
            {
                return NotFound();
            }

            return View(qnAPair);
        }

        // POST: QnAPairs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var qnAPair = await _context.QnAPair.SingleOrDefaultAsync(m => m.Id == id);
            _context.QnAPair.Remove(qnAPair);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool QnAPairExists(int id)
        {
            return _context.QnAPair.Any(e => e.Id == id);
        }
    }
}
