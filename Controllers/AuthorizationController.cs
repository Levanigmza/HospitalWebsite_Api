using HospitalWebsiteApi.Models;
using HospitalWebsiteApi.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using HospitalWebsiteApi;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Data;
using Microsoft.IdentityModel.Tokens;
using System.Globalization;
using Microsoft.AspNetCore.Identity;
using System.Numerics;
using Microsoft.EntityFrameworkCore;
using Azure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.AspNetCore.Http;

namespace HospitalWebsiteApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthorizationController : ControllerBase
    {
        private readonly SqlService _userService;
        private readonly JwtService _jwtService;
        private readonly ApplicationDbContext _context;

        private const string StaticOtpCode = "1111";


        public AuthorizationController(SqlService userService, JwtService jwtService, ApplicationDbContext context)
        {
            _userService = userService;
            _jwtService = jwtService;
            _context = context;

        }
        [HttpPost("Auth")]
        public async Task<IActionResult> Auth([FromBody] LoginModel login)
        {

            var user = await _userService.FindByEmailAsync(login.Email);

            if (user != null)
            {
                var storedPasswordHash = user.Password;

                if (user.UserRole == 2)
                {
                    if (_userService.VerifyPassword(login.Password, storedPasswordHash))
                    {

                        var token = _jwtService.GenerateJwtToken(user.Email, user.Id, user.UserRole.ToString());
                        string userRole = GetUserRole(user.UserRole);
                        return Ok(new { Token = token, UserRole = userRole });
                    }
                    else
                    {
                        return BadRequest("Invalid Admin's email or password");
                    }
                }
                else
                {
                    if (_userService.VerifyPassword(login.Password, storedPasswordHash))
                    {
                        string userRole = GetUserRole(user.UserRole);

                        var token = _jwtService.GenerateJwtToken(user.Email, user.Id, user.UserRole.ToString());
                        return Ok(new { Token = token, UserRole = userRole });
                    }
                    else
                    {
                        return BadRequest("Invalid email or password");
                    }
                }
            }
            else
            {
                return BadRequest("Invalid email or password");
            }
        }
        private string GetUserRole(int userRole)
        {
            switch (userRole)
            {
                case 0:
                    return "User";
                case 1:
                    return "Doctor";
                case 2:
                    return "Admin";
                default:
                    return "Unknown";
            }
        }

        [Authorize]
        [HttpGet("user")]
        public async Task<IActionResult> GetUser()
        {
            var userIdClaim = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null)
            {
                return Unauthorized();
            }

            if (!int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized();
            }


            var user = await _userService.GetUserByIdAsync(userId);

            if (user == null)
            {
                return NotFound("User not found.");
            }
            if (user.UserRole == 1)
            {
                var position = await _userService.GetDoctorPositionAsync(user.Id);

                var DoctorResponse = new

                {

                    DoctorId = user.Id,
                    Name = user.Name,
                    Surname = user.Surname,
                    Email = user.Email,
                    PersonalId = user.PersonalID,
                    //Phone = user.Phone,
                    Photo = user.Photo,
                    Position = position,

                };

                return Ok(DoctorResponse);

            }
            if (user.UserRole == 2)
            {
                var AdminResponse = new
                {
                    Name = user.Name,
                    Surname = user.Surname,
                    Email = user.Email,
                    PersonalId = user.PersonalID,

                };

                return Ok(AdminResponse);
            }
            else
            {

                var userResponse = new
                {
                    UserId = user.Id,
                    Name = user.Name,
                    Surname = user.Surname,
                    Email = user.Email,
                    //Phone = user.Phone,
                    //Birthdate = user.Birthdate?.ToString("yyyy-MM-dd"),
                    PersonalId = user.PersonalID,
                    Photo = user.Photo,

                };

                return Ok(userResponse);
            }
        }


        [Authorize]
        [HttpPut("user/update")]
        public async Task<IActionResult> UpdateUserAsync([FromBody] UserParameters userUpdate)
        {
            if (userUpdate == null)
            {
                return BadRequest("Invalid user update data.");
            }

            var userIdClaim = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized();
            }

            try
            {


                if (userUpdate.Name != null)
                {
                    await _userService.UpdateUserNameAsync(userId, userUpdate.Name);
                }
                if (userUpdate.PersonalID != null)
                {
                    await _userService.UpdateUserPersonalIdAsync(userId, (int)userUpdate.PersonalID);
                }
                if (userUpdate.Surname != null)
                {
                    await _userService.UpdateSurnameAsync(userId, userUpdate.Surname);
                }
                if (userUpdate.Phone != null)
                {
                    await _userService.UpdateUserPhoneAsync(userId, (int)userUpdate.Phone);
                }
                if (userUpdate.Birthdate != null)
                {
                    await _userService.UpdateUserBirthdateAsync(userId, userUpdate.Birthdate);
                }
                if (userUpdate.Address != null)
                {
                    await _userService.UpdateUserAddressAsync(userId, userUpdate.Address);
                }


                return Ok(userId);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred: " + ex.Message);
            }
        }

        [Authorize(Roles = "2")]
        [HttpPost("admin/update")]
        public async Task<IActionResult> AdminUpdateUser([FromBody] UserParameters userUpdate)
        {
            try
            {
                if (userUpdate.Id != null)
                {
                    if (userUpdate.Name != null)
                    {
                        await _userService.UpdateUserNameAsync(userUpdate.Id, userUpdate.Name);
                    }
                    if (userUpdate.PersonalID != null)
                    {
                        await _userService.UpdateUserPersonalIdAsync(userUpdate.Id, (int)userUpdate.PersonalID);
                    }
                    if (userUpdate.Surname != null)
                    {
                        await _userService.UpdateSurnameAsync(userUpdate.Id, userUpdate.Surname);
                    }
                    if (userUpdate.Phone != null)
                    {
                        await _userService.UpdateUserPhoneAsync(userUpdate.Id, (int)userUpdate.Phone);
                    }
                    if (userUpdate.Birthdate != null)
                    {
                        await _userService.UpdateUserBirthdateAsync(userUpdate.Id, userUpdate.Birthdate);
                    }
                    if (userUpdate.Address != null)
                    {
                        await _userService.UpdateUserAddressAsync(userUpdate.Id, userUpdate.Address);
                    }

                    return Ok(userUpdate.Id);
                }
                else
                {
                    return BadRequest("User ID must be provided for admin updates.");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred: " + ex.Message);
            }
        }



        [Authorize(Roles = "2")]
        [HttpDelete("DeleteUser/{userid}")]
        public async Task<IActionResult> DeleteUserData(int userid)
        {
            try
            {
                var userForDelete = await _context.UserParameters.FindAsync(userid);

                if (userForDelete == null)
                {
                    return NotFound($"User with ID {userid} not found.");
                }

                var userRole = userForDelete.UserRole;

                _context.UserParameters.Remove(userForDelete);
                await _context.SaveChangesAsync();

                if (userRole == 1)
                {
                    var doctorAppointments = await _context.Appointments.Where(a => a.DoctorId == userid.ToString()).ToListAsync();
                    _context.Appointments.RemoveRange(doctorAppointments);

                    var doctorRoles = await _context.Doctor_Roles.Where(dr => dr.DoctorID == userid.ToString()).ToListAsync();
                    _context.Doctor_Roles.RemoveRange(doctorRoles);
                }
                else if (userRole == 0)
                {
                    var userAppointments = await _context.Appointments.Where(a => a.UserId == userid.ToString()).ToListAsync();
                    _context.Appointments.RemoveRange(userAppointments);
                }

                await _context.SaveChangesAsync();

                return Ok($"User with ID {userid} deleted successfully.");
            }
            catch (Exception ex)
            {
                return BadRequest($"Error deleting user: {ex.Message}");
            }
        }










        [Authorize]
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserInfo(int UserId)
        {

            var userIdClaim = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null)
            {
                return Unauthorized();
            }

            if (!int.TryParse(userIdClaim.Value, out int userID))
            {
                return Unauthorized();
            }


            var user = await _userService.GetUserByIdAsync(userID);

            if (user == null)
            {
                return NotFound("User not found.");
            }

            if (user != null)
            {
                var user2 = await _userService.GetUserByIdAsync(UserId);

                var userInfo = new
                {
                    User = user2.Name + " " + user2.Surname + " " + user2.Email,

                };

                return Ok(userInfo.User);

            }
            else
            {
                return NotFound("error");
            }
        }


        [Authorize(Roles = "2")]
        [HttpGet("usersAll")]
        public async Task<IActionResult> GetAllUsers()
        {
            try
            {
                var users = await _userService.GetAllUsersAsync();

                var adminUsers = users.Select(u => new
                {
                    Id = u.Id,
                    Name = u.Name,
                    Surname = u.Surname,
                    personalId = u.PersonalID,
                    Email = u.Email,
                    Photo = u.Photo,

                }).ToList();

                return Ok(adminUsers);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while fetching users: {ex.Message}");
            }
        }

        /*        [Authorize(Roles = "2")]
                [HttpDelete("DeleteUser/{userId}")]
                public async Task<IActionResult> DeleteUserAndAppointments(int userId)
                {
                    try
                    {
                        // Delete the user and associated appointments
                        var deletedUser = await _userService.DeleteUserAndAppointmentsAsync(userId);

                        if (deletedUser == null)
                        {
                            return NotFound("User not found.");
                        }

                        return Ok(deletedUser);
                    }
                    catch (Exception ex)
                    {
                        return StatusCode(500, "An error occurred: " + ex.Message);
                    }
                }*/


        [HttpPost("code")]
        public async Task<IActionResult> GenerateOTP([FromBody] EmailRequest request)
        {

            if (string.IsNullOrEmpty(request.Email))
            {
                return BadRequest("Email is required.");
            }
            var user = _context.UserParameters.FirstOrDefault(u => u.Email == request.Email);
            if (user == null)
            {
                return NotFound("User not found.");
            }


            string otpCode = GenerateRandomOTP(4);


            //otpCode = "1111";
            return Ok(new { OtpCode = otpCode });
        }

        private string GenerateRandomOTP(int length)
        {
            const string characters = "0123456789";
            Random random = new Random();
            char[] otp = new char[length];
            for (int i = 0; i < length; i++)
            {
                otp[i] = characters[random.Next(characters.Length)];

            }
            return new string(otp);
        }

        [HttpPost("checkCode")]
        public IActionResult CheckOTP([FromBody] CheckOTPRequest otpRequest)
        {
            if (string.IsNullOrEmpty(otpRequest.Email))
            {
                return BadRequest("Email is required.");
            }
            else if (otpRequest == null || string.IsNullOrEmpty(otpRequest.otpCode.ToString()))
            {
                return BadRequest("Invalid OTP code.");
            }

            if (int.TryParse(otpRequest.otpCode.ToString(), out int otpCode))
            {
                if (otpCode == Int32.Parse(StaticOtpCode))
                {
                    return Ok(new { Valid = true, Message = "OTP code is correct." });
                }
                else
                {
                    return Ok(new { Valid = false, Message = "OTP code is incorrect." });
                }
            }
            else
            {
                return BadRequest("Invalid OTP code format.");
            }
        }


        [HttpPut("updatePassword")]
        public async Task<IActionResult> UpdatePassword([FromBody] UpdatePasswordRequest request)
        {
            if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.NewPassword))
            {
                return BadRequest("Email and new password are required.");
            }

            var user = _context.UserParameters.FirstOrDefault(u => u.Email == request.Email);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            string hashedPassword = _userService.HashPassword(request.NewPassword);

            user.Password = hashedPassword;
            await _context.SaveChangesAsync();

            return Ok(new{ Message = "Password updated successfully." });
        }







    }


}

