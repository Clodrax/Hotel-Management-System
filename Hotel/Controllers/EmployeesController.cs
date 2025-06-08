using Hotel.DAL;
using Hotel.Helper;
using Hotel.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Hotel.Controllers
{
    [Authorize(Roles = "Admin")]

    public class EmployeesController : Controller
    {
        private readonly AppDbContext _Db;
        private readonly IWebHostEnvironment _env;
        public EmployeesController(AppDbContext Db, IWebHostEnvironment env)
        {

            _Db = Db;
            _env = env;
        }
        public async Task<IActionResult> Index()
        {
            List<Employee> employees = await _Db.Employers.Include(x => x.Positions).OrderByDescending(x => x.Id).ToListAsync();
            return View(employees);
        }

        #region create
        public async Task<IActionResult> Create()
        {
            ViewBag.Positions = await _Db.Positions.ToListAsync();

            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Employee employees, int CatId)
        {
            ViewBag.Positions = await _Db.Positions.ToListAsync();

            #region Save Image

            if (employees.Photo != null)
            {
                if (!employees.Photo.IsImage())
                {
                    ModelState.AddModelError("Photo", "Please select image type");
                    return View(employees);
                }
                if (employees.Photo == null)
                {
                    ModelState.AddModelError("Photo", "max 1mb !!");
                    return View(employees);
                }

                string folder = Path.Combine(_env.WebRootPath, "images", "employee");
                employees.Image = await employees.Photo.SaveFileAsync(folder);
            }
            else
            {
                employees.Image = "employeepfp.jpg";
            }

            #endregion

            //#region Exist Item
            //bool isExist = await _Db.Employers.AnyAsync(x => x.Name == employees.Name);
            //if (isExist)
            //{
            //    ModelState.AddModelError("Name", "This employer is already exist !");
            //    return View(employees);
            //}
            //#endregion
            employees.PositionsId = CatId;
            await _Db.Employers.AddAsync(employees);
            await _Db.SaveChangesAsync();
            return RedirectToAction("Index");
        }
        #endregion
        #region Update
        public async Task<IActionResult> Update(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            Employee _DbEmployers = await _Db.Employers.FirstOrDefaultAsync(x => x.Id == id);
            if (_DbEmployers == null)
            {
                return BadRequest();
            }
            ViewBag.Positions = await _Db.Positions.ToListAsync();
            return View(_DbEmployers);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(int? id, Employee employees, int CatId)
        {
            if (id == null)
            {
                return NotFound();
            }
            Employee _DbEmployers = await _Db.Employers.FirstOrDefaultAsync(x => x.Id == id);
            if (_DbEmployers == null)
            {
                return BadRequest();
            }
            ViewBag.Positions = await _Db.Positions.ToListAsync();
            //#region Exist Item
            //bool isExist = await _Db.Teachers.AnyAsync(x => x.Name == teachers.Name && CatId != id);
            //if (isExist)
            //{
            //    ModelState.AddModelError("Name", "This teachers is already exist !");
            //    return View(teachers);
            //}
            //#endregion
            #region Save Image
            if (employees.Photo != null)
            {
                if (!employees.Photo.IsImage())
                {
                    ModelState.AddModelError("Photo", "Please select a valid image.");
                    return View(employees);
                }
                if (!employees.Photo.IsMb())
                {
                    ModelState.AddModelError("Photo", "Maximum file size is 1MB.");
                    return View(employees);
                }

                string folder = Path.Combine(_env.WebRootPath, "images", "guest");
                string oldImagePath = Path.Combine(folder, _DbEmployers.Image);
                if (System.IO.File.Exists(oldImagePath))
                {
                    System.IO.File.Delete(oldImagePath);
                }
                _DbEmployers.Image = await employees.Photo.SaveFileAsync(folder);
            }


            #endregion
            _DbEmployers.Name = employees.Name;
            _DbEmployers.Email = employees.Email;
            _DbEmployers.Salary = employees.Salary;
            _DbEmployers.Birthday = employees.Birthday;
            _DbEmployers.PhoneNumber = employees.PhoneNumber;

            _DbEmployers.PositionsId = CatId;

            await _Db.SaveChangesAsync();
            return RedirectToAction("Index");
        }
        #endregion
        public async Task<IActionResult> Detail(int id)
        {
            Employee employees = await _Db.Employers.FindAsync(id);
            ViewBag.Positions = await _Db.Positions.ToListAsync();
            return View(employees);
        }
        #region Activity
        public async Task<IActionResult> Activity(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            Employee _DbEmployers = await _Db.Employers.FirstOrDefaultAsync(x => x.Id == id);
            if (_DbEmployers == null)
            {
                return BadRequest();
            }
            if (_DbEmployers.IsDeactive)
            {
                _DbEmployers.IsDeactive = false;
            }
            else
            {
                _DbEmployers.IsDeactive = true;
            }
            await _Db.SaveChangesAsync();
            return RedirectToAction("Index");
        }
        #endregion
        public async Task<IActionResult> SendEmployersSms(int id)
        {
            var employees = _Db.Employers.Where(x => x.Id == id).FirstOrDefaultAsync();

            return View(employees);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendEmployersSms(int id, string subject, string text)
        {
            var employees = _Db.Employers.Where(x => x.Id == id).FirstOrDefaultAsync();
            var email = employees.Result.Email;
            await Sms.SendMailAsync(subject, text, email);

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> SendAllEmployersSms()
        {

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendAllEmployersSms(string subject, string text)
        {
            List<Employee> employees = await _Db.Employers.ToListAsync();

            foreach (var employee in employees)
            {
                await Sms.SendMailAsync(subject, text, employee.Email);
            }

            return RedirectToAction("Index");
        }
        public async Task<IActionResult> Salary(int id)
        {
            var employee = await _Db.Employers.FindAsync(id);
            double salary = Convert.ToDouble(employee.Salary);
            Cost cost = new Cost()
            {
                CreateTime = DateTime.UtcNow.AddHours(4),
                Amount = salary,
                By = User.Identity.Name,
                Description = $"From paying {employee.Name}'s salary"
            };
            Budget budget = await _Db.Budgets.FirstOrDefaultAsync();
            budget.LastModifiedDescription = cost.Description;
            budget.LastModifiedDate = DateTime.UtcNow.AddHours(4);
            budget.LastModifiedAmount = cost.Amount;
            budget.LastModifiedBy = cost.By;
            budget.TotalBudget -= cost.Amount;
            employee.IsPaid = true;
            employee.LastSalaryPaidDate = DateTime.UtcNow.AddHours(4);
            await _Db.Costs.AddAsync(cost);
            await _Db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employee = await _Db.Employers
                .FirstOrDefaultAsync(m => m.Id == id);
            if (employee == null)
            {
                return NotFound();
            }

            return View(employee);
        }
        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]

        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var employee = await _Db.Employers.FindAsync(id);
            if (employee != null)
            {
                _Db.Employers.Remove(employee);
                string folder = Path.Combine(_env.WebRootPath, "images", "guest");
                string path = Path.Combine(folder, employee.Image);
                if (System.IO.File.Exists(path))
                {
                    if (employee.Image == "employeepfp.jpg")
                        await _Db.SaveChangesAsync();
                    else
                    {
                        System.IO.File.Delete(path);
                    }

                }

                await _Db.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
