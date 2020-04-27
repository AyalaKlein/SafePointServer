using System;
using System.Collections.Generic;
using System.Text;

namespace SafePoint.Data.Entities
{
    public class Shelter
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public float Lat { get; set; }

        public float Lon { get; set; }
    }
}
