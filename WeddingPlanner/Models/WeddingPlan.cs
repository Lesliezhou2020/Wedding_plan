using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace WeddingPlanner.Models
{
    public class WeddingPlan
    {
        [Key]
        public int WeddingPlanId { get; set; }
        [Required]
        [MinLength(3, ErrorMessage="Wedding One name must be 3 characters or longer!")]
        public string WedderOne { get; set; }
        [Required]
        [MinLength(3, ErrorMessage="Wedding Two name must be 3 characters or longer!")]
        public string WedderTwo { get; set; }
        [Required]
        public String Date { get; set; }
        [Required]
        [MinLength(1, ErrorMessage="Address can not be empty!")]
        public string Address { get; set; }
        [Required]
        public int UserId { get; set; }
        public List<Resevation> Guests { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
        public User User { get; set; }
    }

}