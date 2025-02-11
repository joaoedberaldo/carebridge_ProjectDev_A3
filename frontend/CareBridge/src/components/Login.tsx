import React from 'react';
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

  const onSubmit = (data: LoginFormValues) => {
    console.log(data);
    navigate('/dashboard');
  };

  return (
    <div className='signup-body'>
      <div className="signup-container">
        <h1>Welcome Back</h1>
        <p>Don't have an account? <a href="/signup" className="signup-link">Sign up</a></p>
        
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
