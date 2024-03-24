namespace HospitalWebsiteApi.Models
{
    public class DoctorInfo
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Email { get; set; }
        public string PersonalID { get; set; }
        public string Position { get; set; }
        public byte[] Photo { get; set; }

    }

    public class DoctorRegistrationRequest
    {
        public UserParameters Doctor { get; set; }
        public string Position { get; set; }
        public IFormFile Photo { get; set; } 
    }


}
