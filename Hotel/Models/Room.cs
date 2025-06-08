using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations.Schema;
using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Storage;

namespace Hotel.Models
{
    public class Room
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Bu xana boş ola bilməz!")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Bu xana boş ola bilməz!")]
        public string Image { get; set; }

        [NotMapped]
        public IFormFile? Photo { get; set; }

        public bool IsDeactive { get; set; }
        public RoomType RoomTypes { get; set; }
        public int RoomTypesId { get; set; }
       
        public List<BookingRoom> BookingRooms { get; set; }
        [Required(ErrorMessage = "Bu xana boş ola bilməz!!")]
        public double Payment { get; set; }
        public List<Guest> Guests { get; set; }


    }
}
