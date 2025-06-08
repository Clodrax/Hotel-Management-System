using Hotel.DAL;
using Hotel.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hotel.Controllers
{
    [Authorize(Roles = "Admin")]
    public class RoomTypesController : Controller
    {
        private readonly AppDbContext _Db;
        private readonly IWebHostEnvironment _env;
        public RoomTypesController(AppDbContext Db, IWebHostEnvironment env)
        {

            _Db = Db;
            _env = env;
        }
        public async Task<IActionResult> Index()
        {

            List<RoomType> roomTypes = await _Db.RoomTypes.OrderByDescending(x => x.Id).ToListAsync();

            return View(roomTypes);
        }
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Create(RoomType courses)
        {
            bool isExist = await _Db.RoomTypes.AnyAsync(x => x.Name == courses.Name);
            if (isExist)
            {
                ModelState.AddModelError("Name", "Bu kurs artıq mövcuddur !");
                return View(courses);
            }
            await _Db.RoomTypes.AddAsync(courses);
            await _Db.SaveChangesAsync();

            return RedirectToAction("Index");
        }
        public async Task<IActionResult> Update(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            RoomType _DbCourses = await _Db.RoomTypes.FirstOrDefaultAsync(x => x.Id == id);
            if (_DbCourses == null)
            {
                return BadRequest();
            }
            return View(_DbCourses);

        }
        [HttpPost]
        public async Task<IActionResult> Update(int? id, RoomType roomTypes)
        {
            if (id == null)
            {
                return NotFound();
            }
            RoomType _DbRoomTypes = await _Db.RoomTypes.FirstOrDefaultAsync(x => x.Id == id);
            if (_DbRoomTypes == null)
            {
                return BadRequest();
            }
            bool isExist = await _Db.RoomTypes.AnyAsync(x => x.Name == roomTypes.Name && x.Id != id);
            if (isExist)
            {
                ModelState.AddModelError("Name", "This courses is already exist !");
                return View(roomTypes);
            }

            _DbRoomTypes.Name = roomTypes.Name;
            _DbRoomTypes.Mark = roomTypes.Mark;

            await _Db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Detail(int? id)
        {

            RoomType roomTypes = await _Db.RoomTypes.FindAsync(id);


            //_DbCourses.Name = service.Name;
            //_DbCourses.Description = service.Description;

            return View(roomTypes);
        }
        //public async Task<IActionResult> Delete(int? id)
        //{
        //    if (id == null)
        //    {
        //        return NotFound();
        //    }
        //    Courses _DbCourses = await _Db.Courses.FirstOrDefaultAsync(x => x.Id == id);
        //    if (_DbCourses == null)
        //    {
        //        return BadRequest();
        //    }
        //    return View(_DbCourses);

        //}
        //[HttpPost]
        //[ActionName("Delete")]
        //public async Task<IActionResult> DeletePost(int? id)
        //{
        //    if (id == null)
        //    {
        //        return NotFound();
        //    }
        //    Courses _DbCourses = await _Db.Courses.FirstOrDefaultAsync(x => x.Id == id);
        //    if (_DbCourses == null)
        //    {
        //        return BadRequest();
        //    }
        //    _DbCourses.IsDeactive = true;
        //    await _Db.SaveChangesAsync();
        //    return RedirectToAction("Index");
        //}
        public async Task<IActionResult> Activity(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            RoomType _DbRoomTypes = await _Db.RoomTypes.FirstOrDefaultAsync(x => x.Id == id);
            if (_DbRoomTypes == null)
            {
                return BadRequest();
            }
            if (_DbRoomTypes.IsDeactive)
            {
                _DbRoomTypes.IsDeactive = false;
            }
            else
            {
                _DbRoomTypes.IsDeactive = true;
            }
            await _Db.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    }
}
