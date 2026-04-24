
namespace SnoopyAirlines.Domain.User
{
    public enum UserRole
    {
        Admin,
        Operator
    }

    public class UserRoleExtensions
    {
        public static string ToString(UserRole role)
        {
            return role switch
            {
                UserRole.Admin => "AD",
                UserRole.Operator => "OP",
                _ => throw new ArgumentOutOfRangeException(nameof(role), $"Not expected user role value: {role}")
            };
        }

        public static UserRole FromString(string roleString)
        {
            return roleString switch
            {
                "AD" => UserRole.Admin,
                "OP" => UserRole.Operator,
                _ => throw new ArgumentOutOfRangeException(nameof(roleString), $"Not expected user role string: {roleString}")
            };
        }
    }
}
