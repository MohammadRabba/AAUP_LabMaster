using AAUP_LabMaster.EntityDTO;
using AAUP_LabMaster.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AAUP_LabMaster.EntityManager
{
    public class LabManager
    {
        private readonly ApplicationDbContext context; private readonly SupervisourManager superManager;
        private readonly EquipmentManager equipManager;

        public LabManager(ApplicationDbContext context, SupervisourManager superManager,EquipmentManager equipmentManager)
        {
            this.context = context;
            this.superManager = superManager;
            this.equipManager = equipmentManager;
        }

        public void AddLab(LabDTO lab)
        {
             var supervisor = superManager.GetAllSupervisours()
                                 .FirstOrDefault(s => s.FullName == lab.SelectedSupervisorId);

            Console.WriteLine($"Supervisor found: {supervisor?.FullName}");

            if (supervisor == null)
            {
                throw new Exception($"Supervisor with Id '{lab.SelectedSupervisorId}' not found.");
            }

            var newlab = new Lab
            {
                Name = lab.Name,
                Description = lab.Description??"",
                SupervisorId = supervisor.Id,
                Supervisour = supervisor
            };
         
            context.Labs.Add(newlab);
            context.SaveChanges();
        }
        public Lab GetLabById(int id)
        {
            var lab = context.Labs
                .Include(l => l.Supervisour)
                .Include(l => l.Equipment)
                .FirstOrDefault(l => l.Id == id&&l.Name!="Empty Lab");

            return lab;
        }
        public List<Lab> getAllLabs()
        {
            

            return context.Labs
              .Include(l => l.Supervisour).Where(l => l.Name != "Empty Lab")
              .ToList();
        }
        public bool UpdateLab(LabDTO labDto)
        {
            var existingLab = context.Labs
                                    .Include(l => l.Equipment) 
                                    .FirstOrDefault(l => l.Name == labDto.Name&&l.Name!="Empty Lab");

            if (existingLab == null)
            {
                return false;
            }

            existingLab.Name = labDto.Name;
            existingLab.Description = labDto.Description ?? "";


            var newSupervisor = context.Supervisours.FirstOrDefault(s => s.FullName == labDto.SelectedSupervisorId);
            if (newSupervisor != null)
            {
                existingLab.SupervisorId = newSupervisor.Id;
                existingLab.Supervisour = newSupervisor;
            }
           

            context.SaveChanges();
            return true;
        }
        public async Task RemoveLabAsync(int labId)
        {
            var lab = await context.Labs.FindAsync(labId);
            if (lab != null)
            {
                context.Labs.Remove(lab);
                await context.SaveChangesAsync();
            }
        }
        public void RemoveLab(int id)
        {
            var exsistingLab = context.Labs.Include(l => l.Equipment).FirstOrDefault(x => x.Id == id);
            var EmptyLab = context.Labs.FirstOrDefault(x => x.Name == "Empty Lab"&&x.Description=="Empty Lab"&&x.SupervisorId==exsistingLab.SupervisorId);

            if (exsistingLab != null)
            {
                foreach (var equipment in exsistingLab.Equipment.ToList())
                {
                    equipment.LabId = EmptyLab.Id; 
                    equipManager.UpdateEquipment (equipment);            
                }
                context.Labs.Remove(exsistingLab);
                context.SaveChanges();
                Console.WriteLine("Lab Removed successfully.");
            }

            else
            {
                Console.WriteLine("Lab Dont Exsist.");

            }
        }
             
    }
}
