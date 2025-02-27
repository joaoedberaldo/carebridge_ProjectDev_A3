import React, { useState } from 'react';
import { useForm } from 'react-hook-form';
import { useNavigate } from 'react-router-dom';
import '../../styles/signup.css';

interface SignUpFormValues {
  firstName: string;
  lastName: string;
  email: string;
  phoneNumber: string;
  password: string;
  role: number;
  dateOfBirth: string;
}

const Signup = () => {
  const { register, handleSubmit, formState: { errors } } = useForm<SignUpFormValues>();
  const navigate = useNavigate();
  // Store backend error messages
  const [apiError, setApiError] = useState<string | null>(null); 


  const onSubmit = async (data: SignUpFormValues) => {
    // Reset error before new request
    setApiError(null); 
      //  console.log(data); //check the data content
    const formattedData = {
      ...data,
      role: Number(data.role), // Ensure role is sent as an integer
    };
      //  console.log(formattedData);// check formated data
    try {
      const response = await fetch('http://localhost:5156/api/auth/register', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(formattedData),
      });

      if (response.ok) {
        const responseData = await response.json();
        console.log(responseData);
        navigate('/dashboard');
      } else {
        // log the server's response to the console.
        // const errorData = await response.json();
        // console.error('Failed to register user', errorData);
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
        <h1>Get Started</h1>
        <p>Already have an account? <a href="/login" className="signup-link">Sign in</a></p>
        
        {/* Show backend error */}
        {apiError && <p className="error-message api-error">{apiError}</p>} 

        
        <form onSubmit={handleSubmit(onSubmit)}>
          <label className="input-label" htmlFor="firstName">First Name</label>
          <input
            className="text-field"
            id="firstName"
            type="text"
            placeholder="Enter your first name"
            {...register('firstName', { required: 'First name is required' })}
          />
          {errors.firstName && <p className="error-message">{errors.firstName.message}</p>}

          <label className="input-label" htmlFor="lastName">Last Name</label>
          <input
            className="text-field"
            id="lastName"
            type="text"
            placeholder="Enter your last name"
            {...register('lastName', { required: 'Last name is required' })}
          />
          {errors.lastName && <p className="error-message">{errors.lastName.message}</p>}

          <label className="input-label" htmlFor="email">Email</label>
          <input
            className="text-field"
            id="email"
            type="email"
            placeholder="Enter your email"
            {...register('email', { 
              required: 'Email is required', 
              pattern: { value: /^[^@\s]+@[^@\s]+\.[^@\s]+$/, message: 'Enter a valid email' } 
            })}
          />
          {errors.email && <p className="error-message">{errors.email.message}</p>}

          <label className="input-label" htmlFor="phoneNumber">Phone Number</label>
          <input
            className="text-field"
            id="phoneNumber"
            type="text"
            placeholder="456-789-0123"
            {...register('phoneNumber', { 
              required: 'Phone number is required', 
              pattern: { value: /^\d{3}-\d{3}-\d{4}$/, message: 'Enter a valid phone number (e.g., 456-789-0123)' } 
            })}
          />
          {errors.phoneNumber && <p className="error-message">{errors.phoneNumber.message}</p>}

          <label className="input-label" htmlFor="password">Password</label>
          <input
            className="text-field"
            id="password"
            type="password"
            placeholder="Enter your password"
            {...register('password', { 
              required: 'Password is required', 
              minLength: { value: 6, message: 'Password must be at least 6 characters' } 
            })}
          />
          {errors.password && <p className="error-message">{errors.password.message}</p>}

          <label className="input-label" htmlFor="role">Role</label>
          <select
            className="text-field"
            id="role"
            {...register('role', { required: 'Role is required' })}
          >
            <option value="">Select Role</option>
            <option value="1">Patient</option>
            <option value="0">Doctor</option>
            <option value="2">Medical Staff</option>
          </select>
          {errors.role && <p className="error-message">{errors.role.message}</p>}

          <label className="input-label" htmlFor="dateOfBirth">Date of Birth</label>
          <input
            className="text-field"
            id="dateOfBirth"
            type="date"
            {...register('dateOfBirth', { required: 'Date of birth is required' })}
          />
          {errors.dateOfBirth && <p className="error-message">{errors.dateOfBirth.message}</p>}

          <div className="button-container">
            <button className="button" type="submit">Sign Up</button>
            <button className="button cancel-button" type="button" onClick={() => navigate('/')}>Cancel</button>
          </div>
        </form>
      </div>
    </div>
  );
};

export default Signup;
