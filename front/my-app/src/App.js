import React, { useState } from 'react';
import './App.css';

function App() {
  const [username, setUsername] = useState('');
  const [otp, setOtp] = useState('');
  const [toastMessage, setToastMessage] = useState('');
  const [generatedOtp, setGeneratedOtp] = useState('');

  const handleGenerateOtp = async () => {
    try {
      const response = await fetch('http://localhost:5077/api/Otp/generate', {
        method: 'POST',
        body: JSON.stringify({ username }),
        headers: {
          'Content-Type': 'application/json',
        },
      });

      if (response.ok) {
        const data = await response.json();
        const generatedOtp = data.otp;

        setToastMessage('OTP generated successfully!');
        setGeneratedOtp(generatedOtp); // Set the generated OTP in the state
      } else {
        // Handle non-200 status code errors
        const errorMessage = await response.text();
        console.error('Error generating OTP:', errorMessage);
        setToastMessage('Error generating OTP. Please try again.');
      }
    } catch (error) {
      console.error('Error during fetch:', error);
      setToastMessage('Error generating OTP. Please try again.');
    }
  };

  const handleLogin = async () => {
    try {
      const response = await fetch('http://localhost:5077/api/Otp/verify', {
        method: 'POST',
        body: JSON.stringify({ username, otp }), // Trimite username și otp către backend
        headers: {
          'Content-Type': 'application/json',
        },
      });

      const result = await response.json();
      if (result.message === 'OTP verified successfully.') {
        console.log('Login successful!');
        setToastMessage('Login successful!');
      } else {
        console.error('Login failed:', result.message);
        setToastMessage('Login failed');
      }
    } catch (error) {
      console.error('Error verifying OTP:', error);
      setToastMessage('Error verifying OTP');
    }
  };

  return (
      <div className="App">
        <h1>OTP Verification</h1>
        <div className="input-fields">
          <label>Username:</label>
          <input type="text" value={username} onChange={(e) => setUsername(e.target.value)} />
          <label>OTP:</label>
          <input type="text" value={otp} onChange={(e) => setOtp(e.target.value)} autoComplete={"off"} />
        </div>
        <div className="buttons">
          <button onClick={handleGenerateOtp}>Generate OTP</button>
          <button onClick={handleLogin}>Login</button>
        </div>
        {toastMessage && (
            <div className="toast">
              {toastMessage}
              {generatedOtp && <p>Generated OTP: {generatedOtp}</p>}
            </div>
        )}
      </div>
  );
}

export default App;
