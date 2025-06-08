using Hotel.DAL;
using Hotel.Helper;
using Hotel.Models;
using Hotel.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using OfficeOpenXml;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Hotel.Controllers
{
    [Authorize(Roles = "Admin")]

    public class RoomsController : Controller
    {
        private readonly AppDbContext _Db;
        private readonly IWebHostEnvironment _env;
        public RoomsController(AppDbContext Db, IWebHostEnvironment env)
        {

            _Db = Db;
            _env = env;
        }
        public async Task<IActionResult> Index(int page = 1)
        {
            int take = 3;
            int roomCount = await _Db.Rooms.CountAsync();
            ViewBag.PageCount = Math.Ceiling((decimal)roomCount / take);

            if (page < 1 || page > ViewBag.PageCount)
            {
                return RedirectToAction(nameof(Error));
            }

            var rooms = await _Db.Rooms
            .Include(s => s.BookingRooms)
            .ThenInclude(sg => sg.BookingStatus)
            .ThenInclude(s => s.RoomTypes)
            .Include(s => s.RoomTypes)
            .OrderByDescending(x => x.Id)
            .Skip((page - 1) * take)
            .Take(take).ToListAsync();

            ViewBag.BookingStatus = await _Db.BookingStatus.ToListAsync();
            ViewBag.Page = page;
            return View(rooms);
        }
        #region create
        public async Task<IActionResult> Create()
        {
            ViewBag.RoomTypes = await _Db.RoomTypes.ToListAsync();
            ViewBag.BookingStatus = await _Db.BookingStatus.ToListAsync();
            //ViewBag.BookingRooms = await _Db.BookingRooms.ToListAsync();

            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Room rooms, int CatId, int GatId)
        {
            ViewBag.RoomTypes = await _Db.RoomTypes.ToListAsync();
            ViewBag.BookingStatus = await _Db.BookingStatus.ToListAsync();
            //ViewBag.BookingRooms = await _Db.BookingRooms.ToListAsync();

            #region Save Image


            if (rooms.Photo == null)
            {
                ModelState.AddModelError("Photo", "Image can't be null!!");
                return View();
            }
            if (!rooms.Photo.IsImage())
            {
                ModelState.AddModelError("Photo", "Please select image type");
                return View();
            }
            if (rooms.Photo.IsMb())
            {
                ModelState.AddModelError("Photo", "max 1mb !!");
                return View();
            }

            string folder = Path.Combine(_env.WebRootPath, "images", "room");
            rooms.Image = await rooms.Photo.SaveFileAsync(folder);
            #endregion

            #region Exist Item
            bool isExist = await _Db.Rooms.AnyAsync(x => x.Name == rooms.Name);
            if (isExist)
            {
                ModelState.AddModelError("Name", "This room is already exist !");
                return View(rooms);
            }
            #endregion
            var BookingRooms = new List<BookingRoom>();
            var bookingRooms = new BookingRoom
            {
                RoomId = rooms.Id,
                BookingStatusId = GatId
            };
            BookingRooms.Add(bookingRooms);
            rooms.BookingRooms = BookingRooms;
            rooms.RoomTypesId = CatId;
            var roomType = await _Db.RoomTypes.FindAsync(rooms.RoomTypesId);
            if (roomType != null)
            {
                rooms.Payment = roomType.Mark;
            }
            else
            {
                ModelState.AddModelError("", "Room Type Is not Available");
                return View();
            }
            await _Db.Rooms.AddAsync(rooms);
            // await _Db.GroupStudent.AddAsync(BookingRoomss);
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
            Room _DbRooms = await _Db.Rooms.FirstOrDefaultAsync(x => x.Id == id);
            if (_DbRooms == null)
            {
                return BadRequest();
            }
            ViewBag.RoomTypes = await _Db.RoomTypes.ToListAsync();
            ViewBag.BookingStatus = await _Db.BookingStatus.ToListAsync();
            ViewBag.Rooms = await _Db.Rooms.ToListAsync();
            return View(_DbRooms);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(int? id, Room rooms, int CatId, int GatId)
        {
            if (id == null)
            {
                return NotFound();
            }
            Room _DbRooms = await _Db.Rooms.FirstOrDefaultAsync(x => x.Id == id);
            if (_DbRooms == null)
            {
                return BadRequest();
            }

            BookingRoom bookingRoom = await _Db.BookingRoom.Where(x => x.RoomId == id).FirstOrDefaultAsync();
            if (bookingRoom == null)
            {
                return BadRequest();
            }

            ViewBag.RoomTypes = await _Db.RoomTypes.ToListAsync();
            ViewBag.BookingStatus = await _Db.BookingStatus.ToListAsync();


            //#region Exist Item
            //bool isExist = await _Db.Teachers.AnyAsync(x => x.Name == teachers.Name && CatId != id);
            //if (isExist)
            //{
            //    ModelState.AddModelError("Name", "This teachers is already exist !");
            //    return View(teachers);
            //}
            //#endregion
            #region Save Image


            if (rooms.Photo != null)
            {
                if (!rooms.Photo.IsImage())
                {
                    ModelState.AddModelError("Photo", "Şəkil seçin!!");
                    return View();
                }
                if (rooms.Photo == null)
                {
                    ModelState.AddModelError("Photo", "max 1mb !!");
                    return View();
                }
                string folder = Path.Combine(_env.WebRootPath, "images", "room");
                string path = Path.Combine(folder, _DbRooms.Image);
                if (System.IO.File.Exists(path))
                {
                    System.IO.File.Delete(path);
                }
                _DbRooms.Image = await rooms.Photo.SaveFileAsync(folder);

            }
            else
            {
                rooms.Image = _DbRooms.Image;
            }

            #endregion
            var GroupStudent = new List<BookingRoom>();
            var groupStudent = new BookingRoom
            {
                RoomId = rooms.Id,
                BookingStatusId = GatId
            };
            GroupStudent.Add(groupStudent);

            _DbRooms.Name = rooms.Name;
            _DbRooms.Payment = rooms.Payment;
            bookingRoom.BookingStatusId = GatId;

            //_DbStudents.GroupStudent. = GatId;
            _DbRooms.RoomTypesId = CatId;

            //students.GroupStudentId = GatId;

            await _Db.SaveChangesAsync();
            return RedirectToAction("Index");
        }
        #endregion
        public async Task<IActionResult> Detail(int id)
        {
            var rooms = await _Db.Rooms
            .Include(s => s.BookingRooms)
            .ThenInclude(sg => sg.BookingStatus).Where(x => x.Id == id).FirstOrDefaultAsync();


            ViewBag.RoomTypes = await _Db.RoomTypes.ToListAsync();
            ViewBag.BookingRooms = await _Db.BookingRoom.ToListAsync();


            return View(rooms);
        }
        #region Activity
        public async Task<IActionResult> Activity(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            Room _DbRooms = await _Db.Rooms.FirstOrDefaultAsync(x => x.Id == id);
            if (_DbRooms == null)
            {
                return BadRequest();
            }
            if (_DbRooms.IsDeactive)
            {
                _DbRooms.IsDeactive = false;
            }
            else
            {
                _DbRooms.IsDeactive = true;
            }
            await _Db.SaveChangesAsync();
            return RedirectToAction("Index");
        }
        #endregion

        public async Task<IActionResult> ShowAllRooms()
        {

            var rooms = await _Db.Rooms
            .Include(s => s.BookingRooms)
            .ThenInclude(sg => sg.BookingStatus)
            .ThenInclude(s => s.RoomTypes)
            .Include(s => s.RoomTypes)
            .OrderByDescending(x => x.Id).ToListAsync();

            ViewBag.BookingStatus = await _Db.BookingStatus.ToListAsync();
            return View(rooms);
        }
        public IActionResult Error()
        {
            return View();
        }
    }
}

