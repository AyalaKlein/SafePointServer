using System;
using System.Collections.Generic;
using System.Text;

namespace SafePoint.Data.Entities
{
    public class Location
    {
        public Location(decimal lat, decimal lon)
        {
            this.Latitude = lat;
            this.Longitude = lon;
        }

        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }

        public double CalculateDistance(Location locTo)
        {
            decimal pi180 = (decimal)(Math.PI / 180.0);
            double d1 = (double)(this.Latitude * pi180);
            var num1 = this.Longitude * pi180;
            double d2 = (double)(locTo.Latitude * pi180);
            double num2 = (double)(locTo.Longitude * pi180 - num1);
            var d3 = Math.Pow(Math.Sin((d2 - d1) / 2.0), 2.0) +
                     Math.Cos(d1) * Math.Cos(d2) * Math.Pow(Math.Sin(num2 / 2.0), 2.0);
            return 6376500.0 * (2.0 * Math.Atan2(Math.Sqrt(d3), Math.Sqrt(1.0 - d3)));

            //(PI() * sh."LocX" / 180)
            //(PI() * {LocX} / 180)
            //(sh."LocY" - sh."LocY")
            //((PI() * (sh."LocY" - sh."LocY")) / 180)
            //SIN((PI() * sh."LocX" / 180)) * SIN((PI() * sh."LocX" / 180)) + COS((PI() * sh."LocX" / 180)) * COS((PI() * sh."LocX" / 180)) * COS(((PI() * (sh."LocY" - sh."LocY")) / 180))
            //ACOS(SIN((PI() * sh."LocX" / 180)) * SIN((PI() * {LocX} / 180)) + COS((PI() * sh."LocX" / 180)) * COS((PI() * {LocX} / 180)) * COS(((PI() * (sh."LocY" - {LocY})) / 180)))


            //SIN((PI() * sh."LocX" / 180)) * SIN((PI() * sh."LocX" / 180)) + COS((PI() * sh."LocX" / 180)) * COS((PI() * sh."LocX" / 180)) * COS(((PI() * (sh."LocY" - sh."LocY")) / 180)) * 180/PI() * 60 * 1.1515
            //SIN(((sh.""LocX"" * {pi180}) - (sh.""LocX"" * {pi180}))/2), 2) + COS(sh.""LocX"" * {pi180}) * COS(sh.""LocY"" * {pi180}) *POW(SIN((sh.""LocX"" * {pi180} - (sh.""LocY""-{pi180}))/2),2)),
            //SQRT(1 - SQRT(Power(SIN(((sh.""LocY"" * { pi180}) -(sh.""LocX"" * { pi180}))/ 2), 2) +COS(sh.""LocX"" * { pi180}) *COS(sh.""LocY"" * { pi180}) *POW(SIN((sh.""LocX"" * { pi180}
            //-(sh.""LocY"" -{ pi180}))/ 2),2))))) 
        }
    }
}
