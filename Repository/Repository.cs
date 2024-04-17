using System.Collections.Concurrent;

namespace d1123.Repository;

public class Repository
{
    private ConcurrentDictionary<string, string> _otpCache = new ConcurrentDictionary<string, string>();


  
    public void AddUsername(string username, string otp)
    {
        _otpCache.TryAdd(username, otp);
    }

    public string GetOtp(string username)
    {
        if (_otpCache.ContainsKey(username))
        {
            return _otpCache[username];
        }
        else
        {
            return null;
        }
    }
    
}