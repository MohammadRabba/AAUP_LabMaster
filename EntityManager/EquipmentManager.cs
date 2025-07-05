using AAUP_LabMaster.Models;
using Microsoft.EntityFrameworkCore;

namespace AAUP_LabMaster.EntityManager
{
    public class EquipmentManager
    {
        private readonly ApplicationDbContext context;
        public EquipmentManager(ApplicationDbContext context)
        {
            this.context = context;
        }

        public List<Equipment> GetAllEquipments()
        {
            return context.Equipments.ToList();
        }
        public Equipment? GetEquipmentById(int id)
        {
            return context.Equipments.FirstOrDefault(e => e.Id == id);
        }
        public List<Equipment> GetAllEquipments(string searchString = null)
        {
            var query = context.Equipments.AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                searchString = searchString.ToLower();
                query = query.Where(e =>
                    e.Name.ToLower().Contains(searchString) ||
                    e.Description.ToLower().Contains(searchString));
            }

            return query.ToList();
        }
        public Equipment AddEquipment(Equipment equipment)
        {
            if (equipment.LabId == 0) // Basic validation if LabId is required
            {
                throw new ArgumentException("LabId must be provided for the equipment.");
            }
            context.Equipments.Add(equipment);
            context.SaveChanges();
            return equipment;
        }
        public bool UpdateEquipment(Equipment equipment)
        {
            if (equipment == null) return false;
            var existingEquipment = context.Equipments.FirstOrDefault(e => e.Id == equipment.Id);
            if (existingEquipment == null) return false;
            existingEquipment.Name = equipment.Name;
            existingEquipment.Description = equipment.Description;
            existingEquipment.Quantity = equipment.Quantity;
            existingEquipment.Price = equipment.Price;
            existingEquipment.status = equipment.status;
            existingEquipment.LabId = equipment.LabId;
            existingEquipment.ImagePath = equipment.ImagePath;
            existingEquipment.Link = equipment.Link;

            context.SaveChanges();
            return true;
        }
        public List<Equipment> GetEquipmentsByLabId(int labName)
        {
            return context.Equipments.Where(e => e.Lab.Id == labName).ToList();
        }
        public bool DeleteEquipment(int id)
        {
            var equipment = context.Equipments.FirstOrDefault(e => e.Id == id);
            if (equipment == null) return false;
            context.Equipments.Remove(equipment);
            context.SaveChanges();
            return true;
        }
    }
}
