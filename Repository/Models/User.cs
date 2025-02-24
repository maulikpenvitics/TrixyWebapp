using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Models
{
    public class User
    {
        public ObjectId Id { get; set; }
        public string? firstname { get; set; }
        public string? lastname { get; set; }
        public string? email { get; set; }
        public string? password { get; set; }
        public string? role { get; set; }
        public string? username { get; set; }
    }
}
