using System;
using System.Collections.Generic;
using System.Text;

namespace SafePoint.Data.Entities
{
    public class Shelter
    {
        public int Id { get; set; }

        public string Description { get; set; }

        public decimal LocX { get; set; }

        public decimal LocY { get; set; }
    }
}
