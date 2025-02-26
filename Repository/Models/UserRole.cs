using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Models
{
    public class UserRole
    {
        public ObjectId Id { get; set; }
        public string? Role{ get; set; }
        public string? Code { get; set; }
    }
}
