using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.Models;
using IdentityServer4.Validation;

namespace RepostAspNet
{
    public class ResourceOwnerPasswordValidator : IResourceOwnerPasswordValidator
    {
        private readonly DatabaseContext _databaseContext;

        public ResourceOwnerPasswordValidator(DatabaseContext databaseContext)
        {
            _databaseContext = databaseContext;
        }

        public async Task ValidateAsync(ResourceOwnerPasswordValidationContext context)
        {
            context.Result = ValidateUser(context.UserName, context.Password);
        }

        private GrantValidationResult ValidateUser(string username, string password)
        {
            var user = _databaseContext.Users.SingleOrDefault(u => u.Username == username);
            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.HashedPassword))
            {
                return new GrantValidationResult(TokenRequestErrors.InvalidGrant);
            }

            return new GrantValidationResult(user.Username, "password");
        }
    }
}