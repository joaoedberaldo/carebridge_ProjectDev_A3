import React from 'react';
import { useForm } from 'react-hook-form';
import { useNavigate } from 'react-router-dom';
import '../../styles/signup.css';

interface SignUpFormValues {
  firstName: string;
  lastName: string;
  email: string;
  password: string;
}

const Signup = () => {
  const { register, handleSubmit, formState: { errors } } = useForm<SignUpFormValues>();
  const navigate = useNavigate();

  const onSubmit = (data: SignUpFormValues) => {
    console.log(data);
    navigate('/dashboard');
  };

  return (
    <div className='signup-body'>
      <div className="signup-container">
        <h1>Get Started</h1>
        <p>Already have an account? <a href="/login" className="signup-link">Sign in</a></p>
        
        <form onSubmit={handleSubmit(onSubmit)}>
          <label className="input-label" htmlFor="firstName">First Name</label>
          <input className="text-field" id="firstName" type="text" placeholder="Enter your first name" {...register('firstName', { required: 'First name is required' })} />
          {errors.firstName && <p className="error-message">{errors.firstName.message}</p>}

          <label className="input-label" htmlFor="lastName">Last Name</label>
          <input className="text-field" id="lastName" type="text" placeholder="Enter your last name" {...register('lastName', { required: 'Last name is required' })} />
          {errors.lastName && <p className="error-message">{errors.lastName.message}</p>}

          <label className="input-label" htmlFor="email">Email</label>
          <input className="text-field" id="email" type="email" placeholder="Enter your email" {...register('email', { required: 'Email is required', pattern: { value: /^[^@\s]+@[^@\s]+\.[^@\s]+$/, message: 'Enter a valid email' } })} />
          {errors.email && <p className="error-message">{errors.email.message}</p>}

          <label className="input-label" htmlFor="password">Password</label>
          <input className="text-field" id="password" type="password" placeholder="Enter your password" {...register('password', { required: 'Password is required', minLength: { value: 6, message: 'Password must be at least 6 characters' } })} />
          {errors.password && <p className="error-message">{errors.password.message}</p>}
          
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
