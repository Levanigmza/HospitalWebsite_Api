using System.ComponentModel.DataAnnotations.Schema;

namespace HospitalWebsiteApi.Models
{
    public class User
    {
        public int Id { get; set; }

        public IFormFile Photo { get; set; }

    }


}

