using System;
using System.ComponentModel.DataAnnotations;

namespace FikirHavuzu.Entities
{
    public class PurchasedItem
    {
        [Key]
        public int Id { get; set; }

        public int UserId { get; set; }
        public virtual User User { get; set; }
        public int ProductId { get; set; } 
        public virtual Product Product { get; set; }

        public DateTime PurchaseDate { get; set; } = DateTime.Now;
    }
}