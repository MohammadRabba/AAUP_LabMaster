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
                Description = lab.Description??"",
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
            var lab = context.Labs
                .Include(l => l.Supervisour)
                .Include(l => l.Equipment)
                .FirstOrDefault(l => l.Id == id);

            return lab;
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
            existingLab.Description = labDto.Description ?? "";

            // Supervisor update logic (based on your chosen approach - editable or not)
            // Assuming the EditLab form sends SelectedSupervisorId as FullName if editable,
            // or originalLab.Supervisour?.FullName if not editable from that specific form.
            var newSupervisor = context.Supervisours.FirstOrDefault(s => s.FullName == labDto.SelectedSupervisorId);
            if (newSupervisor != null)
            {
                existingLab.SupervisorId = newSupervisor.Id;
                existingLab.Supervisour = newSupervisor;
            }
           

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
