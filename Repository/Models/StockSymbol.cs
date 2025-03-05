using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Repository.Models
{
    public class StockSymbol
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId Id { get; set; }

        [Required]
        public string? Symbol { get; set; }  // Stock ticker symbol (e.g., AAPL, MSFT)

        [Required]
        public string? CompanyName { get; set; }  // Full name of the company

      
        public string? CompanyIconUrl { get; set; }  // Full name of the company

        public bool? Status { get; set; }


        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime? CreatedDate { get; set; }

        public string? CreatedBy { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime? UpdatedDate { get; set; }

        public string? UpdatedBy { get; set; }

        public string StatusText => (bool)Status ? "Active" : "Inactive";

        [BsonIgnore] // This will prevent MongoDB from storing the file object
        public IFormFile IconFile { get; set; }


        public StockSymbol()
        {
            CreatedDate = DateTime.UtcNow;
            UpdatedDate = DateTime.UtcNow;
            Status = true;
        }
    }

}
