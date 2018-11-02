using System;
using System.ComponentModel.DataAnnotations;

namespace Wedding_Planner.Models
{
    public class RSVP
    {  
        [Key]
        public int RSVPId {get;set;}
        public int WeddingId {get;set;}
        public Wedding Wedding {get;set;}
        public int UserId {get;set;}
        public User User {get;set;}
        public DateTime CreatedAt {get;set;}
        public DateTime UpdatedAt {get;set;}
    }
}