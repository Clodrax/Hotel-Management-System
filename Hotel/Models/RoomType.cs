using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Hotel.Models
{
    public class RoomType
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Bu xana boş ola bilməz!")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Bu xana boş ola bilməz!")]
        public double Mark { get; set; }
        [Required(ErrorMessage = "Bu xana boş ola bilməz!")]
        public bool IsDeactive { get; set; }
        public List<Room> Rooms { get; set; }

    }
}
