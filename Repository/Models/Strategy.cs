using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Models
{
    public class Strategy
    {
        public ObjectId Id { get; set; }
        public string strategy_name { get; set; }
    }
}
