using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace INCHEQS.ViewModels {
    public class LoginViewModel {
        [Display(Name = "User Id")]
        public string UserAbbr { get; set; }

        [Display(Name = "Password")]
        public string UserPassword { get; set; }

        [Display(Name = "Old Password")]
        public string OldPassword { get; set; }

        [Display(Name = "New Password")]
        public string NewPassword { get; set; }

        [Display(Name = "Confirm Password")]
        public string ConfirmNewPassword { get; set; }

        [Display(Name = "Login AD")]
        public string SelectLoginAD { get; set; }

        [HiddenInput(DisplayValue = false)]
        public string UserId { get; set; }

        [HiddenInput(DisplayValue = false)]
        public string macAddress { get; set; }

        [Display(Name = "Domain")]
        public string Domain { get; set; }
    }
}