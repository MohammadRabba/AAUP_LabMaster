using AAUP_LabMaster.Models;

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
            existingEquipment.status = equipment.status;
            existingEquipment.LabId = equipment.LabId;
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
