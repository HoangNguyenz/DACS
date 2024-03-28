using System;
using System.ComponentModel.DataAnnotations;

namespace Test123.Models
{
    public class PersonModel
    {
        [Key]
        public int PersonID { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public DateTime Birthday { get; set; }
    }
}

