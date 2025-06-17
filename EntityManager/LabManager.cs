using AAUP_LabMaster.EntityDTO;
using AAUP_LabMaster.Models;

namespace AAUP_LabMaster.EntityManager
{
    public class LabManager
    {
        private readonly ApplicationDbContext context;
public LabManager(ApplicationDbContext context)
        {
            this.context = context;
        }

        public void AddLab(LabDTO lab, string SupervisourName, List<string> equipments)
        {


            var supervisor = context.Supervisours.FirstOrDefault(x => x.FullName == SupervisourName);
            var newlab = new Lab { Name = lab.Name, Description = lab.Description, Supervisour = supervisor, SupervisorId = supervisor.Id };

            foreach (var equip in equipments)
            {
                var selectedEquip = context.Equipments.FirstOrDefault(x => x.Name == equip);
                newlab.Equipment.Add(selectedEquip);
            }
            context.Labs.Add(newlab);
            context.SaveChanges();
            Console.WriteLine("Lab added successfully.");
        }
        public List<Lab> getAllLabs()
        {

            return context.Labs.ToList();
        }
        public bool UpdateLab(int id, LabDTO lab)
        {

            {

                var newLab = context.Labs.FirstOrDefault(x => x.Id == id);
                if (newLab != null)
                {
                    newLab.Name = lab.Name;
                    newLab.Description = lab.Description;

                    context.SaveChanges();
                    return true;
                }
                return false;
            }
        }
        public void RemoveLab(string LabName)
        {
            var exsistingLab = context.Labs.FirstOrDefault(x => x.Name == LabName);
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
