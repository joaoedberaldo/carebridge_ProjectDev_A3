import React, { useState, useEffect } from "react";
import "../../styles/FindDoctor.css";
import { toast, ToastContainer } from "react-toastify";
import "react-toastify/dist/ReactToastify.css";


// Define the interface for appointment form values
interface AppointmentFormValues {
  appointmentDate: string;
  notes: string;
  doctorId: number;
  patientId: number;
}

// Define the interface for a doctor object
interface Doctor {
  Id: number;
  firstName: string;
  lastName: string;
  specialization?: string;
  licenseNumber?: string;
}

// Define the props that the component expects
interface FindDoctorProps {
  user: { Id: number; role: number }; // Expect a user object with Id and role
  token: string;
}

const FindDoctor: React.FC<FindDoctorProps> = ({ user, token }) => {
  const [doctors, setDoctors] = useState<Doctor[]>([]); // List of doctors
  const [selectedDoctor, setSelectedDoctor] = useState<Doctor | null>(null); // Selected doctor for booking
  const [showModal, setShowModal] = useState<boolean>(false); // Controls modal visibility
  const [formValues, setFormValues] = useState<AppointmentFormValues>({
    appointmentDate: "",
    notes: "",
    doctorId: 0,
    patientId: 0, // Default to logged-in user ID
  });
  const [message, setMessage] = useState<string>(""); // Feedback message

  // Fetch doctors from backend when component loads
  useEffect(() => {
    const fetchDoctors = async () => {
      try {
        const response = await fetch("http://localhost:5156/api/doctors");
        if (response.ok) {
          const data = await response.json();
          setDoctors(data);
        } else {
          console.error("Failed to fetch doctors");
        }
      } catch (error) {
        console.error("Error fetching doctors", error);
      }
    };
    fetchDoctors();
  }, []);

  const handleBookClick = (doctor: Doctor) => {
    console.log("User data:", user); // Debugging user object
    console.log("Doctor data:", doctor); // Debugging doctor object
  
    setSelectedDoctor(doctor); // Sets doctor but does NOT update immediately!
  
    // Directly use `doctor` and `user` instead of relying on state updates
    setFormValues((prev) => ({ ...prev, doctorId: Number(doctor.id), patientId: Number(user.id) }));

    console.log("FormValues:", formValues);
    setShowModal(true); // Show modal when booking is clicked
  };

  // Handle input changes
  const handleChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => {
    setFormValues({ ...formValues, [e.target.name]: e.target.value });
  };

  // Handle form submission
  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
  
    if (!formValues.appointmentDate) {
      setMessage("Please select an appointment date.");
      return;
    }

    try {
      const response = await fetch("http://localhost:5156/api/appointments", {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
          Authorization: `Bearer ${token}`,
        },
        body: JSON.stringify(formValues),
      });
  
      // Check if the response is OK (200-299 range)
      if (!response.ok) {
        const errorText = await response.text(); // Get raw error message
        //  setMessage(`Failed to book appointment: ${errorText || "Unknown error"}`);
        throw new Error(`Failed to book appointment: ${errorText || "Unknown error"}`);
        // return;
      }
  
      // Handle cases where the response is empty (e.g., 204 No Content)
      let data;
      try {
        data = await response.json(); // Attempt to parse JSON
      } catch (err) {
        data = { Message: "Appointment booked successfully!" }; // Default message
      }
  
      // setMessage(data.Message || "Appointment booked successfully!");
      toast.success("Appointment booked successfully!");
      setShowModal(false); // Close modal on success
      setFormValues({ appointmentDate: "", notes: "", doctorId: 0, patientId: user.Id });
    } catch (error) {
      console.error("Error booking appointment", error);
      // setMessage("An error occurred. Please try again.");
      toast.error(error.message || "Something went wrong!");
    }
  };

  return (
    <div className="dashboard-content">
      <h2>Find a Doctor</h2>
      
      <ToastContainer />

      <div className="doctor-list">
        {doctors.map((doctor) => (
          <div key={doctor.Id} className="doctor-card">
            <h3>{doctor.firstName} {doctor.lastName}</h3>
            <p>Specialization: {doctor.specialization || "N/A"}</p>
            <p>License: {doctor.licenseNumber || "N/A"}</p>
            <button className="book-button" onClick={() => handleBookClick(doctor)}>
              Book Appointment
            </button>
          </div>
        ))}
      </div>

      {/* MODAL - Appointment Form */}
      {showModal && selectedDoctor && (
        <div className="modal-overlay" onClick={() => setShowModal(false)}>
          <div className="modal-content" onClick={(e) => e.stopPropagation()}>
            <h3>Book Appointment with {selectedDoctor.firstName} {selectedDoctor.lastName}</h3>
            <form onSubmit={handleSubmit}>
              <label>Appointment Date & Time:</label>
              <input
                type="datetime-local"
                name="appointmentDate"
                value={formValues.appointmentDate}
                onChange={handleChange}
                required
              />

              <label>Notes:</label>
              <textarea
                name="notes"
                value={formValues.notes}
                onChange={handleChange}
                placeholder="Enter any details for the doctor..."
              ></textarea>

              <button type="submit">Confirm Booking</button>
              <button type="button" onClick={() => setShowModal(false)}>Cancel</button>
            </form>
          </div>
        </div>
      )}
    </div>
  );
};

export default FindDoctor;
