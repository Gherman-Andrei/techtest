using System;
using System.Collections.Generic;
using System.IO;

namespace d1123.Repository
{
    public class FileRepository
    {
        private readonly string _filePath = "otp_data.txt";
        private readonly TimeSpan _otpValidity = TimeSpan.FromSeconds(30); // Durata de valabilitate a OTP-ului

        private void UpdateFileContents(List<string> lines)
        {
            File.WriteAllLines(_filePath, lines);
        }

        public long GetOtpExpiry(string username)
        {
            string[] lines = File.ReadAllLines(_filePath);

            foreach (string line in lines)
            {
                string[] parts = line.Split(',');
                if (parts.Length == 3 && parts[0] == username)
                {
                    long expiryTimestamp = long.Parse(parts[2]);
                    long currentTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                    if (expiryTimestamp >= currentTimestamp)
                    {
                        return expiryTimestamp; // Returnează timestamp-ul de expirare
                    }
                    else
                    {
                        ExpireOtp(username); // Expiră OTP-ul după ce a fost folosit
                        break;
                    }
                }
            }

            return 0; // Returnează 0 dacă nu găsește timestamp-ul
        }

        private void ExpireOtp(string username)
        {
            string[] lines = File.ReadAllLines(_filePath);
            List<string> updatedLines = new List<string>();

            foreach (string line in lines)
            {
                string[] parts = line.Split(',');
                if (parts.Length == 3 && parts[0] == username)
                {
                    parts[2] = "0"; // Setează timestamp-ul de expirare la 0 (expirat)
                }
                updatedLines.Add(string.Join(",", parts));
            }

            UpdateFileContents(updatedLines);
        }

        public void SaveOtp(string username, string otp, long generationTimestamp)
        {
            long expiryTimestamp = generationTimestamp + (long)_otpValidity.TotalMilliseconds;

            // Citim mai întâi conținutul existent al fișierului
            List<string> lines = new List<string>();
            if (File.Exists(_filePath))
            {
                lines.AddRange(File.ReadAllLines(_filePath));
            }

            // Adăugăm sau actualizăm linia OTP-ului pentru utilizatorul dat
            bool foundUser = false;
            for (int i = 0; i < lines.Count; i++)
            {
                string[] parts = lines[i].Split(',');
                if (parts.Length == 3 && parts[0] == username)
                {
                    parts[1] = otp; // Actualizăm OTP-ul
                    parts[2] = expiryTimestamp.ToString(); // Actualizăm timestamp-ul de expirare
                    lines[i] = string.Join(",", parts);
                    foundUser = true;
                    break;
                }
            }

            if (!foundUser)
            {
                lines.Add($"{username},{otp},{expiryTimestamp}");
            }

            UpdateFileContents(lines);
        }

        public string GetOtp(string username)
        {
            string[] lines = File.ReadAllLines(_filePath);

            foreach (string line in lines)
            {
                string[] parts = line.Split(',');
                if (parts.Length == 3 && parts[0] == username)
                {
                    long expiryTimestamp;
                    if (long.TryParse(parts[2], out expiryTimestamp))
                    {
                        if (expiryTimestamp >= DateTimeOffset.UtcNow.ToUnixTimeMilliseconds())
                        {
                            return parts[1]; // Return OTP
                        }
                        else
                        {
                            // Ștergem OTP-ul expirat
                            ExpireOtp(username);
                            break;
                        }
                    }
                }
            }

            return null; // OTP not found or expired
        }

        public string VerifyAndExpireOtp(string username, string providedOtp)
        {
            string cachedOtp = GetOtp(username);

            if (string.IsNullOrEmpty(cachedOtp))
            {
                return "Invalid"; // OTP nu există sau username invalid
            }

            if (cachedOtp != providedOtp)
            {
                return "Invalid"; // OTP incorect
            }

            return "Valid"; // OTP corect și neexpirat
        }
    }
}
