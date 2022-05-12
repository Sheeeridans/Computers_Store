using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MVC_Store.Models.ViewModels.Account
{
    public class LoginUserVM
    {
        [Required]
        [DisplayName("Логин")]
        public string Username { get; set; }
        [Required]
        [DisplayName("Пароль")]
        public string Password { get; set; }
        [DisplayName("Запомнить меня")]
        public bool RememberMe { get; set; }
    }
}