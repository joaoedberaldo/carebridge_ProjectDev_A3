
import React, { useState } from 'react';
import { createRoot } from 'react-dom/client';
import { BrowserRouter as Router, Route, Routes, Link, useNavigate } from 'react-router-dom';
import '../../styles/index.css';


const sections = {
  home: {
    title: "Welcome to CareBridge",
    description: "Connecting you with the best healthcare services.",
    image: "/Home.jpg"
  },
  about: {
    title: "About CareBridge",
    description: "CareBridge is designed to make healthcare access easier and more efficient.",
    image: "/About.jpg"
  },
  contact: {
    title: "Contact Us",
    description: "Get in touch with us for any inquiries or support.",
    image: "/Contact.jpg"
  },
  features: {
    title: "Our Features",
    description: "Explore the features that make CareBridge the best healthcare platform.",
    image: "/Feature.jpg"
  }
};

function MainPage () {
  const [activeSection, setActiveSection] = useState(sections.home);
  const navigate = useNavigate();

  return (
    <div className="main-container">
      <nav className="navbar">
        <ul>
          <li onClick={() => setActiveSection(sections.home)}>Home</li>
          <li onClick={() => setActiveSection(sections.about)}>About</li>
          <li onClick={() => navigate('/login')}>Dashboard</li>
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
            <button className="button" onClick={() => navigate('/signup')}>Register</button>
            <button className="button" onClick={() => navigate('/login')}>Login</button>
          </div>
      </div>
    </div>
  );
};

export default MainPage;