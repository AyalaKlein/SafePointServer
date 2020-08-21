using SafePoint.Data.Entities;
using System.Collections.Generic;

namespace SafePoint.Server.Controllers
{
    internal class ShelterInfo
    {
        public List<string> AssignedUsers { get; set; }
        public double Distance { get; internal set; }
        public Shelter Shelter { get; internal set; }
    }
}