using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Models
{
    public class Master
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string? userId { get; set; }

        public string? RSI { get; set; }
        public string? Moving_Average { get; set; }
        public string? Bollinger_Bands { get; set; }
        public string? Mean_Reversion { get; set; }
        public string? VWAP { get; set; }
        public string? MACD { get; set; }
        public string? Sentiment_Analysis { get; set; }
        public string? Combine_Strategy { get; set; }
    }
}
