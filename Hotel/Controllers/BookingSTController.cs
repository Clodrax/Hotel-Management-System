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

    public class BookingSTController : Controller
    {

        private readonly AppDbContext _Db;
        private readonly IWebHostEnvironment _env;



        public BookingSTController(AppDbContext Db, IWebHostEnvironment env)
        {

            _Db = Db;
            _env = env;
        }
        public async Task<IActionResult> Index()
        {
            //List<Groups> groups = await _Db.Groups.Include(x => x.Courses).OrderByDescending(x => x.Id).ToListAsync();

            //return View(groups);
            var groups = await _Db.BookingStatus
           .Include(s => s.BookingRooms)
           .ThenInclude(sg => sg.Rooms)
           .ToListAsync();
            ViewBag.BookingRoom = await _Db.BookingRoom.ToListAsync();

            return View(groups);
        }
        #region create
        public async Task<IActionResult> Create()
        {
            ViewBag.RoomTypes = await _Db.RoomTypes.ToListAsync();

            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BookingStatus bookingStatus, int CatId)
        {
            ViewBag.RoomTypes = await _Db.RoomTypes.ToListAsync();


            //    #region Save Image


            //    if (groups.Photo == null)
            //    {
            //        ModelState.AddModelError("Photo", "Image can't be null!!");
            //        return View();
            //    }
            //    if (!groups.Photo.IsImage())
            //    {
            //        ModelState.AddModelError("Photo", "Please select image type");
            //        return View();
            //    }
            //    if (groups.Photo == null)
            //    {
            //        ModelState.AddModelError("Photo", "max 1mb !!");
            //        return View();
            //    }

            //    string folder = Path.Combine(_env.WebRootPath, "images");
            //groups.Image = await groups.Photo.SaveFileAsync(folder);
            //    #endregion
            #region Exist Item
            bool isExist = await _Db.BookingStatus.AnyAsync(x => x.Name == bookingStatus.Name);
            if (isExist)
            {
                ModelState.AddModelError("Name", "This Booking Status is already exist !");
                return View(bookingStatus);
            }
            #endregion

            await _Db.BookingStatus.AddAsync(bookingStatus);
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
            BookingStatus _DbRoomTypes = await _Db.BookingStatus.FirstOrDefaultAsync(x => x.Id == id);
            if (_DbRoomTypes == null)
            {
                return BadRequest();
            }
            ViewBag.RoomTypes = await _Db.RoomTypes.ToListAsync();
            return View(_DbRoomTypes);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(int? id, BookingStatus groups, int CatId)
        {
            if (id == null)
            {
                return NotFound();
            }
            BookingStatus _DbGroups = await _Db.BookingStatus.FirstOrDefaultAsync(x => x.Id == id);
            if (_DbGroups == null)
            {
                return BadRequest();
            }
            ViewBag.RoomTypes = await _Db.RoomTypes.ToListAsync();
            //#region Exist Item
            //bool isExist = await _Db.Teachers.AnyAsync(x => x.Name == teachers.Name && CatId != id);
            //if (isExist)
            //{
            //    ModelState.AddModelError("Name", "This teachers is already exist !");
            //    return View(teachers);
            //}
            //#endregion
            //#region Save Image


            //if (groups.Photo != null)
            //{
            //    if (!groups.Photo.IsImage())
            //    {
            //        ModelState.AddModelError("Photo", "Şəkil seçin!!");
            //        return View();
            //    }
            //    if (teachers.Photo == null)
            //    {
            //        ModelState.AddModelError("Photo", "max 1mb !!");
            //        return View();
            //    }
            //    string folder = Path.Combine(_env.WebRootPath, "images");
            //    _DbGroups.Image = await teachers.Photo.SaveFileAsync(folder);

            //}

            //#endregion
            _DbGroups.Name = groups.Name;



            await _Db.SaveChangesAsync();
            return RedirectToAction("Index");
        }
        #endregion
        #region Activity
        public async Task<IActionResult> Activity(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            BookingStatus _DbGroups = await _Db.BookingStatus.FirstOrDefaultAsync(x => x.Id == id);
            if (_DbGroups == null)
            {
                return BadRequest();
            }
            if (_DbGroups.IsDeactive)
            {
                _DbGroups.IsDeactive = false;
            }
            else
            {
                _DbGroups.IsDeactive = true;
            }
            await _Db.SaveChangesAsync();
            return RedirectToAction("Index");
        }
        #endregion
        public async Task<IActionResult> Room(int id)
        {

            var bookingStatus = await _Db.BookingStatus
            .Include(s => s.BookingRooms)
                .ThenInclude(sg => sg.Rooms).FirstOrDefaultAsync();


            ViewBag.RoomTypes = await _Db.RoomTypes.ToListAsync();
            ViewBag.GroupStudent = await _Db.BookingRoom.ToListAsync();


            return View(bookingStatus);
        }

    }
}
