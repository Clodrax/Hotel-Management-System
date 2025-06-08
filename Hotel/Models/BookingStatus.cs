using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hotel.Models
{
    public class BookingStatus
    {
        public int Id { get; set; }
        public string Name { get; set; }
        [Required(ErrorMessage = "Bu xana boş ola bilməz!")]
        public List<BookingRoom> BookingRooms { get; set; }
        //public List<TeacherGroup> TeacherGroups { get; set; }
        public bool IsDeactive { get; set; }

        public RoomType RoomTypes { get; set; }

        public int? RoomTypesId { get; set; }
        
    }
}
