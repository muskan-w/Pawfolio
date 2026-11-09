// Pet creation 

// Import error message/required tag 
using System.ComponentModel.DataAnnotations;

namespace pawfolio_webapp.Models
{
    // Pet in the application with properties/error handling 
    public class Pet
    {
        // ID for each pet 
        public int Id { get; set; }

        // Link the pet specifically to the user who created it
        public string? UserId { get; set; }

        [Required(ErrorMessage = "Please enter the pet's name.")]
        public string Name { get; set; } 

        [Required(ErrorMessage = "Please enter the pet's breed.")]
        public string Breed { get; set; } 

        [Required(ErrorMessage = "Please enter the pet's age.")]
        [Range(0, 100, ErrorMessage = "Please enter a valid age between 0 and 100.")]
        public int Age { get; set; }

        [Required(ErrorMessage = "Please enter the pet's gender.")]
        public string Gender { get; set; } 

        // Constructor for the Pet class to initialize properties
        public Pet()
        {
            Name = string.Empty;
            Breed = string.Empty;
            Age = 0;
            Gender = string.Empty;
        }
    }
}
