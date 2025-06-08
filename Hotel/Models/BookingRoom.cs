namespace Hotel.Models
{
    public class BookingRoom
    {

        public int Id { get; set; }
        public Room Rooms { get; set; }
        public int RoomId { get; set; }
        public BookingStatus BookingStatus { get; set; }
        public int BookingStatusId { get; set; }
    }
}
