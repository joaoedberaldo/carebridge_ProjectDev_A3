/* eslint-disable no-unused-vars */
/* eslint-disable react/prop-types */
import React, { useState } from "react";
import { FaHome, FaUserMd, FaFileMedical, FaCalendarAlt, FaQuestionCircle, FaBell, FaUser } from "react-icons/fa";
import { Menu, MenuItem } from "@mui/material";
import Logo from "/logo.jpg";
import "./App.css";

const Sidebar = () => {

  return (
     <div className={`sidebar`}>
      <div className="logo-container">
        <img src={Logo} alt="Logo" className="logo" />
      </div>
      <NavItem icon={<FaHome />} text="HomePage" />
      <NavItem icon={<FaUserMd />} text="Find a doctor" />
      <NavItem icon={<FaFileMedical />} text="Medical reports" />
      <NavItem icon={<FaCalendarAlt />} text="Schedule" />
      <NavItem icon={<FaQuestionCircle />} text="Help" />
    </div>
  );
};

const NavItem = ({ icon, text }) => {
  return (
    <div className="nav-item">
      {icon}
      {text}
    </div>
  );
};

const TopBar = () => {
  const [anchorEl, setAnchorEl] = useState(null);
  const open = Boolean(anchorEl);

  const handleClick = (event) => {
    setAnchorEl(event.currentTarget);
  };

  const handleClose = () => {
    setAnchorEl(null);
  };

  return (
    <div className="topbar">
      <h2 className="welcome-text">Welcome User101!</h2>
      <div className="topbar-icons">
        <div className="notification">
          <FaBell className="icon" />
          <span className="notification-badge">6</span>
        </div>
        <div>
          <FaUser className="icon" onClick={handleClick} />
          <Menu anchorEl={anchorEl} open={open} onClose={handleClose}>
            <MenuItem onClick={handleClose}>Profile</MenuItem>
            <MenuItem onClick={handleClose}>Settings</MenuItem>
            <MenuItem onClick={handleClose}>Logout</MenuItem>
          </Menu>
        </div>
      </div>
    </div>
  );
};

const DashboardContent = () => {
  return (
    <div className="dashboard-content">
      <div className="card">Patient Information Here</div>
      <div className="card">Next Appointments Here</div>
      <div className="card">
        <p>Specialty</p>
        <p>Doctor - Status</p>
        <p>E.g. Cardiologist</p>
        <p>Dr. John Doe - Waiting results of heart exam</p>
      </div>
    </div>
  );
};

const Dashboard = () => {
  return (
    <div className="app-container">
      <Sidebar />
      <div className="dashboard">
        <TopBar />
        <DashboardContent />
      </div>
    </div>
  );
};

export default Dashboard;
