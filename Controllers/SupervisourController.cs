using AAUP_LabMaster.EntityDTO;
using AAUP_LabMaster.EntityManager;
using AAUP_LabMaster.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AAUP_LabMaster.Controllers
{

    public class SupervisourController : Controller {
        private readonly SupervisourManager superManager;
        private readonly BookingManager bookingManager;
        private readonly EquipmentManager equipmentManager;
        private readonly LabManager labManager;

        public SupervisourController(SupervisourManager superManager, BookingManager bookingManager, EquipmentManager equipmentManager, LabManager labManager)
        {
            this.superManager = superManager;
            this.bookingManager = bookingManager;
            this.equipmentManager = equipmentManager;
            this.labManager = labManager;
        }

        public List<Booking> getAllBookings()
        {
            return bookingManager.getAllBooking();
        }

        public IActionResult GetAllLabsBySupervisourId()
        {
            var labs = superManager.GetAllLabsbySupervisourId();
            return View(labs);
        }
        public List<Lab> GetAllLabs()
        {
            return labManager.getAllLabs();
        }
        public IActionResult ViewEquipment(int id)
        {
            var lab = equipmentManager.GetEquipmentsByLabId(id);
            if (lab == null)
            {
                return NotFound();
            }
            return View(lab);
        }

        [HttpGet]

        public IActionResult AddNewEquipment()
        {
            return View(new Equipment());
        }
        [HttpGet]
        public IActionResult AddNewEquipment(int? id) // 'id' from route, e.g., /Supervisour/AddNewEquipment/5
        {
            ViewBag.Labs = labManager.getAllLabs(); // Ensure labs are loaded for the dropdown

            var model = new Equipment();
            if (id.HasValue)
            {
                // Optional: Verify lab exists
                var labExists = labManager.GetLabById(id.Value);
                if (labExists != null)
                {
                    model.LabId = id.Value; // Set the LabId here for pre-selection
                }
                else
                {
                    TempData["ErrorMessage"] = $"The specified Lab (ID: {id.Value}) was not found.";
                    // Do not set model.LabId if the ID is invalid, so the dropdown remains "-- Select Lab --"
                }
            }
            return View(model); // Pass the model (potentially with pre-selected LabId) to the view
        }


        [HttpPost]
        // Fix 1: Corrected typo in action name to match Get action and form's asp-action.
        // Fix 2: Removed redundant 'int LabId' parameter. Equipment model already contains LabId.
        public IActionResult AddNewEquipment(Equipment equipment)
        {
            // Fix 4a: Re-populate ViewBag.Labs if returning the view due to ModelState errors
            ViewBag.Labs = labManager.getAllLabs();

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Please correct the errors and try again.";
                // Fix 4b: Return to the *same* view (AddNewEquipment) with the current 'equipment' model
                return View("AddNewEquipment", equipment);
            }

            try
            {
                // Fix 5: Ensure your AddEquipment method in EquipmentManager takes only 'Equipment'
                // and the LabId is correctly set within the 'equipment' object.
                var addedEquipment = equipmentManager.AddEquipment(equipment);

                TempData["Message"] = "Equipment added successfully.";
                // Fix 3: Redirect to GetEquipmentByLabId and pass the LabId
                return RedirectToAction("GetEquipmentByLabId", new { id = addedEquipment.LabId });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding equipment: {ex.Message}"); // Log the full error for debugging
                TempData["ErrorMessage"] = $"Error adding equipment: {ex.Message}";
                // Fix 4b: Return to the *same* view (AddNewEquipment) with the current 'equipment' model
                // Make sure ViewBag.Labs is re-populated before returning here (done above).
                return View("AddNewEquipment", equipment);
            }
        }

    
        public List<Equipment> GetAllEquipments()
        {
            return equipmentManager.GetAllEquipments();
        }
        [HttpGet]
        public IActionResult GetEquipmentByLabId(int? id) // Make 'id' nullable to handle cases where it's not provided
        {
            List<Equipment> equipmentList;
            string labName = "All Labs"; // Default label

            if (id.HasValue)
            {
                // Check if the Lab actually exists
                var lab = labManager.GetLabById(id.Value); // Assuming GetLabById returns Lab object or null
                if (lab != null)
                {
                    equipmentList = equipmentManager.GetEquipmentsByLabId(id.Value); // Fetch equipment for specific lab
                    labName = lab.Name; // Set the actual lab name
                }
                else
                {
                    // If ID is provided but lab doesn't exist
                    TempData["ErrorMessage"] = $"Lab with ID {id.Value} not found.";
                    equipmentList = new List<Equipment>(); // Return empty list
                    labName = "Unknown Lab";
                }
            }
            else
            {
                // If no ID is provided, typically you'd show ALL equipment
                equipmentList = equipmentManager.GetAllEquipments(); // You need to implement this in EquipmentManager
                TempData["Message"] = "Displaying equipment from all labs.";
            }

            ViewBag.CurrentLabId = id; // Pass the id to the view, even if null, for the "Add Equipment" link
            ViewBag.CurrentLabName = labName; // Pass the lab name for display

            return View(equipmentList); // Pass the list of equipment as the model
        }

        public IActionResult UpdateEquipment(Equipment equipment)
        {
            var updated= equipmentManager.UpdateEquipment(equipment);
            return updated ? RedirectToAction("GetAllEquipments") : View("Error", "Failed to update equipment.");
        }
        public IActionResult DeleteEquipment(int id)
        {
           var deleted= equipmentManager.DeleteEquipment(id);
            return deleted ? RedirectToAction("GetAllEquipments") : View("Error", "Failed to delete equipment.");
        }
    }
}
