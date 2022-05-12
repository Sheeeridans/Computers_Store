using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using MVC_Store.Models.Data;

namespace MVC_Store.Models.ViewModels.Account
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
        [Required]
        [DisplayName("Имя")]
        public string FirstName { get; set; }
        [Required]
        [DisplayName("Фамилия")]
        public string LastName { get; set; }
        [Required]
        [DataType(DataType.EmailAddress)]
        [DisplayName("e-mail")]
        public string EmailAdress { get; set; }
        [Required]
        [DisplayName("Логин")]
        public string Username { get; set; }
        [Required]
        [DisplayName("Пароль")]
        public string Password { get; set; }
        [Required]
        [DisplayName("Повторите пароль")]
        public string ConfirmPassword { get; set; }
    }
}