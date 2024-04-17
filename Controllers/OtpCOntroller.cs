using System.Security.Cryptography;
using System.Text;


using Microsoft.AspNetCore.Mvc;
using OtpNet;
using d1123.DTO;
using d1123.Repository;


namespace d1123.Controllers;
[Route("api/[controller]")]
[ApiController]
public class OtpController : Controller
{
    private FileRepository _fileRepository;
    private static readonly TimeSpan CacheTimeout = TimeSpan.FromSeconds(30);
    public OtpController()
    {
        _fileRepository = new FileRepository();
    }
    
    [HttpPost("generate")]
    public IActionResult GenerateOtp([FromBody] GenerateOtpRequest request)
    {
        string secretKey = GenerateSecretKey(request.Username);

        var totp = new Totp(Base32Encoding.ToBytes(secretKey));

        long currentTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        string otp = totp.ComputeTotp(DateTime.UtcNow);

        StoreOtpInFile(request.Username, otp, currentTime);
        
        
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
    private void StoreOtpInFile(string username, string otp, long generationTimestamp)
    {
        long expiryTimestamp = generationTimestamp + (long)CacheTimeout.TotalMilliseconds;
        _fileRepository.SaveOtp(username, otp, expiryTimestamp);
    }
    
    [HttpPost("verify")]
    public IActionResult VerifyOtp([FromBody] VerifyOtpRequest request)
    {
        string username = request.Username;
        string providedOtp = request.Otp;
        string cachedOtp = _fileRepository.GetOtp(username);

        if (string.IsNullOrEmpty(cachedOtp))
        {
            return BadRequest(new { message = "Invalid username or OTP not generated yet." });
        }

        long expiryTimestamp = _fileRepository.GetOtpExpiry(username);
        if (expiryTimestamp < DateTimeOffset.UtcNow.ToUnixTimeMilliseconds())
        {
            return Unauthorized(new { message = "OTP has expired." });
        }

        if (!ValidateOtp(providedOtp, cachedOtp))
        {
            return Unauthorized(new { message = "Invalid OTP." });
        }

        return Ok(new { message = "OTP verified successfully." });
    }
    
    private bool ValidateOtp(string providedOtp, string cachedOtp)
    {
        return providedOtp == cachedOtp;
    }
}