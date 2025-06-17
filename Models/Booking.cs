using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;

namespace AAUP_LabMaster.Models
{
    public class Booking
    {
        public enum BookStatus
        {
            Approved, Pending, Rejected

        }
        public int Id { get; set; }
        public string Description { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        //public int LabId {  get; set; }
        //public Lab Lab { get; set; } 
        public int ClientId { get; set; }
        public Client Client { get; set; }
        public int EquipmentId { get; set; }
        public BookStatus status { get; set; }

        public Equipment Equipment { get; set; } 

        public string Notes { get; set; } = string.Empty;
        [Precision(18, 2)]

        public decimal Price { get; set; }
        public Booking() { }
        public Booking(string Desc, DateTime date,Client client,Equipment 
            equipments,string notes,decimal total) {
            if (equipments.status == Equipment.Availability.Available &&equipments.Quantity>1)
            {
                Date = date;
                //Lab = lab;
                Client = client;
                ClientId = client.Id;
                //LabId=lab.Id;
                Equipment = equipments;
                EquipmentId = equipments.Id;
                Notes = notes;
                Price = total;
                equipments.Quantity -= 1;
                status=BookStatus.Pending;
               
                }
            else if (equipments.Quantity == 1)
            {
                equipments.Quantity -= 1;
                equipments.status = Equipment.Availability.nonAvailable; Date = date;
                //Lab = lab;
                Client = client;
                ClientId = client.Id;
                //LabId=lab.Id;
                Equipment = equipments;
                EquipmentId = equipments.Id;
                Notes = notes;
                Price = total; status = BookStatus.Pending;

            }
            else

                     if (equipments.Quantity == 0)
            {
                equipments.status = Equipment.Availability.nonAvailable;
                Console.WriteLine("Equipment is not available for booking.");
            }


        }

    }
}
