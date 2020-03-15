using AuthService.Interface;
using AuthService.Interface.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;
using System.Linq;
using AuthService.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Azure.Cosmos.Table;
using System;
using Serilog;
using AuthService.Storages;
using AutoMapper;
using AzureStorage;
using Microsoft.Azure.Storage.Queue;

namespace AuthService
{
    public class AuthService : IAuthService
    {
        public const string UserTableName = "Users";
        public const string UserQueueName = "users";

        private readonly IConfiguration _configuration;

        private readonly IMapper _mapper;

        public AuthService(IConfiguration configuration, IMapper mapper)
        {
            _configuration = configuration;
            _mapper = mapper;
        }

        public async Task<User> GetUserAsync(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                throw new ArgumentNullException("token");
            }

            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
            var claims = jwt.Claims.ToList();
            var username = claims.FirstOrDefault(x => x.Type == "unique_name")?.Value?.Split('#').Last();
            var firstName = claims.FirstOrDefault(x => x.Type == "given_name")?.Value;

            try
            {
                var table = await TableStorage.CreateTableAsync(_configuration, UserTableName);
                TableOperation retrieveOperation = TableOperation.Retrieve<UserEntity>(firstName, username);
                TableResult result = await table.ExecuteAsync(retrieveOperation);
                UserEntity user = result.Result as UserEntity;

                if(user == null)
                {
                    Log.Logger.Error("Cannot find user {firstName}, {username}.", firstName, username);
                }

                return _mapper.Map<UserEntity, User>(user);
            }
            catch (StorageException e)
            {
                Log.Logger.Error(e, "Error when getting user from storage.");
                throw e;
            }
        }

        public async Task<bool> RegisterUserAsync(User user)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            if (!await InsertUserAsync(user))
            {
                return false;
            }

            return await QueueMessageAsync(user);
        }

        private async Task<bool> QueueMessageAsync(User user)
        {
            try
            {
                // Add message to queue to trigger azure logic app to create AD user
                var queue = await QueueStorage.CreateQueueAsync(_configuration, UserQueueName);

                // Create random user password and put to queue
                var random = new Random();
                user.InitialPassword = random.Next(0, 999999).ToString("000000");
                CloudQueueMessage message = new CloudQueueMessage(JsonExtensions.SerializeToJson(user));
                await queue.AddMessageAsync(message);
                return true;
            }
            catch (StorageException e)
            {
                Log.Logger.Error(e, "Error when queue user creation message.");
                throw e;
            }
        }

        private async Task<bool> InsertUserAsync(User user)
        {
            try
            {
                var identity = user.Email.Replace('@', '_');
                user.Username = string.Concat(identity, "@", _configuration.GetValue<string>("AzureAd:Domain"));

                var entity = new UserEntity(user);
                var table = await TableStorage.CreateTableAsync(_configuration, UserTableName);

                TableOperation insert = TableOperation.Insert(entity);

                TableResult result = await table.ExecuteAsync(insert);

                return result.HttpStatusCode == 200 || result.HttpStatusCode == 204;
            }
            catch (StorageException e)
            {
                Log.Logger.Error(e, "Error when register user to storage.");
                throw e;
            }
        }
    }
}
