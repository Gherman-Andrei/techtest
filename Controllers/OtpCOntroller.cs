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
    private readonly Repository.Repository _repository;
    private static readonly TimeSpan CacheTimeout = TimeSpan.FromSeconds(30);

    public OtpController(Repository.Repository test)
    {
        _repository = test;
    }

    [HttpPost("generate")]
    public IActionResult GenerateOtp([FromBody] GenerateOtpRequest request)
    {
        string secretKey = GenerateSecretKey(request.Username);

        var totp = new Totp(Base32Encoding.ToBytes(secretKey));

        long currentTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        string otp = totp.ComputeTotp(DateTime.UtcNow);

        StoreOtp(request.Username, otp, currentTime);
        Console.WriteLine("salut");
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
      _repository.AddUsername(username,otp);
    }
    
    [HttpPost("verify")]
    public IActionResult VerifyOtp([FromBody] VerifyOtpRequest request) {
        string username = request.Username;
        string providedOtp = request.Otp;

        // Check if the username exists in the cache
        string cachedOtp = _repository.GetOtp(username);
        if (string.IsNullOrEmpty(cachedOtp))
        {
            return BadRequest("Invalid username or OTP not generated yet.");
        }

        // Validate the provided OTP against the cached OTP
        if (!ValidateOtp(providedOtp, cachedOtp))
        {
            return Unauthorized("Invalid OTP.");
        }

        return Ok("OTP verified successfully.");
    }
    private bool ValidateOtp(string providedOtp, string cachedOtp)
    {
        return providedOtp == cachedOtp;
    }
    
}