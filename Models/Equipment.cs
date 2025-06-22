using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;
namespace AAUP_LabMaster.Models
{
    public class Equipment
    {
        public enum Availability
        {
            Available, nonAvailable, notExsist
        }

        public int Id { get; set; }

        [Required(ErrorMessage = "Name is required")]
        public string Name { get; set; } = string.Empty;

        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        public int Quantity { get; set; }

        [Precision(18, 2)]
        public decimal Price { get; set; }

        public string Description { get; set; } = string.Empty;

        public Availability status { get; set; }

        [Required(ErrorMessage = "Lab selection is required")]
        public int LabId { get; set; }

        [ValidateNever]
        public Lab Lab { get; set; }

        [ValidateNever]
        public List<Booking> Bookings { get; set; }
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