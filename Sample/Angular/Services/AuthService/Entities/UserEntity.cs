using AuthService.Interface.Models;
using Microsoft.Azure.Cosmos.Table;
using System;

namespace AuthService.Entities
{
    public class UserEntity : TableEntity
    {
        public string Username { get ; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get ; set; }
        public string SocialProfiles { get ; set; }
        public string JobTitle { get ; set; }
        public int YearsOfExperience { get ; set; }
        public string Company { get ; set; }

        public UserEntity()
        {
        }

        public UserEntity(User user): this(user?.Username, user?.FirstName)
        {
            Username = user.Username;
            Email = user.Email;
            FirstName = user.FirstName;
            LastName = user.LastName;
            SocialProfiles = string.Join(" | " ,user.SocialProfiles);
            JobTitle = user.JobTitle;
            YearsOfExperience = user.YearsOfExperience;
            Company = user.Company;
        }

        public UserEntity(string username, string firstName)
        {
            PartitionKey = firstName;
            RowKey = username;
        }
    }
}
