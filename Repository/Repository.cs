using System.Collections.Concurrent;

namespace d1123.Repository;

public class Repository
{
    public  ConcurrentDictionary<string, string> OtpCache;

    public Repository()
    {
        this.OtpCache = new ConcurrentDictionary<string, string>();
    }

    public void AddUsername(string username, string otp)
    {
        this.OtpCache.AddOrUpdate(username, otp, (key, oldValue) => otp);
    } public string GetOtp(string username)
    {
        this.OtpCache.TryGetValue(username, out string otp);
        return otp;
    }
    
}