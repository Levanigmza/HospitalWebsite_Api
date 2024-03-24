using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HospitalWebsiteApi.Models
{
    public class LoginModel
    {
        [Required(ErrorMessage = "User Name is requeired")]
        public string Email { get; set; }
        [Required(ErrorMessage = "Password is requeired")]
        public string Password { get; set; }

        public int otpCode { get; set; }


    }
    public class EmailRequest
    {
        public string Email { get; set; }
    }

    public class CheckOTPRequest
    {
        public int otpCode { get; set; }
        public string Email { get; set; }

    }

    public class UpdatePasswordRequest
    {
        public string Email { get; set; }
        public string NewPassword { get; set; }
    }
}
