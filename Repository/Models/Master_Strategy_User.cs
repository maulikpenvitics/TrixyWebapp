using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Models
{
    public class Master_Strategy_User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string? userId { get; set; }

        public bool RSI { get; set; }
        public bool Moving_Average { get; set; }
        public bool Bollinger_Bands { get; set; }
        public bool Mean_Reversion { get; set; }
        public bool VWAP { get; set; }
        public bool MACD { get; set; }
        public bool Sentiment_Analysis { get; set; }
        public bool Combine_Strategy { get; set; }
    }
}
