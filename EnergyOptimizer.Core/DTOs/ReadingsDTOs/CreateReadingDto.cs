using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnergyOptimizer.Core.DTOs.ReadingsDTOs
{
    public class CreateReadingDto
    {
        public int DeviceId { get; set; }
        public decimal PowerConsumptionKW { get; set; }
        public decimal Voltage { get; set; }
        public decimal Current { get; set; }
        public double Temperature { get; set; }
    }
}
