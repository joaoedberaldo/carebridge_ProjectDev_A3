import React, { useState } from 'react';
import { useForm } from 'react-hook-form';
import { useNavigate } from 'react-router-dom';
import '../../styles/signup.css';

interface LoginFormValues {
  email: string;
  password: string;
}

const Login = () => {
  const { register, handleSubmit, formState: { errors } } = useForm<LoginFormValues>();
  const navigate = useNavigate();
  // Store backend error messages
  const [apiError, setApiError] = useState<string | null>(null); 

  const onSubmit = async (data: LoginFormValues) => {
    // Reset error before new request
    setApiError(null); 
    //check the data content
    // console.log(data);

    try {
      const response = await fetch('http://localhost:5156/api/auth/login', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(data),
      });

      if (response.ok) {
        // console.log('User registered successfully');
        const responseData = await response.json();
        console.log(responseData);
        localStorage.setItem("token", responseData.token); // Store JWT token
        // localStorage.setItem("userId", responseData.id); // Store User ID
        navigate('/dashboard');
      } else {
        // log the server's response to the console.
        const errorData = await response.json();
        console.error('Failed to register user', errorData);

         // Extract meaningful error message
        if (typeof errorData === 'string') {
            setApiError(errorData); // If the API returns a simple string message
          } else if (errorData.message) {
            setApiError(errorData.message); // If API uses `{ message: "Error description" }`
          } else if (errorData.errors) {
            // Handle validation errors (like missing fields)
            const firstError = Object.values(errorData.errors)[0] as string[];
            setApiError(firstError ? firstError[0] : 'Something went wrong.');
          } else {
            setApiError('Registration failed. Please try again.');
        }
      }
    } catch (error) {
      // console.error('Error connecting to the API', error);
      console.error('Error connecting to the API\n', error);
      setApiError('Could not connect to the server. Please try again later.');
    }
  };

  return (
    <div className='signup-body'>
      <div className="signup-container">
        <h1>Welcome Back</h1>
        <p>Don't have an account? <a href="/signup" className="signup-link">Sign up</a></p>
        
        {/* Show backend error */}
        {apiError && <p className="error-message api-error">{apiError}</p>} 
        
        <form onSubmit={handleSubmit(onSubmit)}>
          <label className="input-label" htmlFor="email">Email</label>
          <input className="text-field" id="email" type="email" placeholder="Enter your email" {...register('email', { required: 'Email is required', pattern: { value: /^[^@\s]+@[^@\s]+\.[^@\s]+$/, message: 'Enter a valid email' } })} />
          {errors.email && <p className="error-message">{errors.email.message}</p>}

          <label className="input-label" htmlFor="password">Password</label>
          <input className="text-field" id="password" type="password" placeholder="Enter your password" {...register('password', { required: 'Password is required' })} />
          {errors.password && <p className="error-message">{errors.password.message}</p>}
          
          <div className="button-container">
            <button className="button" type="submit">Login</button>
            <button className="button cancel-button" type="button" onClick={() => navigate('/')}>Cancel</button>
          </div>
        </form>
      </div>
    </div>
  );
};

export default Login;
