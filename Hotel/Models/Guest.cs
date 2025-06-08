using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.ComponentModel;
using System;

namespace Hotel.Models
{
    public class Guest
    {
        public int Id { get; set; }
        [Required(ErrorMessage ="Bu xana boş ola bilməz!")]      
        public string Name { get; set; }
        [DataType(DataType.EmailAddress)]
        [Required(ErrorMessage = "Bu xana boş ola bilməz!")]
        public string Email { get; set; }
        [Required(ErrorMessage = "Bu xana boş ola bilməz!")]
        public string Image { get; set; }
        public bool IsDeactive { get; set; }
        [NotMapped]
        public IFormFile? Photo { get; set; }
        //public Positions Positions { get; set; }
        //public int PositionsId { get; set; }
        public Room Rooms { get; set; }
        public int RoomsId { get; set; }
        public bool IsCreated { get; set; }
        public DateTime CreatedTime { get; set; }
        public double Payment { get; set; }
        public bool IsRefunded { get; set; }
        public bool IsPaid { get; set; }
        public DateTime ArrivalDate {  get; set; }
        public DateTime DepartureDate {  get; set; }
        public int StayDuration { get; set; }


    }
}
