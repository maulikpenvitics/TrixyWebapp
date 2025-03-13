using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Models
{
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId Id { get; set; }
        [Required]
        public string? Firstname { get; set; }
        [Required]
        public string? Lastname { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string? Email { get; set; }
        [Required(ErrorMessage = "Password is required")]
        public string? Password { get; set; }
        [Required]
        public string? Role { get; set; }
        public bool? Status { get; set; }
        public string? ProfileImageUrl { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime? CreatedDate { get; set; }
        public string? CreatedBy { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime? UpdatedDate { get; set; }
        public string? UpdatedBy { get; set; }
        public string StatusText => (bool)Status ? "Active" : "Inactive";
        public List<UserStrategy>? UserStrategy { get; set; }
        public List<Stocks>? Stocks { get; set; }
        public User()
        {
            CreatedDate = DateTime.UtcNow;  
            UpdatedDate = DateTime.UtcNow;
            Status=true;
        }
    }

    public class UserStrategy
    {
        public string? StretagyName { get; set; }
        public bool StretagyEnableDisable { get; set; }
        public bool IsActive { get; set; }
    }
    public class Stocks
    {
        public string? Symbol { get; set; }
        public bool? StockNotification { get; set; }
        public string? CompanyName { get; set; }
        public string? CompanyLogoUrl { get; set; }
        public bool IsActive { get; set; }
        public string? BuySellSignal { get; set; }
    }
}
