using System.Collections.Concurrent;
using Microsoft.AspNetCore.Mvc;
using d1123.DTO;

namespace d1123.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VerifyOtpController(OtpController otpController) : Controller
    {
        // Define OtpCache as a static dictionary
        private static readonly IDictionary<string, OtpCacheEntry> OtpCache =
            new ConcurrentDictionary<string, OtpCacheEntry>();

        private readonly OtpController _otpController = otpController; // Reference to OtpController

        private static readonly TimeSpan CacheTimeout = TimeSpan.FromSeconds(30);

        [HttpPost("verify")]
        public IActionResult VerifyOtp([FromBody] VerifyOtpRequest request)
        {
            var username = request.Username;
            var otp = request.Otp;

            // Check if the username exists in the cache
            if (!OtpCache.ContainsKey(username))
            {
                return BadRequest("Invalid username or OTP not generated yet.");
            }

            // Retrieve the cached OTP and timestamp for the username
            var cachedOtpEntry = OtpCache[username];
            string cachedOtp = cachedOtpEntry.Otp;
            long cachedTimestamp = cachedOtpEntry.GenerationTimestamp;

            // Validate the provided OTP against the cached OTP
            if (!ValidateOtp(otp, cachedOtp))
            {
                return Unauthorized("Invalid OTP.");
            }

            // Check if the OTP is still valid within the cache timeout
            if ((cachedTimestamp + CacheTimeout.Milliseconds) < DateTimeOffset.UtcNow.ToUnixTimeMilliseconds())
            {
                return Unauthorized("OTP expired.");
            }

            // OTP is valid, return a success response
            return Ok("OTP verified successfully.");
        }

        private bool ValidateOtp(string providedOtp, string cachedOtp)
        {
            return providedOtp == cachedOtp;
        }
    }
    public class OtpCacheEntry
    {
        public string Otp { get; set; }
        public long GenerationTimestamp { get; set; }
    }
}

