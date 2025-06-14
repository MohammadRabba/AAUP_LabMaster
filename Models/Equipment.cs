using Microsoft.EntityFrameworkCore;
using System;
namespace AAUP_LabMaster.Models
{
    public class Equipment
    {
        public enum Availability
        {
            Available, nonAvailable, notExsist

        }
        public int Id { get; set; }    
        public string Name { get; set; } = string.Empty;
        public int Quantity { get; set; }
        [Precision(18, 2)]

        public decimal Price { get; set; }
        public string Description { get; set; } = string.Empty;
        public Availability status{ get; set; } 
        public int LabId {  get; set; }
        public Lab Lab { get; set; }
        public  List<Booking> Bookings { get; set; }
        public Equipment() { }
        public Equipment(string name,int Quantity,decimal Price,string Description,Availability Status,Lab lab) {
            Name = name;
            this.Quantity=
                Quantity;
            Lab = lab;
            LabId = lab.Id;
            this.Price = Price;
            this.Description = Description;
            this.status = Status;
        }

    }
}