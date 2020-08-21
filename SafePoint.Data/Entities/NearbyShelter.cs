using System;
using System.Collections.Generic;
using System.Text;

namespace SafePoint.Data.Entities
{
    public class NearbyShelter
    {
        public int Id { get; set; }
        public decimal LocX { get; set; }
        public decimal LocY { get; set; }
        public string UserToken { get; set; }
        public double Distance { get; set; }
        public int MaxCapacity { get; set; }
        public string Description { get; set; }
    }
}
