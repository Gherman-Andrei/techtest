using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;


using Microsoft.AspNetCore.Mvc;
using OtpNet;
using d1123.DTO;

namespace d1123.Controllers;

[Route("api/[controller]")]
[ApiController]
public class OtpController : Controller
{
    private static readonly ConcurrentDictionary<string, string> OtpCache = new ConcurrentDictionary<string, string>();
    private static readonly TimeSpan CacheTimeout = TimeSpan.FromSeconds(30);
    [HttpPost("generate")]
    public IActionResult GenerateOtp([FromBody] GenerateOtpRequest request)
    {
        string secretKey = GenerateSecretKey(request.Username);
        
        var totp = new Totp(Base32Encoding.ToBytes(secretKey));
        
        long currentTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        string otp = totp.ComputeTotp(DateTime.UtcNow);
        
        StoreOtp(request.Username, otp,currentTime);

        return Ok(new { otp = otp });
    }

    private string GenerateSecretKey(string username)
    {
        byte[] usernameBytes = Encoding.UTF8.GetBytes(username);
        
        using (var sha256 = SHA256.Create())
        {
            byte[] hashBytes = sha256.ComputeHash(usernameBytes);
            return Base32Encoding.ToString(hashBytes);
        }
    }
    private void StoreOtp(string username, string otp, long generationTimestamp)
    {
        // Check if OTP exists for the user and is still valid within the cache timeout
        if (OtpCache.TryGetValue(username, out var cachedOtp) &&
            (generationTimestamp + CacheTimeout.Milliseconds) >= DateTimeOffset.UtcNow.ToUnixTimeMilliseconds())
        {
            return; // OTP already cached and valid, avoid overwriting
        }

        // Update cache with new OTP and timestamp
        OtpCache.TryUpdate(username, otp, cachedOtp);
    }
    
}z