using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using iRaidTools;

namespace iRaidDatamodel.Entities
{
    [LoadInfo]
    public class User : DomainLogic<User>
    {
        public User(string Name, string UserName, string Email, string Password, bool enabled = true)
        {
            this.Name = Name;
            this.UserName = UserName;
            this.Email = Email;
            this.Tokens = 0;
            this.Created = DateTime.UtcNow;
            this.Enabled = enabled;
            this.Access = AccessLevel.Member;
            this.PasswordSalt = PasswordHelper.RandomString(10);
            this.Password = PasswordHelper.ComputeHash(Password, this.PasswordSalt);
            this.PasswordSet = DateTime.UtcNow;
            this.CharacterConfirmToken = PasswordHelper.RandomString(16);
        }

        public string Name { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public bool Enabled { get; set; }
        public double Tokens { get; set; }
        public DateTime Created { get; set; }
        public DateTime PasswordSet { get; private set; }
        public string Password { get; set; }
        public string PasswordSalt { get; set; }
        public string PasswordResetToken { get; set; }
        public string CharacterConfirmToken { get; set; }
        public AccessLevel Access { get; set; }
        
        /// <summary>
        /// Communities that user is member of.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Community> MemberOf()
        {
            long[] l = Repository<Member>.FindBy(x => x.UserId == this.Id).Select(x => x.CommunityId).ToArray();

            return Repository<Community>.FindBy(x => l.Contains(x.Id));
        }

        /// <summary>
        /// Private messages sent to User.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<PrivateUserMessage> PrivateMessages()
        {
            return Repository<PrivateUserMessage>.FindBy(x => x.UserId == this.Id);
        }

        /// <summary>
        /// Tries to get user by {UserId} from Object-cache.
        /// </summary>
        /// <param name="UserId"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        public static bool TryGetUser(long UserId, out User user)
        {
            user = Repository<User>.FindFirstBy(x => x.Id == UserId);

            return user != null;
        }

        /// <summary>
        /// Tries to get user by {username} from Object-cache.
        /// </summary>
        /// <param name="username"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        public static bool TryGetUser(string username, out User user)
        {
            user = User.FindFirst(x => x.UserName.ToLower() == username.ToLower());

            return user != null;
        }

        /// <summary>
        /// Tries to join community.
        /// </summary>
        /// <param name="community"></param>
        /// <returns></returns>
        public bool JoinCommunity(Community community)
        {
            return community.AddMember(this, this);
        }

        public bool Login(string password)
        {
            if(PasswordHelper.ComputeHash(password, this.PasswordSalt) == this.Password)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Change password.
        /// </summary>
        /// <param name="oldPassword"></param>
        /// <param name="newPassword"></param>
        /// <returns></returns>
        public bool ChangePassword(string oldPassword, string newPassword)
        {
            if(this.Password == PasswordHelper.ComputeHash(oldPassword, this.PasswordSalt))
            {
                this.PasswordSalt = PasswordHelper.RandomString(10);
                this.Password = PasswordHelper.ComputeHash(newPassword, this.PasswordSalt);
                this.PasswordSet = DateTime.UtcNow;
                this.PasswordResetToken = string.Empty;
                this.Save();

                return true;
            }

            return false;
        }

        public void AddTokens(int p, User user)
        {
            this.Tokens += Math.Abs(p);

            Save(user);
        }

        public void RemoveTokens(int p, User user)
        {
            if(this.Tokens-Math.Abs(p) < 0)
            {
                throw new Exception("Cannot go below zero.");
            }

            this.Tokens -= Math.Abs(p);

            Save(user);
        }
    }
}
