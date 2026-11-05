using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Backend.Cliniq.Models.Entities.Base;
using Cliniq.Models.Options;
using Microsoft.IdentityModel.Tokens;

namespace Cliniq.Services.DataServices.Helpers
{
    public static class JwtHelpers
    {
        public static string GenerateJwtToken(User user, JwtOptions jwtOptions)
        {
            ArgumentNullException.ThrowIfNull(user);
            ArgumentNullException.ThrowIfNull(jwtOptions);

            // Only active users with a valid role can sign in.
            if (!user.IsActive)
            {
                throw new InvalidOperationException("Cannot generate a token for an inactive user.");
            }

            if (!Enum.IsDefined(user.Role))
            {
                throw new ArgumentOutOfRangeException(nameof(user), "The user's role is invalid.");
            }

            var signingKey = GetSigningKey(jwtOptions);
            var now = DateTime.UtcNow;
            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Issuer = jwtOptions.Issuer,
                Audience = jwtOptions.Audience,
                // The server signs the token so it cannot be changed unnoticed.
                SigningCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256),
                // Claims are user details that the API can read after sign-in.
                Subject = new ClaimsIdentity(
                [
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.FullName),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Role, user.Role.ToString())
                ]),
                IssuedAt = now,
                NotBefore = now,
                // The token stops working after the configured lifetime.
                Expires = now.AddMinutes(jwtOptions.Lifetime)
            };

            // Turn the token details into the string sent to the client.
            return tokenHandler.WriteToken(tokenHandler.CreateToken(tokenDescriptor));
        }

        public static int? ValidateJwtToken(string token, JwtOptions jwtOptions)
        {
            ArgumentNullException.ThrowIfNull(jwtOptions);
            if (string.IsNullOrWhiteSpace(token))
            {
                // An empty token cannot identify a user.
                return null;
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            try
            {
                // Check the signature, issuer, audience, algorithm, and expiry.
                var principal = tokenHandler.ValidateToken(
                    token,
                    new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = GetSigningKey(jwtOptions),
                        ValidateIssuer = true,
                        ValidIssuer = jwtOptions.Issuer,
                        ValidateAudience = true,
                        ValidAudience = jwtOptions.Audience,
                        ValidateLifetime = true,
                        RequireExpirationTime = true,
                        RequireSignedTokens = true,
                        ValidAlgorithms = [SecurityAlgorithms.HmacSha256],
                        ClockSkew = TimeSpan.FromMinutes(1)
                    },
                    out _);

                // Return the user ID only when the token contains a valid ID claim.
                var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                return int.TryParse(userId, out var id) ? id : null;
            }
            catch (SecurityTokenException)
            {
                // Invalid or expired tokens do not identify a user.
                return null;
            }
            catch (ArgumentException)
            {
                return null;
            }
        }

        public static string HashPassword(string password)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(password);
            // BCrypt adds a salt and makes the password costly to guess.
            return BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);
        }

        public static bool VerifyPasswordHash(string password, string storedHash)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(password);
            ArgumentException.ThrowIfNullOrWhiteSpace(storedHash);
            // Compare the password with its saved hash; never save the plain password.
            return BCrypt.Net.BCrypt.Verify(password, storedHash);
        }

        private static SymmetricSecurityKey GetSigningKey(JwtOptions jwtOptions)
        {
            // A missing or short key would make signed tokens unsafe.
            if (string.IsNullOrWhiteSpace(jwtOptions.SignKey))
            {
                throw new InvalidOperationException("A JWT signing key must be configured.");
            }

            var key = Encoding.UTF8.GetBytes(jwtOptions.SignKey);
            if (key.Length < 32)
            {
                throw new InvalidOperationException(
                    "The JWT signing key must be at least 32 bytes for HMAC-SHA256.");
            }

            return new SymmetricSecurityKey(key);
        }
    }
}
