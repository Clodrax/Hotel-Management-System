using Hotel.DAL;
using Hotel.Helper;
using Hotel.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Hotel.Controllers
{
    [Authorize(Roles = "Receptionist,Admin")]
    public class DashBoardController : Controller
    {
        private readonly AppDbContext _Db;
        public DashBoardController(AppDbContext Db)
        {
            _Db = Db;
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


        public IActionResult Error()
        {
            return View();
        }
    }
}
