using Hotel.DAL;
using Hotel.Helper;
using Hotel.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Hotel.Controllers
{
    [Authorize(Roles = "Receptionist,Admin")]

    public class GuestsController : Controller
    {
        private readonly AppDbContext _Db;
        private readonly IWebHostEnvironment _env;
        public GuestsController(AppDbContext Db, IWebHostEnvironment env, ILogger<GuestsController> logger = null)
        {

            _Db = Db;
            _env = env;
        }
        public async Task<IActionResult> Index()
        {
            List<Guest> guests = await _Db.Guests.Include(x => x.Rooms).OrderByDescending(x => x.Id).ToListAsync();
            ViewBag.Rooms = await _Db.Rooms.ToListAsync();
            return View(guests);
        }
        #region create
        public async Task<IActionResult> Create()
        {
            var freeRooms = await _Db.BookingRoom
            .Where(br => br.BookingStatus.Name == "Free")
            .Include(br => br.Rooms)
            .Select(br => br.Rooms)
            .ToListAsync();

            ViewBag.Rooms = freeRooms.Select(r => new SelectListItem
            {
                Value = r.Id.ToString(),
                Text = r.Name
            }).ToList();
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Guest guest, int CatId, string daterange)
        {
            var freeRooms = await _Db.BookingRoom
                .Where(br => br.BookingStatus.Name == "Free")
                .Include(br => br.Rooms)
                .Select(br => br.Rooms)
                .ToListAsync();

            ViewBag.Rooms = freeRooms.Select(r => new SelectListItem
            {
                Value = r.Id.ToString(),
                Text = r.Name
            }).ToList();
            guest.CreatedTime = DateTime.UtcNow.AddHours(4);

            // Parse ArrivalDate and DepartureDate from the daterange string
            var dates = daterange.Split(" - ");
            guest.ArrivalDate = DateTime.ParseExact(dates[0], "yyyy-MM-dd", null);
            guest.DepartureDate = DateTime.ParseExact(dates[1], "yyyy-MM-dd", null);

            if (guest.ArrivalDate > guest.DepartureDate)
            {
                ModelState.AddModelError("DepartureDate", "Departure Date must be after Arrival Date.");
                return View(guest);
            }
            if (guest.CreatedTime.Day > guest.ArrivalDate.Day)
            {
                ModelState.AddModelError("ArrivalDate", "Arrival Date must be after Current Date");
                return View(guest);
            }

            // Save Image
            #region Save Image

            if (guest.Photo != null)
            {
                if (!guest.Photo.IsImage())
                {
                    ModelState.AddModelError("Photo", "Please select image type");
                    return View(guest);
                }
                if (guest.Photo == null)
                {
                    ModelState.AddModelError("Photo", "max 1mb !!");
                    return View(guest);
                }

                string folder = Path.Combine(_env.WebRootPath, "images", "guest");
                guest.Image = await guest.Photo.SaveFileAsync(folder);
            }
            else
            {
                guest.Image = "guestpfp.jpg";
            }

            #endregion

            TimeSpan stayDuration = guest.DepartureDate - guest.ArrivalDate;
            guest.StayDuration = (int)stayDuration.TotalDays;
            guest.RoomsId = CatId;
            var room = await _Db.Rooms.FindAsync(guest.RoomsId);
            if (room != null)
            {
                guest.Payment = room.Payment * guest.StayDuration;
            }

            await _Db.Guests.AddAsync(guest);
            await _Db.SaveChangesAsync();
            var bookingRoom = await _Db.BookingRoom.FirstOrDefaultAsync(br => br.RoomId == CatId);
            if (bookingRoom != null)
            {
                var reservedStatus = await _Db.BookingStatus.FirstOrDefaultAsync(bs => bs.Name == "Reserved");
                if (reservedStatus != null)
                {
                    bookingRoom.BookingStatusId = reservedStatus.Id;
                    _Db.BookingRoom.Update(bookingRoom);
                    await _Db.SaveChangesAsync();
                }
            }

            return RedirectToAction("Index");
        }

        #endregion
        #region Update
        // GET: Guests/Update/{id}
        public async Task<IActionResult> Update(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Guest guest = await _Db.Guests
                .Include(g => g.Rooms)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (guest == null)
            {
                return BadRequest();
            }

            var reservedRoomId = guest.RoomsId;
            var freeRooms = await _Db.BookingRoom
                .Where(br => br.BookingStatus.Name == "Free" || br.RoomId == reservedRoomId)
                .Include(br => br.Rooms)
                .Select(br => br.Rooms)
                .ToListAsync();

            ViewBag.Rooms = freeRooms.Select(r => new SelectListItem
            {
                Value = r.Id.ToString(),
                Text = r.Name
            }).ToList();

            return View(guest);
        }

        // POST: Guests/Update/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(int id, Guest guest, int CatId, string daterange)
        {
            if (id != guest.Id)
            {
                return BadRequest();
            }

            Guest DbGuest = await _Db.Guests.FirstOrDefaultAsync(g => g.Id == id);
            if (DbGuest == null)
            {
                return NotFound();
            }

            var reservedRoomId = DbGuest.RoomsId;
            var freeRooms = await _Db.BookingRoom
                .Where(br => br.BookingStatus.Name == "Free" || br.RoomId == reservedRoomId)
                .Include(br => br.Rooms)
                .Select(br => br.Rooms)
                .ToListAsync();

            ViewBag.Rooms = freeRooms.Select(r => new SelectListItem
            {
                Value = r.Id.ToString(),
                Text = r.Name
            }).ToList();

            var dates = daterange.Split(" - ");
            guest.ArrivalDate = DateTime.ParseExact(dates[0], "yyyy-MM-dd", null);
            guest.DepartureDate = DateTime.ParseExact(dates[1], "yyyy-MM-dd", null);
            if (guest.ArrivalDate > guest.DepartureDate)
            {
                ModelState.AddModelError("DepartureDate", "Departure Date must be after Arrival Date.");
                return View(guest);
            }

            if (guest.CreatedTime.Day > guest.ArrivalDate.Day)
            {
                ModelState.AddModelError("ArrivalDate", "Arrival Date must be after Current Date");
                return View(guest);
            }

            var currentRoomBooking = await _Db.BookingRoom
                .FirstOrDefaultAsync(br => br.RoomId == DbGuest.RoomsId);

            if (currentRoomBooking != null)
            {
                var freeStatus = await _Db.BookingStatus
                    .FirstOrDefaultAsync(bs => bs.Name == "Free");

                if (freeStatus != null)
                {
                    currentRoomBooking.BookingStatusId = freeStatus.Id;
                }
            }

            var newRoomBooking = await _Db.BookingRoom
                .FirstOrDefaultAsync(br => br.RoomId == CatId);

            if (newRoomBooking != null)
            {
                var reservedStatus = await _Db.BookingStatus
                    .FirstOrDefaultAsync(bs => bs.Name == "Reserved");

                if (reservedStatus != null)
                {
                    newRoomBooking.BookingStatusId = reservedStatus.Id;
                }
            }

            if (guest.Photo != null)
            {
                if (!guest.Photo.IsImage())
                {
                    ModelState.AddModelError("Photo", "Please select a valid image.");
                    return View(guest);
                }
                if (!guest.Photo.IsMb())
                {
                    ModelState.AddModelError("Photo", "Maximum file size is 1MB.");
                    return View(guest);
                }

                string folder = Path.Combine(_env.WebRootPath, "images", "guest");
                string oldImagePath = Path.Combine(folder, DbGuest.Image);
                if (System.IO.File.Exists(oldImagePath))
                {
                    System.IO.File.Delete(oldImagePath);
                }
                DbGuest.Image = await guest.Photo.SaveFileAsync(folder);
            }

            DbGuest.Name = guest.Name;
            DbGuest.Email = guest.Email;
            DbGuest.RoomsId = CatId;
            DbGuest.ArrivalDate = guest.ArrivalDate;
            DbGuest.DepartureDate = guest.DepartureDate;
            TimeSpan stayDuration = guest.DepartureDate - guest.ArrivalDate;
            DbGuest.StayDuration = (int)stayDuration.TotalDays;

            var room = await _Db.Rooms.FindAsync(CatId);
            if (room != null)
            {
                if (room.Payment == null)
                {
                    ModelState.AddModelError("Payment", "Room payment is not set. Please check the database.");
                    return View(guest);
                }

                DbGuest.Payment = room.Payment * DbGuest.StayDuration;
            }
            else
            {
                ModelState.AddModelError("Room", "Room not found. Please check the Room ID.");
                return View(guest);
            }

            await _Db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        #endregion
        public async Task<IActionResult> Detail(int id)
        {
            Guest guest = await _Db.Guests.FindAsync(id);
            ViewBag.Rooms = await _Db.Rooms.ToListAsync();
            return View(guest);
        }
        #region Activity
        public async Task<IActionResult> Activity(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            Guest _DbGuests = await _Db.Guests.FirstOrDefaultAsync(x => x.Id == id);
            if (_DbGuests == null)
            {
                return BadRequest();
            }
            if (_DbGuests.IsDeactive)
            {
                _DbGuests.IsDeactive = false;
            }
            else
            {
                _DbGuests.IsDeactive = true;
            }
            await _Db.SaveChangesAsync();
            return RedirectToAction("Index");
        }
        #endregion
        public async Task<IActionResult> SendGuestsSms(int id)
        {
            var guest = _Db.Guests.Where(x => x.Id == id).FirstOrDefaultAsync();

            return View(guest);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendGuestsSms(int id, string subject, string text)
        {
            var teachers = _Db.Guests.Where(x => x.Id == id).FirstOrDefaultAsync();
            var email = teachers.Result.Email;
            await Sms.SendMailAsync(subject, text, email);

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> SendAllGuestsSms()
        {

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendAllGuestsSms(string subject, string text)
        {
            List<Guest> guests = await _Db.Guests.ToListAsync();

            foreach (var guest in guests)
            {
                await Sms.SendMailAsync(subject, text, guest.Email);
            }

            return RedirectToAction("Index");
        }
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var guest = await _Db.Guests
                .FirstOrDefaultAsync(m => m.Id == id);
            if (guest == null)
            {
                return NotFound();
            }

            return View(guest);
        }
        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]

        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var guest = await _Db.Guests.FindAsync(id);
            if (guest != null)
            {
                _Db.Guests.Remove(guest);
                var bookingRoom = await _Db.BookingRoom
                    .FirstOrDefaultAsync(br => br.RoomId == guest.RoomsId && br.BookingStatus.Name == "Reserved");

                if (bookingRoom != null)
                {
                    var freeStatus = await _Db.BookingStatus
                        .FirstOrDefaultAsync(bs => bs.Name == "Free");

                    if (freeStatus != null)
                    {
                        bookingRoom.BookingStatusId = freeStatus.Id;
                        _Db.BookingRoom.Update(bookingRoom);
                    }
                }
                string folder = Path.Combine(_env.WebRootPath, "images", "guest");
                string path = Path.Combine(folder, guest.Image);
                if (System.IO.File.Exists(path))
                {
                    if (guest.Image == "guestpfp.jpg")
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
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Refund(int id)
        {
            var guest = await _Db.Guests.FindAsync(id);
            Cost cost = new Cost()
            {
                Amount = (guest.Payment) / 2,
                CreateTime = DateTime.UtcNow.AddHours(4),
                By = guest.Name,
                Description = $"From Refund By {guest.Name}"
            };
            Budget budget = await _Db.Budgets.FirstOrDefaultAsync();
            budget.LastModifiedDescription = cost.Description;
            budget.LastModifiedDate = DateTime.UtcNow.AddHours(4);
            budget.LastModifiedAmount = cost.Amount;
            budget.LastModifiedBy = cost.By;
            budget.TotalBudget -= cost.Amount;
            guest.IsRefunded = true;
            await _Db.Costs.AddAsync(cost);
            await _Db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> Payment(int id)
        {
            var guest = await _Db.Guests.FindAsync(id);
            Benefit benefit = new Benefit()
            {
                CreateTime = DateTime.UtcNow.AddHours(4),
                Amount = guest.Payment,
                By = guest.Name,
                Description = "From room booking"
            };
            Budget budget = await _Db.Budgets.FirstOrDefaultAsync();
            budget.LastModifiedDescription = benefit.Description;
            budget.LastModifiedDate = DateTime.UtcNow.AddHours(4);
            budget.LastModifiedAmount = benefit.Amount;
            budget.LastModifiedBy = benefit.By;
            budget.TotalBudget += benefit.Amount;
            guest.IsPaid = true;
            await _Db.Benefits.AddAsync(benefit);
            await _Db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

    }
}
