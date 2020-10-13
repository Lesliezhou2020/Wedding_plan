using System.Collections.Generic;

namespace WeddingPlanner.Models
{
    public class DisplayOnePlan
    {
        public List<Resevation> Guest { get; set; }
        public WeddingPlan Oneplan { get; set; }
    }
}