using System.ComponentModel.DataAnnotations;

namespace WeddingPlanner.Models
{
    public class Resevation
    {
        [Key]
        public int ResevationId { get; set; }
        [Required]
        public int WeddingPlanId { get; set; }
        [Required]
        public WeddingPlan WeddingPlan { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
        
    }

}