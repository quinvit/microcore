namespace AuthService.Interface.Models
{
    public class User
    {
        public string Username { get; set; }

        public string InitialPassword { get; set; }

        public string Email { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string JobTitle { get; set; }

        public int YearsOfExperience { get; set; }

        public string Company { get; set; }

        public string[] SocialProfiles { get; set; }
    }
}
