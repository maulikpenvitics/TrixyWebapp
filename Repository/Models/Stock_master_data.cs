using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Models
{
    public class Stock_master_data
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("StockSymbol")]
        public string? StockSymbl { get; set; }
        public string? CompanyName { get; set; }
        public string? Code { get; set; }
        public bool Status { get; set; }
        [BsonDateTimeOptions]
        public DateTime CreatedDate { get; set; }
        public string? CreatedBy { get; set; }
        [BsonDateTimeOptions]
        public DateTime UpdatedDate { get; set; }

        public string? Updatedby { get; set; }

        public Stock_master_data()
        {
            UpdatedDate = DateTime.UtcNow;
            CreatedDate = DateTime.UtcNow;
        }
    }
}
