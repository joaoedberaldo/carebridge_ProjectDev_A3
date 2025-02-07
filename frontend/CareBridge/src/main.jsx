
import React, { useState } from 'react';
import { createRoot } from 'react-dom/client';
import { BrowserRouter as Router, Route, Routes, Link, useNavigate } from 'react-router-dom';
import './index.css';
import Dashboard from './Dashboard.jsx';

const sections = {
  home: {
    title: "Welcome to CareBridge",
    description: "Connecting you with the best healthcare services.",
    image: "/images/home.jpg"
  },
  about: {
    title: "About CareBridge",
    description: "CareBridge is designed to make healthcare access easier and more efficient.",
    image: "/images/about.jpg"
  },
  contact: {
    title: "Contact Us",
    description: "Get in touch with us for any inquiries or support.",
    image: "/images/contact.jpg"
  },
  features: {
    title: "Our Features",
    description: "Explore the features that make CareBridge the best healthcare platform.",
    image: "/images/features.jpg"
  }
};

const MainPage = () => {
  const [activeSection, setActiveSection] = useState(sections.home);
  const navigate = useNavigate();

  return (
    <div className="main-container">
      <nav className="navbar">
        <ul>
          <li onClick={() => setActiveSection(sections.home)}>Home</li>
          <li onClick={() => setActiveSection(sections.about)}>About</li>
          <li onClick={() => navigate('/dashboard')}>Dashboard</li>
          <li onClick={() => setActiveSection(sections.contact)}>Contact</li>
          <li onClick={() => setActiveSection(sections.features)}>Features</li>
        </ul>
      </nav>
      <div className="content">
        <div className="info-card">
          <img src={activeSection.image} alt={activeSection.title} className="info-image" />
          <div className="info-text">
            <h2>{activeSection.title}</h2>
            <p>{activeSection.description}</p>
          </div>
        </div>
        <div className="button-container">
            <button className="button" onClick={() => navigate('/register')}>Register</button>
            <button className="button" onClick={() => navigate('/login')}>Login</button>
          </div>
      </div>
    </div>
  );
};

const App = () => {
  return (
    <Router>
      <Routes>
        <Route path="/" element={<MainPage />} />
        <Route path="/dashboard" element={<Dashboard />} />
      </Routes>
    </Router>
  );
};

createRoot(document.getElementById('root')).render(
  <React.StrictMode>
    <App />
  </React.StrictMode>
);
