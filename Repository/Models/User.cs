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
        public UserStrategy? UserStrategy { get; set; }
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
        public bool RSI { get; set; }
        public bool Moving_Average { get; set; }
        public bool Bollinger_Bands { get; set; }
        public bool Mean_Reversion { get; set; }
        public bool VWAP { get; set; }
        public bool MACD { get; set; }
        public bool Sentiment_Analysis { get; set; }
        public bool Combine_Strategy { get; set; }
    }
    public class Stocks
    {
        public string? Symbol { get; set; }
        public bool? StockNotification { get; set; }
    }
}
