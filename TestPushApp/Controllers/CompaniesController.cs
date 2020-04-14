using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using TestPushApp.Data;
using TestPushApp.Models;

namespace TestPushApp.Controllers
{
    public class CompaniesController : Controller
    {
        private readonly ApplicationDbContext _context;
        protected HttpClient ClientFireBase;

        public CompaniesController(ApplicationDbContext context)
        {
            _context = context;
            ClientFireBase = StartFireBase();
        }

        // GET: Companies
        public async Task<IActionResult> Index()
        {
            return View(await _context.Companies.ToListAsync());
        }

        // GET: Companies/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var company = await _context.Companies
                .FirstOrDefaultAsync(m => m.Id == id);
            if (company == null)
            {
                return NotFound();
            }

            return View(company);
        }

        // GET: Companies/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Companies/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Email")] Company company)
        {
            if (ModelState.IsValid)
            {
                _context.Add(company);
                await _context.SaveChangesAsync();

                var body = new
                {
                    notification = new
                    {
                        body = $"New company: {company.Name} - {company.Email}",
                        title = "Company Created - Just a test"
                    },
                    priority = "high",
                    data = new
                    {
                        clickaction = "FLUTTERNOTIFICATIONCLICK",
                        id = "1",
                        status = "done"
                    },
                    to = "/topics/all"
                };

                ClientFireBase.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("key", "=AAAADRYMQjc:APA91bGmjuHc10rAwh86UX2_e_b-xAiJ9wHtWTQMXSIOxWF0p2wxDgBF8Lv3Csvyi_wG5NLwC5zrK8Sm1PD74crAKSvNYaVycLoYUEuSzUb7LvSHCBVQfHHBNK0wbNX1KBGFKtEfLo0P");
                var response = await ClientFireBase.PostAsync("fcm/send", new StringContent(
                    JsonConvert.SerializeObject(body),
                    Encoding.UTF8, "application/json"));
                var retFireBase = await response.Content.ReadAsStringAsync();


                return RedirectToAction(nameof(Index));
            }

            return View(company);
        }

        // GET: Companies/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var company = await _context.Companies.FindAsync(id);
            if (company == null)
            {
                return NotFound();
            }
            return View(company);
        }

        // POST: Companies/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Email")] Company company)
        {
            if (id != company.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(company);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CompanyExists(company.Id))
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
            return View(company);
        }

        // GET: Companies/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var company = await _context.Companies
                .FirstOrDefaultAsync(m => m.Id == id);
            if (company == null)
            {
                return NotFound();
            }

            return View(company);
        }

        // POST: Companies/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var company = await _context.Companies.FindAsync(id);
            _context.Companies.Remove(company);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CompanyExists(int id)
        {
            return _context.Companies.Any(e => e.Id == id);
        }

        private HttpClient StartFireBase()
        {
            var client = new HttpClient();
            client.BaseAddress = new Uri("https://fcm.googleapis.com");
            return client;
        }
    }
}
