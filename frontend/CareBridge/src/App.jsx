
import React, { useState } from 'react';
import { createRoot } from 'react-dom/client';
import { BrowserRouter as Router, Route, Routes, Link, useNavigate } from 'react-router-dom';
import '../styles/index.css';
import HomePage from './components/HomePage'
import Dashboard from './components/Dashboard';
import Signup from './components/Signup';
import Login from './components/Login';
import EditProfile from './components/EditProfile';

function App () {
    return (
      <Router>
        <Routes>
          <Route path="/" element={<HomePage />} />
          <Route path="/dashboard" element={<Dashboard />} />
          <Route path="/signup" element={<Signup />} />
          <Route path="/login" element={<Login />} />
          <Route path="/editprofile" element={<EditProfile />} />
        </Routes>
      </Router>
    );
  };
  
export default App;
