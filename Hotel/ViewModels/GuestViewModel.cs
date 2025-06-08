using Hotel.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Hotel.ViewModels
{
    public class GuestViewModel
    {
        public Guest Guest { get; set; }
        public List<SelectListItem> FreeRooms { get; set; }
        public SelectListItem SelectedRoom { get; set; }
    }

}
