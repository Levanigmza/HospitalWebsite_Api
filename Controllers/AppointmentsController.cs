using HospitalWebsiteApi.Models;
using HospitalWebsiteApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace HospitalWebsiteApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AppointmentController : ControllerBase
    {
        private readonly SqlService _appointmentService;
        private readonly ApplicationDbContext _context;

        public AppointmentController(SqlService appointmentService, ApplicationDbContext context)
        {
            _appointmentService = appointmentService;
            _context = context;
        }

        [Authorize]
        [HttpPost("schedule")]
        public async Task<IActionResult> ScheduleAppointment([FromBody] Appointment appointment)
        {
            var userIdClaim = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized();
            }
            if (await _appointmentService.IsDoctorAppointmentSlotAvailableAsync(appointment.DoctorId, appointment.Date, appointment.Time))
            {
                if (await _appointmentService.IsUserAppointmentSlotAvailableAsync(appointment.UserId, appointment.Date, appointment.Time))
                {
                    try
                    {
                        int appointmentId = await _appointmentService.ScheduleAppointmentAsync(appointment);

                        var response = new AppointmentResponse
                        {
                            AppointmentId = appointmentId,
                            UserId = appointment.UserId,
                            DoctorId = appointment.DoctorId,
                            Date = appointment.Date,
                            Time = appointment.Time,
                            Comment = appointment.Comment
                        };

                        return Ok(response);
                    }
                    catch (Exception ex)
                    {
                        return BadRequest("Appointment scheduling failed: " + ex.Message);
                    }
                }
                else
                {
                    return BadRequest("User have another Appointment on this Date and time. Please choose another date or time.");
                }
            }
            else
            {
                return BadRequest("Doctor's Appointment slot not available. Please choose another date or time.");
            }
        }


        [Authorize]
        [HttpGet("doctor-appointments/{doctorId}")]
        public async Task<ActionResult<IEnumerable<AppointmentResponse>>> GetDoctorAppointments(string doctorId)
        {
            try
            {
                var doctorAppointments = await _context.Appointments
                    .Where(appointment => appointment.DoctorId == doctorId) // Filter appointments by doctorId
                    .Select(appointment => new AppointmentResponse
                    {
                        AppointmentId = appointment.Id,
                        UserId = appointment.UserId,
                        DoctorId = appointment.DoctorId,
                        Date = appointment.Date,
                        Time = appointment.Time,
                        Comment = appointment.Comment,
                    })
                    .ToListAsync();

                return Ok(doctorAppointments);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error: {ex.Message}");
            }
        }





        [Authorize(Policy = "DoubleRolePolicy")]
        [HttpDelete("delete/{appointmentId}")]
        public async Task<IActionResult> DeleteAppointment(int appointmentId)


        {
            var userIdClaim = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier);



            if (!int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized();
            }
            try
            {

                var appointmentToDelete = await _context.Appointments.FindAsync(appointmentId);

                if (appointmentToDelete == null)
                {
                    return NotFound($"Appointment with ID {appointmentId} not found.");
                }

                if (appointmentToDelete.UserId == userId.ToString() || appointmentToDelete.DoctorId == userId.ToString())
                {


                    _context.Appointments.Remove(appointmentToDelete);
                    await _context.SaveChangesAsync();

                    return Ok($"Appointment with ID {appointmentId} deleted successfully.");


                }
                else
                {
                    return BadRequest("access denided");
                }



            }
            catch (Exception ex)
            {
                return BadRequest($"Error deleting appointment: {ex.Message}");
            }
        }



        [Authorize(Roles = "2")]
        [HttpDelete("admin_delete/{appointmentId}")]
        public async Task<IActionResult> DeleteAppointment_Admin(int appointmentId)
        {



            try
            {

                var appointmentToDelete = await _context.Appointments.FindAsync(appointmentId);

                if (appointmentToDelete == null)
                {
                    return NotFound($"Appointment with ID {appointmentId} not found.");
                }
                else
                {


                    _context.Appointments.Remove(appointmentToDelete);
                    await _context.SaveChangesAsync();

                    return Ok($"Appointment with ID {appointmentId} deleted successfully.");


                }



            }
            catch (Exception ex)
            {
                return BadRequest($"Error deleting appointment: {ex.Message}");
            }
        }







        [Authorize]
        [HttpGet("user-appointments")]
        public async Task<ActionResult<IEnumerable<AppointmentResponse>>> GetUserAppointments()
        {
            var userIdClaim = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized();
            }

            try
            {
                var userAppointments = await _context.Appointments
                    .Where(appointment => appointment.UserId == userId.ToString())
                    .Select(appointment => new AppointmentResponse
                    {
                        AppointmentId = appointment.Id,
                        UserId = appointment.UserId,
                        DoctorId = appointment.DoctorId,
                        Date = appointment.Date,
                        Time = appointment.Time,
                        Comment = appointment.Comment,
                    })
                    .ToListAsync();

                return Ok(userAppointments);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error: {ex.Message}");
            }
        }

        [Authorize(Roles = "2")]
        [HttpGet("user-appointments_Admin/{userid}")]
        public async Task<ActionResult<IEnumerable<AppointmentResponse>>> GetUserAppointments_Admin(int userId)
        {

            try
            {
                var userAppointments = await _context.Appointments
                    .Where(appointment => appointment.UserId == userId.ToString())
                    .Select(appointment => new AppointmentResponse
                    {
                        AppointmentId = appointment.Id,
                        UserId = appointment.UserId,
                        DoctorId = appointment.DoctorId,
                        Date = appointment.Date,
                        Time = appointment.Time,
                        Comment = appointment.Comment,
                    })
                    .ToListAsync();

                return Ok(userAppointments);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error: {ex.Message}");
            }
        }


    }

}
