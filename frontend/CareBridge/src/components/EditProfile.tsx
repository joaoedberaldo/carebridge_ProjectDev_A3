import React, { useEffect, useState } from "react";
import { useForm } from "react-hook-form";
import { useNavigate } from "react-router-dom";
import "../../styles/signup.css"; // Reusing the signup styles

interface UserProfile {
  firstName: string;
  lastName: string;
  phoneNumber: string;
}

const EditProfile = () => {
  const { register, handleSubmit, setValue, formState: { errors } } = useForm<UserProfile>();
  const navigate = useNavigate();
  const [apiError, setApiError] = useState<string | null>(null);
  const [userId, setUserId] = useState<number | null>(null);

  useEffect(() => {
    const fetchUserData = async () => {
      const token = localStorage.getItem("token");

      if (!token) {
        navigate("/");
        return;
      }

      try {
        const response = await fetch("http://localhost:5156/api/auth/me", {
          method: "GET",
          headers: {
            Authorization: `Bearer ${token}`,
            "Content-Type": "application/json",
          },
        });

        if (response.ok) {
          const userData = await response.json();
          setUserId(userData.id);
          
          // Populate form fields with existing data
          setValue("firstName", userData.firstName);
          setValue("lastName", userData.lastName);
          setValue("phoneNumber", userData.phoneNumber || "");

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
  }, [navigate, setValue]);

  const onSubmit = async (data: UserProfile) => {
    setApiError(null);

    if (!userId) return;

    try {
      const token = localStorage.getItem("token");
      const response = await fetch(`http://localhost:5156/api/users/${userId}`, {
        method: "PUT",
        headers: {
          Authorization: `Bearer ${token}`,
          "Content-Type": "application/json",
        },
        body: JSON.stringify(data),
      });

      if (response.ok) {
        navigate("/dashboard");
      } else {
        const errorData = await response.json();
        setApiError(errorData.message || "Failed to update profile.");
      }
    } catch (error) {
      console.error("Error updating profile", error);
      setApiError("Could not connect to the server.");
    }
  };

  return (
    <div className="signup-body">
      <div className="signup-container">
        <h1>Edit Profile</h1>
        
        {apiError && <p className="error-message api-error">{apiError}</p>} 

        <form onSubmit={handleSubmit(onSubmit)}>
          <label className="input-label" htmlFor="firstName">First Name</label>
          <input
            className="text-field"
            id="firstName"
            type="text"
            {...register("firstName", { required: "First name is required" })}
          />
          {errors.firstName && <p className="error-message">{errors.firstName.message}</p>}

          <label className="input-label" htmlFor="lastName">Last Name</label>
          <input
            className="text-field"
            id="lastName"
            type="text"
            {...register("lastName", { required: "Last name is required" })}
          />
          {errors.lastName && <p className="error-message">{errors.lastName.message}</p>}

          <label className="input-label" htmlFor="phoneNumber">Phone Number</label>
          <input
            className="text-field"
            id="phoneNumber"
            type="text"
            {...register("phoneNumber", {
              required: "Phone number is required",
              pattern: { value: /^\d{3}-\d{3}-\d{4}$/, message: "Enter a valid phone number (e.g., 456-789-0123)" },
            })}
          />
          {errors.phoneNumber && <p className="error-message">{errors.phoneNumber.message}</p>}

          <div className="button-container">
            <button className="button" type="submit">Save Changes</button>
            <button className="button cancel-button" type="button" onClick={() => navigate("/dashboard")}>
              Cancel
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};

export default EditProfile;
