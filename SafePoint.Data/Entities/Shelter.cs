using System;
using System.Collections.Generic;
using System.Text;

namespace SafePoint.Data.Entities
{
    public class Shelter
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public decimal Lat { get; set; }

        public decimal Lon { get; set; }
    }
}
