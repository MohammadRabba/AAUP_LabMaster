using AAUP_LabMaster.EntityManager;
using AAUP_LabMaster.Models;
using Microsoft.AspNetCore.Mvc;

namespace AAUP_LabMaster.Controllers
{
   
        public class SupervisourController : Controller {
        private readonly SupervisourManager superManager;
        private readonly BookingManager bookingManager;
        private readonly EquipmentManager equipmentManager;
        public SupervisourController(SupervisourManager superManager, BookingManager bookingManager,EquipmentManager equipmentManager)
        {
            this.superManager = superManager;
            this.bookingManager = bookingManager;
            this.equipmentManager = equipmentManager;
        }

        public List<Booking> getAllBookings()
        {
            return bookingManager.getAllBooking();
        }


    }
}
