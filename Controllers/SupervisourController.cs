using AAUP_LabMaster.EntityDTO;
using AAUP_LabMaster.EntityManager;
using AAUP_LabMaster.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static AAUP_LabMaster.Models.Equipment;

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
        public IActionResult AddNewEquipment(int? id) 
        {
            // --- FIX: ALWAYS ensure ViewBag.Labs is populated ---
            ViewBag.Labs = labManager.getAllLabs();
            if (ViewBag.Labs == null) // Defensive check, though getAllLabs should return empty list, not null
            {
                ViewBag.Labs = new List<AAUP_LabMaster.Models.Lab>();
            }
            // --- END FIX ---

            var model = new EquipmentDTO();
            if (id.HasValue && id.Value > 0) // Ensure id is valid
            {
                // Verify lab exists before setting the LabId in the model
                var labExists = labManager.GetLabById(id.Value);
                if (labExists != null)
                {
                    model.LabId = id.Value; // Set the LabId for pre-selection
                }
                else
                {
                    TempData["ErrorMessage"] = $"The specified Lab (ID: {id.Value}) was not found. Please select a lab.";
                    // Do not set model.LabId if the ID is invalid, so the dropdown remains "-- Select Lab --"
                }
            }
            return View(model); // Pass the model to the view
        }

        [HttpGet]
        public IActionResult UpdateEquipment(int? id)
        {
            ViewBag.Labs = labManager.getAllLabs() ?? new List<Lab>();

            if (!id.HasValue || id <= 0)
            {
                TempData["ErrorMessage"] = "Invalid equipment ID.";
                return RedirectToAction("Index");
            }

            var equipment = equipmentManager.GetEquipmentById(id.Value);
            if (equipment == null)
            {
                TempData["ErrorMessage"] = $"Equipment with ID {id.Value} not found.";
                return RedirectToAction("Index");
            }

            return View(equipment);
        }
        [HttpPost]
        public IActionResult UpdateEquipment(Equipment equipment)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Labs = labManager.getAllLabs();
                return View(equipment);
            }

            try
            {
                equipmentManager.UpdateEquipment(equipment);
                TempData["Message"] = "Equipment updated successfully!";
                return RedirectToAction("GetEquipmentByLabId", new { id = equipment.LabId });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error updating equipment: {ex.Message}";
                ViewBag.Labs = labManager.getAllLabs();
                return View(equipment);
            }
        }

        [HttpPost]
        public IActionResult AddNewEquipment(EquipmentDTO equipment)
        {
            if (!ModelState.IsValid)
            {
                // قم بإعادة البيانات مع رسالة الخطأ
                ViewBag.Labs = labManager.getAllLabs();
                return View(equipment);
            }

            try
            {
                var equip = new Equipment
                {
                    Name = equipment.Name,
                    Description = equipment.Description,
                    Quantity = equipment.Quantity,
                    Price = equipment.Price,
                    LabId = equipment.LabId, // Ensure LabId is set correctly
                    status = equipment.status // Assuming you want to set the default status to Available
                };
                var addedEquipment = equipmentManager.AddEquipment(equip); // عندك هذا الميثود
                TempData["Message"] = "Equipment added successfully!";
               return RedirectToAction("GetEquipmentByLabId", new { id = equipment.LabId });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error adding equipment: {ex.Message}";
                ViewBag.Labs = labManager.getAllLabs();
                return View(equipment);
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


        public IActionResult DeleteEquipment(int id) // Action name is DeleteEquipment, no need for ActionName attribute if matching route
        {
            int? labId = null;

            try
            {
                var equipment = equipmentManager.GetEquipmentById(id);
                if (equipment != null)
                {
                    labId = equipment.LabId;
                }

                var deleted = equipmentManager.DeleteEquipment(id);
                if (deleted)
                {
                    TempData["Message"] = "Equipment deleted successfully.";
                    if (labId.HasValue)
                    {
                        return RedirectToAction("GetEquipmentByLabId", new { id = labId.Value });
                    }
                    else
                    {
                        return RedirectToAction("GetAllEquipments");
                    }
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to delete equipment: Equipment not found or an error occurred.";
                    // Redirect back to where they were trying to delete from
                    if (labId.HasValue)
                    {
                        return RedirectToAction("GetEquipmentByLabId", new { id = labId.Value });
                    }
                    else
                    {
                        return RedirectToAction("GetAllEquipments");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting equipment (ID: {id}): {ex.Message}");
                TempData["ErrorMessage"] = $"An error occurred while deleting equipment: {ex.Message}";
                if (labId.HasValue)
                {
                    return RedirectToAction("GetEquipmentByLabId", new { id = labId.Value });
                }
                else
                {
                    return RedirectToAction("GetAllEquipments");
                }
            }
        }

    }
}
