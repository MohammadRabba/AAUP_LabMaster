using AAUP_LabMaster.EntityDTO;
using AAUP_LabMaster.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AAUP_LabMaster.EntityManager
{
    public class LabManager
    {
        private readonly ApplicationDbContext context; private readonly SupervisourManager superManager;

        public LabManager(ApplicationDbContext context, SupervisourManager superManager)
        {
            this.context = context;
            this.superManager = superManager;
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
                Description = lab.Description,
                SupervisorId = supervisor.Id,
                Supervisour = supervisor
            };
            //if (equipments != null && equipments.Any())
            //{
            //    newlab.Equipment = equipments
            //                        .Where(eName => !string.IsNullOrWhiteSpace(eName)) 
            //                        .Select(eName => new Equipment { Name = eName })
            //                        .ToList();
            //}

            context.Labs.Add(newlab);
            context.SaveChanges();
        }
        public Lab GetLabById(int id)
        {
            return context.Labs
                .Include(l => l.Supervisour) 
                .Include(l => l.Equipment) 
                .FirstOrDefault(l => l.Id == id);
        }
        public List<Lab> getAllLabs()
        {
            

            return context.Labs
              .Include(l => l.Supervisour) 
              .ToList();
        }
        public bool UpdateLab(LabDTO labDto)
        {
            var existingLab = context.Labs
                                    .Include(l => l.Equipment) 
                                    .FirstOrDefault(l => l.Name == labDto.Name);

            if (existingLab == null)
            {
                return false;
            }

            existingLab.Name = labDto.Name;
            existingLab.Description = labDto.Description;

            // Supervisor update logic (based on your chosen approach - editable or not)
            // Assuming the EditLab form sends SelectedSupervisorId as FullName if editable,
            // or originalLab.Supervisour?.FullName if not editable from that specific form.
            var newSupervisor = context.Supervisours.FirstOrDefault(s => s.FullName == labDto.SelectedSupervisorId);
            if (newSupervisor != null)
            {
                existingLab.SupervisorId = newSupervisor.Id;
                existingLab.Supervisour = newSupervisor;
            }
            // If newSupervisor is null, it means the submitted name was invalid/empty.
            // In this case, the existingLab.SupervisorId will *not* be changed, preserving the current one.


            // Equipment update logic
            // 1. Filter the incoming equipment names from the
            // to remove any null, empty, or whitespace strings.
            //var currentEquipmentNames = (labDto.EquipmentNames ?? new List<string>())
            //                                .Where(eName => !string.IsNullOrWhiteSpace(eName)) // CRITICAL: Filters out invalid names here
            //                                .ToList();

            //var equipmentToRemove = existingLab.Equipment
            //                                   .Where(e => !currentEquipmentNames.Contains(e.Name))
            //                                   .ToList();
            //context.Equipments.RemoveRange(equipmentToRemove);

            // 3. Determine which new equipment items need to be added to the database.
            // These are items in 'currentEquipmentNames' that are NOT already in the existing equipment list for this lab.
            //var existingEquipmentNames = existingLab.Equipment.Select(e => e.Name).ToList();
            //var equipmentToAdd = currentEquipmentNames
            //                    .Where(eName => !existingEquipmentNames.Contains(eName)) // No need for IsNullOrWhiteSpace here, it was filtered in step 1
            //                    .Select(eName => new Equipment { Name = eName, LabId = existingLab.Id })
            //                    .ToList();
            //context.Equipments.AddRange(equipmentToAdd);

            context.SaveChanges();
            return true;
        }
        public void RemoveLab(int id)
        {
            var exsistingLab = context.Labs.FirstOrDefault(x => x.Id == id);
            if (exsistingLab != null)
            {
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
