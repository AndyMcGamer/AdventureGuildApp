using AdventureGuildAPI.Data;
using AdventureGuildAPI.Models;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MimeKit;
using Org.BouncyCastle.Asn1.Ocsp;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace AdventureGuildAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(DataContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [AllowAnonymous]
        [HttpPost("register")]
        [ProducesResponseType(typeof(User), 200)]
        [ProducesResponseType(409)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> Register(UserRegistration request)
        {
            var foundUser = await _context.Users.AsNoTracking().FirstOrDefaultAsync(o => o.Username.ToLower() == request.Username.ToLower() || o.EmailAddress.ToLower() == request.Email.ToLower());

            if (foundUser != null) return StatusCode(409);

            User user = new()
            {
                Username = request.Username,
                EmailAddress = request.Email,
                PasswordSalt = CreateRandomToken(32),
                FirstName = request.FirstName,
                LastName = request.LastName,
                Role = "User",
                VerificationToken = await CreateVerificationToken(),
                ResetPasswordToken = null,
                Money = 0
            };
            user.Password = GenerateSaltedHash(Encoding.UTF8.GetBytes(request.Password), user.PasswordSalt);
            user.Verified = false;

            await _context.Users.AddAsync(user);

            await _context.SaveChangesAsync();

            //Send Email
            return await SendVerificationEmail(user);

        }

        [AllowAnonymous]
        [HttpPost("login")]
        [ProducesResponseType(400), ProducesResponseType(typeof(AccessToken),200)]
        public async Task<IActionResult> Login(UserLogin request)
        {
            var foundUser = await _context.Users.AsNoTracking().FirstOrDefaultAsync(o => o.Username.ToLower() == request.Username.ToLower() || o.EmailAddress.ToLower() == request.Username.ToLower());
            if (foundUser == null) return BadRequest("Invalid Credentials");

            if (!foundUser.Verified) return BadRequest("Not Verified");

            if (!VerifyCredentials(foundUser, request)) return BadRequest("Invalid Credentials.");

            var token = GenerateAccessToken(foundUser);

            var refreshToken = GenerateRefreshToken();
            await SetRefreshToken(refreshToken, foundUser);

            var idToken = GenerateIdToken(foundUser, refreshToken.Expires);
            SetIdToken(idToken, refreshToken.Expires);

            return Ok(token);
        }

        [AllowAnonymous]
        [HttpPost("refresh-token")]
        [ProducesResponseType(typeof(AccessToken), 200), ProducesResponseType(404), ProducesResponseType(400)]
        public async Task<IActionResult> RefreshToken()
        {
            var refreshToken = Request.Cookies["refreshToken"];
            if (refreshToken == null) return NotFound("No refresh token found");

            var requestingUser = new JwtSecurityTokenHandler().ReadJwtToken(Request.Cookies["idToken"]).Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (requestingUser == null) return NotFound("No id found");

            if (!int.TryParse(requestingUser, out var userId)) return BadRequest();

            var foundUser = await _context.Users.FindAsync(userId);
            if (foundUser == null) return NotFound("User not found");

            if (foundUser.RefreshToken != refreshToken)
            {
                foundUser.RefreshToken = null;
                await _context.SaveChangesAsync();
                return BadRequest("Invalid refresh token");
            }

            var token = GenerateAccessToken(foundUser);
            var newRefresh = GenerateRefreshToken();
            await SetRefreshToken(newRefresh, foundUser);
            var idToken = GenerateIdToken(foundUser, newRefresh.Expires);
            SetIdToken(idToken, newRefresh.Expires);

            return Ok(token);
        }

        [AllowAnonymous]
        [HttpGet("verify-email")]
        [ProducesResponseType(typeof(string), 200), ProducesResponseType(400)]
        public async Task<IActionResult> VerifyEmail(string token)
        {
            var verificationToken = Convert.FromBase64String(token);
            var foundUser = await _context.Users.FirstOrDefaultAsync(o => o.VerificationToken.SequenceEqual(verificationToken));
            if (foundUser == null) return BadRequest("Invalid token");

            foundUser.Verified = true;
            await _context.SaveChangesAsync();

            return Ok("User verified");
        }

        [AllowAnonymous]
        [HttpPost("forgot-password")]
        [ProducesResponseType(200), ProducesResponseType(404)]
        public async Task<IActionResult> ForgotPassword(string emailAddress)
        {
            var foundUser = await _context.Users.FirstOrDefaultAsync(o => o.EmailAddress == emailAddress);
            if (foundUser == null) return NotFound("No user found");

            foundUser.ResetPasswordToken = await CreateResetPasswordToken();
            foundUser.ResetPassExpires = DateTime.UtcNow.AddDays(1);

            await _context.SaveChangesAsync();

            return await SendResetEmail(foundUser);
        }

        [AllowAnonymous]
        [HttpGet("reset-token-check")]
        [ProducesResponseType(typeof(string), 200), ProducesResponseType(400)]
        public async Task<IActionResult> ResetTokenCheck(string token)
        {
            var resetToken = Convert.FromBase64String(token);
            var foundUser = await _context.Users.FirstOrDefaultAsync(o => o.ResetPasswordToken != null && o.ResetPasswordToken.SequenceEqual(resetToken));
            if (foundUser == null) return BadRequest("Invalid Token");
            if (foundUser.ResetPassExpires < DateTime.UtcNow) return BadRequest("Token expired");

            return Ok(token);
        }

        [AllowAnonymous]
        [HttpPost("reset-password")]
        [ProducesResponseType(400), ProducesResponseType(200)]
        public async Task<IActionResult> ResetPassword(ResetPasswordRequest request)
        {
            var foundUser = await _context.Users.FirstOrDefaultAsync(o => o.ResetPasswordToken != null && o.ResetPasswordToken.SequenceEqual(request.Token));
            if (foundUser == null) return BadRequest("Invalid Token");
            if (foundUser.ResetPassExpires < DateTime.UtcNow) return BadRequest("Token expired");

            foundUser.PasswordSalt = CreateRandomToken(32);
            foundUser.Password = GenerateSaltedHash(Encoding.UTF8.GetBytes(request.Password), foundUser.PasswordSalt);
            foundUser.ResetPasswordToken = null;
            foundUser.ResetPassExpires = null;
            await _context.SaveChangesAsync();

            return Ok("Password Reset");
        }

        [Authorize(Policy = "RequireUser")]
        [HttpPost("logout")]
        [ProducesResponseType(typeof(string), 200), ProducesResponseType(404), ProducesResponseType(401)]
        public async Task<IActionResult> Logout()
        {
            Response.Cookies.Delete("refreshToken");
            Response.Cookies.Delete("idToken");
            var userName = User.FindFirstValue(ClaimTypes.Name);
            if (userName == null) return NotFound("No user found.");
            var foundUser = await _context.Users.FirstOrDefaultAsync(o => o.Username.ToLower() == userName);

            if (foundUser == null) return NotFound("No user found.");

            foundUser.RefreshToken = null;
            await _context.SaveChangesAsync();

            return Ok("User logged out.");
        }

        private async Task<byte[]> CreateVerificationToken()
        {
            byte[] token = CreateRandomToken(32);
            var foundUser = await _context.Users.FirstOrDefaultAsync(o => o.VerificationToken.SequenceEqual(token));
            
            while(foundUser != null)
            {
                token = CreateRandomToken(32);
                foundUser = await _context.Users.FirstOrDefaultAsync(o => o.VerificationToken.SequenceEqual(token));
            }

            return token;
        }

        private async Task<byte[]> CreateResetPasswordToken()
        {
            byte[] token = CreateRandomToken(32);
            var foundUser = await _context.Users.AsNoTracking().FirstOrDefaultAsync(o => o.ResetPasswordToken != null && o.ResetPasswordToken.SequenceEqual(token));

            while (foundUser != null)
            {
                token = CreateRandomToken(32);
                foundUser = await _context.Users.AsNoTracking().FirstOrDefaultAsync(o => o.ResetPasswordToken != null && o.ResetPasswordToken.SequenceEqual(token));
            }

            return token;
        }

        private static bool VerifyCredentials(User foundUser, UserLogin userLogin)
        {
            return CompareByteArrays(foundUser.Password, GenerateSaltedHash(Encoding.UTF8.GetBytes(userLogin.Password), foundUser.PasswordSalt));
        }

        private AccessToken GenerateAccessToken(User user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration.GetSection("Jwt")["Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.EmailAddress),
                new Claim(ClaimTypes.GivenName, user.FirstName),
                new Claim(ClaimTypes.Surname, user.LastName),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var audience = HttpContext.Request.Host.Value;
            var expires = DateTime.UtcNow.AddMinutes(15);
            var token = new JwtSecurityToken(
                issuer: _configuration.GetSection("Jwt")["Issuer"],
                audience: audience,
                claims: claims,
                expires: expires,
                signingCredentials: credentials
                );

            return new AccessToken{
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                Expiration = expires
            };
        }

        private string GenerateIdToken(User user, DateTime expires)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration.GetSection("Jwt")["Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username)
            };

            var audience = HttpContext.Request.Host.Value;
            var idToken = new JwtSecurityToken(
                issuer: _configuration.GetSection("Jwt")["Issuer"],
                audience: audience,
                claims: claims,
                expires:expires,
                signingCredentials: credentials
                );

            return new JwtSecurityTokenHandler().WriteToken(idToken);
        }

        private void SetIdToken(string idToken, DateTime expires)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = expires
            };
            Response.Cookies.Append("idToken", idToken, cookieOptions);
        }

        private static RefreshToken GenerateRefreshToken()
        {
            return new RefreshToken
            {
                Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
                Expires = DateTime.UtcNow.AddDays(5),
                Created = DateTime.UtcNow
            };
        }

        private async Task<string> SetRefreshToken(RefreshToken refreshToken, User foundUser)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = refreshToken.Expires
            };
            Response.Cookies.Append("refreshToken", refreshToken.Token, cookieOptions);

            foundUser.RefreshToken = refreshToken.Token;
            _context.Entry(foundUser).Property("RefreshToken").IsModified = true;
            await _context.SaveChangesAsync();

            return refreshToken.Token;
        }

        private static byte[] CreateRandomToken(int bytes)
        {
            return RandomNumberGenerator.GetBytes(bytes);
        }

        private async Task<IActionResult> SendVerificationEmail(User user)
        {
            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse(_configuration.GetSection("EmailConfig")["From"]));
            email.To.Add(MailboxAddress.Parse(user.EmailAddress));
            email.Subject = "Adventurer's Guild - Verify your email";
            string link = GenerateLink(user.VerificationToken, "verify-email");
            email.Body = new TextPart(MimeKit.Text.TextFormat.Html) { Text =  $"Hello {user.Username}, here is your verification link: <a href={link}> {link} </a>" };

            using var client = new SmtpClient();
            try
            {
                await client.ConnectAsync(_configuration.GetSection("EmailConfig")["SmtpServer"], int.Parse(_configuration.GetSection("EmailConfig")["Port"]), SecureSocketOptions.StartTls);
                //client.AuthenticationMechanisms.Remove("XOAUTH2");
                await client.AuthenticateAsync(_configuration.GetSection("EmailConfig")["Username"], _configuration.GetSection("EmailConfig")["Password"]);

                await client.SendAsync(email);
            }
            catch
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
                return BadRequest("Something went wrong");
            }
            finally
            {
                await client.DisconnectAsync(true);
                client.Dispose();
            }
            return Ok("Verification sent");
        }


        private async Task<IActionResult> SendResetEmail(User user)
        {
            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse(_configuration.GetSection("EmailConfig")["From"]));
            email.To.Add(MailboxAddress.Parse(user.EmailAddress));
            email.Subject = "Adventurer's Guild - Reset Your Password";
            string link = GenerateLink(user.ResetPasswordToken, "reset-password");
            email.Body = new TextPart(MimeKit.Text.TextFormat.Html) { Text = $"Hello {user.Username}, click this link to reset your password: <a href={link}> {link} </a>"};

            using var client = new SmtpClient();
            try
            {
                await client.ConnectAsync(_configuration.GetSection("EmailConfig")["SmtpServer"], int.Parse(_configuration.GetSection("EmailConfig")["Port"]), SecureSocketOptions.StartTls);
                //client.AuthenticationMechanisms.Remove("XOAUTH2");
                await client.AuthenticateAsync(_configuration.GetSection("EmailConfig")["Username"], _configuration.GetSection("EmailConfig")["Password"]);

                await client.SendAsync(email);
            }
            catch
            {
                return BadRequest();
            }
            finally
            {
                await client.DisconnectAsync(true);
                client.Dispose();
            }
            return Ok("Password reset link sent");
        }

        private string GenerateLink(byte[] token, string endpoint)
        {
            StringBuilder sb = new();
            sb.Append(Request.Scheme);
            sb.Append("://");
            sb.Append(Request.Host);
            sb.Append("/api/Auth/");
            sb.Append(endpoint);
            sb.Append("?token=");
            sb.Append(Convert.ToBase64String(token));
            return sb.ToString();
        }

        private static byte[] GenerateSaltedHash(byte[] plainText, byte[] salt)
        {
            var algorithm = SHA256.Create();

            byte[] plainTextWithSaltBytes =
              new byte[plainText.Length + salt.Length];

            for (int i = 0; i < plainText.Length; i++)
            {
                plainTextWithSaltBytes[i] = plainText[i];
            }
            for (int i = 0; i < salt.Length; i++)
            {
                plainTextWithSaltBytes[plainText.Length + i] = salt[i];
            }

            return algorithm.ComputeHash(plainTextWithSaltBytes);
        }

        public static bool CompareByteArrays(byte[] array1, byte[] array2)
        {
            if (array1.Length != array2.Length)
            {
                return false;
            }

            for (int i = 0; i < array1.Length; i++)
            {
                if (array1[i] != array2[i])
                {
                    return false;
                }
            }

            return true;
        }
    }
}
