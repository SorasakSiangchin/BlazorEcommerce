using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;

namespace BlazorEcommerce.Server.Services.AuthService
{
    public class AuthService : IAuthService
    {
        private readonly DataContext _context;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public AuthService(DataContext context , IConfiguration configuration , IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
        }

        public int GetUserId () => int.Parse(_httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));
        public string GetUserEmail() => _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.Name);
        public async Task<ServiceResponse<string>> Login(string email, string password)
        {
            var response = new ServiceResponse<string>();
            var user = await _context.Users.FirstOrDefaultAsync(e => e.Email.ToLower() == email.ToLower());
            if (user == null)
            {
                response.Success = false;
                response.Message = "User not found!";
            }

            //VerifyPasswordHash จะรับพารามิเตอร์ 3 ตัว คือ รหัสที่กรอก , รหัสผ่านที่ผ่านการ Hash จาก Database ,เกลือ จาก Database
            else if (!VerifyPasswordHash(password , user.PasswordHash , user.PasswordSalt))
            {
                response.Success = false;
                response.Message = "Wrong password!";
            }
            else
            {
                response.Data = CreateToken(user);
            }
            return response;
        }

        public async Task<ServiceResponse<int>> Register(User user, string password)
        {
            var response = new ServiceResponse<int>();
            if (await UserExists(user.Email))
            {
                response.Success = false;
                response.Message = "User already exists.";
                return response;
            }

            CreatePasswordHash(password, out byte[] passwordHash, out byte[] passwordSalt);

            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;

            await _context.AddAsync(user);
            await _context.SaveChangesAsync();
            response.Data = user.Id;
            response.Message = "Registration Successful!";
            return response;
        }

        public async Task<bool> UserExists(string email)
        {
            // AnyAsync => 
            if (await _context.Users.AnyAsync(e => e.Email.ToLower().Equals(email.ToLower())))
            {
                return true;
            }
            return false;
        }

        private  void CreatePasswordHash(string password , out byte[] passwordHash , out byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512())
            {
                passwordSalt = hmac.Key;
                // นำรหัสผ่านไป hash
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }

        private bool VerifyPasswordHash(string password , byte[] passwordHash , byte[] passwordSalt)
        {
            // นำเกลือใส่ในอัลกอริทึมก่อน
            using (var hmac = new HMACSHA512(passwordSalt))
            {
                var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                // SequenceEqual เป็นตัวเปรียบเทียบ
                return computedHash.SequenceEqual(passwordHash);
            }
        }

        private string CreateToken(User user)
        {
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier,user.Id.ToString()),
                new Claim(ClaimTypes.Name , user.Email),
                new Claim(ClaimTypes.Role , user.Role),
            };
            // ดึง key มาทำการเข้ารหัส
            var key = new SymmetricSecurityKey(
                System.Text.Encoding.UTF8.GetBytes(_configuration.GetSection("AppSettings:Token").Value));
            // จะเข้ารหัสด้วยวิธีอะไร
            var creds = new SigningCredentials(key , SecurityAlgorithms.HmacSha256Signature);
            //สร้าง Token
            var token = new JwtSecurityToken(
                claims : claims ,
                expires : DateTime.Now.AddDays(1) ,
                signingCredentials : creds
                );
            // WriteToken เขียนออกมา เป็น string
            var jwt = new JwtSecurityTokenHandler().WriteToken(token);
            return jwt;
        }

        public async Task<ServiceResponse<bool>> ChangePassword(int userId, string newPassword)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return new ServiceResponse<bool>
                {
                    Message = "User not found.",
                    Data = false,
                };
            };

            CreatePasswordHash(newPassword , out byte[] passwordHash, out byte[] passwordSalt);

            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;

           await _context.SaveChangesAsync();

          return new ServiceResponse<bool>
          {
              Message = "Password has been changed.",
              Data = true,
          };
        }

        public async Task<User> GetUserByEmail(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(x => x.Email == email);
        }
    }
}
