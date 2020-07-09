using Shop.Models.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Shop.Models.ViewModels.Account
{
    public class UserVM
    {
        public UserVM()
        {
        }

        public UserVM(UserDTO row)
        {
            Id = row.Id;
            FirstName = row.FirstName;
            LastName = row.LastName;
            EmailAdress = row.EmailAdress;
            Username = row.Username;
            Password = row.Password;
        }

        public int Id { get; set; }
        [Required(ErrorMessage = "First Name area is empty")]
        [DisplayName("First Name")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last Name area is empty")]
        [DisplayName("Last Name")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Email area is empty")]
        [DataType(DataType.EmailAddress)]
        [DisplayName("Email")]
        public string EmailAdress { get; set; }

        [Required(ErrorMessage = "User Name area is empty")]
        [DisplayName("User Name")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Password area is empty")]
        public string Password { get; set; }

        [Required]
        [DisplayName("Confirm Password")]
        public string ConfirmPassword { get; set; }
    }
}