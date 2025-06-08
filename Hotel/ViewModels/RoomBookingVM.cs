using Hotel.Models;

namespace Hotel.ViewModels
{
    public class RoomBookingVM
    {
        public Room Rooms { get; set; }
        public IEnumerable<RoomType> RoomTypes { get; set; }
        public IEnumerable<BookingStatus> BookingStatuses { get; set; }
        public BookingRoom BookingRoom { get; set; }
    }
}
