using IdentityApi.DbModels;
using IdentityApi.Helpers;
using IdentityApi.Interfaces;
using IdentityApi.Messages;
using IdentityApi.Models;
using MessageService.MessageServices;

namespace IdentityApi.Managers
{
    public class UserManager : IUserManager
    {
        private readonly IUserProvider _userProvider;
        private readonly IMessageService _messageService;
        private readonly IMessageProvider _messageProvider;

        public UserManager(IUserProvider userProvider, IMessageService messageService, IMessageProvider messageProvider)
        {
            _userProvider = userProvider;
            _messageService = messageService;
            _messageProvider = messageProvider;
        }

        /// <inheritdoc/>
        public async Task<User> CreateUserAsync(UserCreate userCreate)
        {
            // check if user exists
            var existingUser = await _userProvider.GetUserByEmailAsync(userCreate.Email);
            // User already in use 
            if (existingUser != null)
            {
                // TODO: log

                throw new Exception("Email already in use");
            }


            // TODO: create mapper

            var toCreateDbUser = new DbUser()
            {
                Email = userCreate.Email,
                FirstName = userCreate.FirstName,
                LastName = userCreate.LastName,
                PhoneNumber = userCreate.PhoneNumber
            };

            toCreateDbUser.Salt = Security.GetSalt(50);
            toCreateDbUser.HashedPassword = Security.GetEncryptedAndSaltedPassword(userCreate.Password, toCreateDbUser.Salt);
            toCreateDbUser.Counter = 0;
            toCreateDbUser.SecretKey = Security.GetHmacKey();

            var createdUser = await _userProvider.CreateUserAsync(toCreateDbUser);

            var registerMessage = _messageProvider.GetRegisterMessage(createdUser.Email, createdUser.SecretKey);

            _messageService.SendMessageAsync(registerMessage);

            return new User()
            {
                ID = createdUser.ID,
                Email = createdUser.Email,
                FirstName = createdUser.FirstName,
                LastName = createdUser.LastName,
                PhoneNumber = createdUser.PhoneNumber
            };
        }

        public async Task<User> GetUserByIDAsync(int ID)
        {
            try
            {
                var dbUser = await _userProvider.GetUserByIDAsync(ID);

                if (dbUser == null)
                    return null;

                // TODO: Add mapper

                return new User()
                {
                    ID = dbUser.ID,
                    Email = dbUser.Email,
                    FirstName = dbUser.FirstName
                };

            }
            catch (Exception e)
            {
                // TODO: Add log

                throw e;
            }
        }

        /// <inheritdoc/>
        public async Task<User> LoginAsync(UserLogin userLogin)
        {
            // checks if user exists
            var existingUser = await _userProvider.GetUserByEmailAsync(userLogin.Email);

            if (existingUser == null)
            {
                return null;
            }

            // User is locked, no need for further checks
            if (existingUser.IsLocked)
            {
                // TODO: log
                throw new Exception("Login failed, Account locked");
            }



            // check if given password matches with the hashedpassword of the user
            if (existingUser.HashedPassword == Security.GetEncryptedAndSaltedPassword(userLogin.Password, existingUser.Salt))
            {
                // TODO: Add check for ip adresse here
                // if user was not logged in with this ip adress
                // Send 2FA here, then
                var hotp = Security.GetHotp(existingUser.SecretKey, existingUser.Counter);
                if (hotp != null)
                {
                    var loginFromAnotherLocationEmail = _messageProvider.GetLoginAttemptMessage(existingUser.Email, hotp);

                    _messageService.SendMessageAsync(loginFromAnotherLocationEmail);
                }
                // throw error here with "Check email", 

                // else

                // login success 
                existingUser = await _userProvider.UpdateUserLoginSuccess(existingUser.ID);


                // TODO: Update 

                return existingUser;
            }
            else
            {
                // TODO: log
                // login failed

                existingUser = await _userProvider.UpdateUserFailedTries(existingUser.ID);

                if (existingUser.IsLocked)
                {
                    throw new Exception("Login failed, Account locked");
                }

                // update tries, if tries >= 3 lock account  <- consider moving both to sp and returning dbuser
                throw new Exception("Login failed, username or password is incorrect");
            }
        }

        /// <inheritdoc/>
        public async Task<User> LoginOtpAsync(UserLogin userLogin)
        {
            // checks if user exists
            var existingUser = await _userProvider.GetUserByEmailAsync(userLogin.Email);

            if (existingUser == null)
            {
                return null;
            }

            // User is locked, no need for further checks
            if (existingUser.IsLocked)
            {
                // TODO: log
                throw new Exception("Login failed, Account locked");
            }

            // TODO: Add in SP
            if(existingUser.LastRequestDate.HasValue && existingUser.LastRequestDate.Value.AddMinutes(15) < DateTime.Now)
            {
                throw new Exception("Login failed, password expired");
            }

            // check if given otp password matches what is expected
            if (Security.GetHotp(existingUser.SecretKey, existingUser.Counter) == userLogin.Password)
            {

                // login success 
                existingUser = await _userProvider.UpdateUserLoginSuccess(existingUser.ID);


                // TODO: Update counter & login tries

                return existingUser;
            }
            else
            {
                // TODO: log
                // login failed

                existingUser = await _userProvider.UpdateUserFailedTries(existingUser.ID);

                if (existingUser.IsLocked)
                {
                    throw new Exception("Login failed, Account locked");
                }

                // update tries, if tries >= 3 lock account  <- consider moving both to sp and returning dbuser
                throw new Exception("Login failed, one-time password is wrong");
            }
        }
    }
}
