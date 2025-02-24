/* eslint-disable no-unused-vars */
/* eslint-disable react/prop-types */
import React, { useState, useEffect } from "react";
import { FaHome, FaUserMd, FaFileMedical, FaCalendarAlt, FaQuestionCircle, FaUser } from "react-icons/fa";
import { Menu, MenuItem } from "@mui/material";
import { useNavigate } from 'react-router-dom';
import Logo from "/logo.jpg";
import DoctorImage from "../assets/Doctor.png";  // Import doctor image
import PatientImage from "../assets/Patient.png"; // Import patient image
import "../../styles/Dashboard.css";

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

const TopBar = ({user}) => {
  const navigate = useNavigate();
  const [anchorEl, setAnchorEl] = useState(null);
  const open = Boolean(anchorEl);

  const handleClick = (event) => {
    setAnchorEl(event.currentTarget);
  };

  const handleClose = () => {
    setAnchorEl(null);
  };

  const handleProfileClick = () => {
    navigate("/editprofile");
  };

  const handleLogout = () => {
    localStorage.removeItem("token"); // Clear stored token
    navigate("/"); // Redirect to main page
  };

  // Choose image based on role (0 = Doctor, 1 = Patient)
  const userImage = user?.role === 0 ? DoctorImage : PatientImage;

  return (
    <div className="topbar">
      <h2 className="welcome-text">
        Welcome {user?.firstName || "User"}! &nbsp;
        <img src={userImage} alt="User Role" className="user-role-image" />
      </h2>
      <div className="topbar-icons">
        <div>
          <FaUser className="icon" onClick={handleClick} />
          <Menu anchorEl={anchorEl} open={open} onClose={handleClose}>
            <MenuItem onClick={handleProfileClick}>Profile</MenuItem>
            <MenuItem onClick={handleLogout}>Logout</MenuItem>
          </Menu>
        </div>
      </div>
    </div>
  );
};

const DashboardContent = ({user}) => {
  // Map role number to text
  const roleMapping = {
    0: "Doctor",
    1: "Patient",
    2: "Assistant",
  };

  // Format Date of Birth (if available)
  const formattedDOB = user?.dateOfBirth 
    ? new Date(user.dateOfBirth).toISOString().split("T")[0] 
    : "N/A";

  return (
    <div className="dashboard-content">
      <div className="card">Personal Information 
        <p>&nbsp;&nbsp;&nbsp;&nbsp;First Name: {user?.firstName}</p> 
        <p>&nbsp;&nbsp;&nbsp;&nbsp;Last Name: {user?.lastName}</p> 
        <p>&nbsp;&nbsp;&nbsp;&nbsp;Email: {user?.email}</p>
        <p>&nbsp;&nbsp;&nbsp;&nbsp;Role: {roleMapping[user?.role] || "Unknown"}</p>
        <p>&nbsp;&nbsp;&nbsp;&nbsp;Date of Birth: {formattedDOB}</p>
        {/* <p>&nbsp;&nbsp;&nbsp;&nbsp;PhoneNumber: {user?.phonenumber}</p> */}
      </div>
      <div className="card">Notifications Here </div>
      <div className="card">Next Appointments Here</div>
      <div className="card">
        <p>Dr. John Doe - Cardiologist - Waiting results of heart exam</p>
      </div>      
    </div>
  );
};

const Dashboard = () => {
  const [user, setUser] = useState(null);
  const navigate = useNavigate();

  useEffect(() => {
    const fetchUserData = async () => {
      const token = localStorage.getItem("token");

      if (!token) {
        navigate("/");
        return;
      }

      try {
        const response = await fetch(`http://localhost:5156/api/auth/me`, {
          method: "GET",
          headers: {
            "Authorization": `Bearer ${token}`,
            "Content-Type": "application/json",
          },
        });

        if (response.ok) {
          const userData = await response.json();
          console.log(userData);
          setUser(userData);
        } else {
          console.error("Failed to fetch user data");
          navigate("/");
        }
      } catch (error) {
        console.error("Error fetching user data", error);
        navigate("/");
      }
    };

    fetchUserData();
  }, [navigate]);
  
  return (
    <div className="app-container">
      <Sidebar />
      <div className="dashboard">
        <TopBar user={user}/>
        <DashboardContent user={user}/>
      </div>
    </div>
  );
};

export default Dashboard;
