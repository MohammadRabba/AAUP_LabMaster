using AAUP_LabMaster.EntityDTO;
using AAUP_LabMaster.EntityManager;
using AAUP_LabMaster.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static AAUP_LabMaster.Models.Equipment;
using Microsoft.AspNetCore.Hosting;

namespace AAUP_LabMaster.Controllers
{

    public class SupervisourController : Controller
    {
        private readonly SupervisourManager superManager;
        private readonly BookingManager bookingManager;
        private readonly EquipmentManager equipmentManager;
        private readonly LabManager labManager;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public SupervisourController(SupervisourManager superManager, BookingManager bookingManager, EquipmentManager equipmentManager, LabManager labManager, IWebHostEnvironment webHostEnvironment)
        {
            this.superManager = superManager;
            this.bookingManager = bookingManager;
            this.equipmentManager = equipmentManager;
            this.labManager = labManager;
            _webHostEnvironment = webHostEnvironment;
        }

        public IActionResult getAllBookingsSupervisour()
        {
            var labs = superManager.getAllBookingBySupervisourId();

            return View(labs);
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
        [ValidateAntiForgeryToken] // Always recommended for POST actions that modify data
        public async Task<IActionResult> UpdateEquipment(EquipmentDTO equipmentDto)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Labs = labManager.getAllLabs();
                // Pass the DTO back to the view so form fields are repopulated with user's input
                return View(equipmentDto);
            }

            try
            {
                var existingEquipment = equipmentManager.GetEquipmentById(equipmentDto.Id);
                if (existingEquipment == null)
                {
                    TempData["ErrorMessage"] = $"Equipment with ID {equipmentDto.Id} not found.";
                    // Redirecting to list view if equipment is not found
                    return RedirectToAction("GetEquipmentByLabId", new { id = equipmentDto.LabId });
                }

                // --- 1. Handle Image File Upload ---
                if (equipmentDto.ImageFile != null && equipmentDto.ImageFile.Length > 0)
                {
                    // Delete old image file if it exists and a new one is uploaded
                    if (!string.IsNullOrEmpty(existingEquipment.ImagePath))
                    {
                        // Construct the full physical path to the old image file
                        // Use TrimStart('/') to remove leading slash before combining with WebRootPath
                        var oldImageFullPath = Path.Combine(_webHostEnvironment.WebRootPath, existingEquipment.ImagePath.TrimStart('/'));
                        if (System.IO.File.Exists(oldImageFullPath))
                        {
                            System.IO.File.Delete(oldImageFullPath);
                        }
                    }

                    // Define the upload folder path. Use Path.Combine correctly.
                    var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "img"); // This will resolve to wwwroot/img
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    // Generate a unique file name to prevent overwriting issues
                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(equipmentDto.ImageFile.FileName);
                    var newFilePath = Path.Combine(uploadsFolder, uniqueFileName);

                    // Save the new file
                    using (var fileStream = new FileStream(newFilePath, FileMode.Create))
                    {
                        await equipmentDto.ImageFile.CopyToAsync(fileStream);
                    }

                    existingEquipment.ImagePath = "/img/" + uniqueFileName; 
                }
              


                existingEquipment.Name = equipmentDto.Name;
                existingEquipment.Description = equipmentDto.Description;
                existingEquipment.Quantity = equipmentDto.Quantity;
                existingEquipment.Price = equipmentDto.Price;
                existingEquipment.status = equipmentDto.status;
                existingEquipment.LabId = equipmentDto.LabId;

          
                existingEquipment.Link = equipmentDto.linkUrl; 


                // --- 4. Persist Changes to the Database ---
                equipmentManager.UpdateEquipment(existingEquipment); // This should update the tracked entity

                TempData["Message"] = "Equipment updated successfully!";
                return RedirectToAction("GetEquipmentByLabId", new { id = existingEquipment.LabId });
            }
            catch (Exception ex)
            {
                // Log the full exception for debugging, not just the message
                // _logger.LogError(ex, "Error updating equipment with ID {EquipmentId}", equipmentDto.Id);

                TempData["ErrorMessage"] = $"Error updating equipment: {ex.Message}";
                ViewBag.Labs = labManager.getAllLabs();
                // Pass the DTO back to the view in case of error
                return View(equipmentDto);
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken] // Always recommended for POST actions that modify data
        public async Task<IActionResult> AddNewEquipmentAsync(EquipmentDTO equipmentDto) // Renamed parameter for clarity
        {
            if (!ModelState.IsValid)
            {
                // قم بإعادة البيانات مع رسالة الخطأ
                ViewBag.Labs = labManager.getAllLabs();
                return View(equipmentDto); // Pass the DTO back to the view to repopulate fields
            }

            try
            {
                string? imagePath = null; 

                // Handle Image File Upload
                if (equipmentDto.ImageFile != null && equipmentDto.ImageFile.Length > 0)
                {
                    var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "img"); 

                    if (!Directory.Exists(uploadsFolder))
                        Directory.CreateDirectory(uploadsFolder);

                    // Use Path.GetFileName to ensure only the file name   from the client's input
                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(equipmentDto.ImageFile.FileName);
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await equipmentDto.ImageFile.CopyToAsync(fileStream);
                    }

                    // Set the imagePath for the Equipment entity
                    imagePath = "/img/" + uniqueFileName; // This is the relative URL for the database
                }

                // Create a new Equipment entity from the DTO
                var newEquipment = new Equipment
                {
                    Name = equipmentDto.Name,
                    Description = equipmentDto.Description,
                    Quantity = equipmentDto.Quantity,
                    Price = equipmentDto.Price,
                    status = equipmentDto.status,
                    LabId = equipmentDto.LabId,
                    ImagePath = imagePath, 
                    Link = equipmentDto.linkUrl
                };

                var addedEquipment = equipmentManager.AddEquipment(newEquipment); // Call your AddEquipment method
                TempData["Message"] = "Equipment added successfully!";
                return RedirectToAction("GetEquipmentByLabId", new { id = addedEquipment.LabId }); // Redirect using the ID of the newly added equipment
            }
            catch (Exception ex)
            {
                // Log the full exception for debugging, not just the message
                // _logger.LogError(ex, "Error adding new equipment.");

                TempData["ErrorMessage"] = $"Error adding equipment: {ex.Message}";
                ViewBag.Labs = labManager.getAllLabs();
                return View(equipmentDto); // Return the DTO to repopulate the form
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

        public IActionResult updateBookingStatus(int id, Booking.BookStatus status)
        {
            try
            {
                bookingManager.updateBookingStatus(id, status);
                TempData["Message"] = "Booking status updated successfully.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error updating booking status: {ex.Message}";
            }
            // Redirect to the bookings page or wherever appropriate
            return RedirectToAction("getAllBookingsSupervisour");
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


