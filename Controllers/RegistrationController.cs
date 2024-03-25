using HospitalWebsiteApi.Models;
using HospitalWebsiteApi.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using HospitalWebsiteApi;
using Microsoft.AspNetCore.Authorization;
using System.Data;
using System.Security.Claims;
using System.Numerics;
using Microsoft.EntityFrameworkCore;

namespace HospitalWebsiteApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RegistrationController : ControllerBase
    {
        private readonly SqlService _userService;

        public RegistrationController(SqlService userService)
        {
            _userService = userService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> RegisterUser([FromBody] UserParameters user)
        {
            if (_userService.IsEmailRegistered(user.Email))
            {
                return StatusCode(409);
            }
            else
                try
                {
                    int userId = await _userService.RegisterUserAsync(user);

                    var response = new RegistrationResponse
                    {
                        Name = user.Name,
                        Surname = user.Surname,
                        Email = user.Email,
                        PersonalID = user.PersonalID,
                    };

                    return Ok(response);
                }
                catch (Exception ex)
                {
                    return BadRequest("Registration failed: " + ex.Message);
                }
        }


        [Authorize]
        [HttpPost("register-doctor")]
        public async Task<IActionResult> RegisterDoctorUser([FromForm] DoctorRegistrationRequest doctorRequest)
        {
            var userIdClaim = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized();
            }

            var user = await _userService.GetUserByIdAsync(userId);

            if (user.UserRole == 2)
            {
                if (_userService.IsEmailRegistered(doctorRequest.Doctor.Email))
                {
                    return BadRequest("Error: Email is already registered!");
                }
                else
                {
                    try
                    {
                        int newDoctorUserId = await _userService.RegisterDoctorAsync(doctorRequest.Doctor, doctorRequest.Position, doctorRequest.Photo);

                        var response = new RegistrationResponse
                        {
                            Name = doctorRequest.Doctor.Name,
                            Surname = doctorRequest.Doctor.Surname,
                            Email = doctorRequest.Doctor.Email,
                            PersonalID = doctorRequest.Doctor.PersonalID,
                        };

                        return Ok(response);
                    }
                    catch (Exception ex)
                    {
                        return BadRequest("Registration failed: " + ex.Message);
                    }
                }
            }
            else
            {
                return Forbid();
            }
        }


        [HttpGet("doctors")]
        public async Task<IActionResult> GetDoctors()
        {
            try
            {
                var doctors = await _userService.GetDoctorsAsync();

                var doctorResponses = new List<DoctorInfo>();

                foreach (var doctor in doctors)
                {
                    var position = await _userService.GetDoctorPositionAsync(doctor.Id);

                    var doctorResponse = new DoctorInfo
                    {
                        Id = doctor.Id,
                        Name = doctor.Name,
                        Email = doctor.Email,
                        Surname = doctor.Surname,
                        Position = position,
                        Photo = doctor.Photo,
                        PersonalID=doctor.PersonalID.ToString(),
                        
                    };

                    doctorResponses.Add(doctorResponse);
                }

                return Ok(doctorResponses);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred: " + ex.Message);
            }
        }



        [Authorize]
        [HttpPut("update/photo")]
        public async Task<IActionResult> UpdateUserPhoto([FromForm] User model)
        {
            var userIdClaim = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdClaim?.Value, out int userId))
            {
                return Unauthorized();
            }

            try
            {
                await _userService.UpdateUserPhotoAsync(userId, model.Photo);
                return Ok("User photo updated successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }





    }

}


