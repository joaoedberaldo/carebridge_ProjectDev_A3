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
  id: number;
  firstName: string;
  lastName: string;
  specialization?: string;
  licenseNumber?: string;
}

// Define the props that the component expects
interface FindDoctorProps {
  user: { 
    id: number; 
    role: number;
  };
  token: string;
}

const FindDoctor: React.FC<FindDoctorProps> = ({ user, token }) => {
  const [doctors, setDoctors] = useState<Doctor[]>([]);
  const [selectedDoctor, setSelectedDoctor] = useState<Doctor | null>(null);
  const [showModal, setShowModal] = useState<boolean>(false);
  const [formValues, setFormValues] = useState<AppointmentFormValues>({
    appointmentDate: "",
    notes: "",
    doctorId: 0,
    patientId: user?.id || 0,
  });
  
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
          toast.error("Failed to load doctors list");
        }
      } catch (error) {
        console.error("Error fetching doctors", error);
        toast.error("Could not connect to the server");
      }
    };
    fetchDoctors();
  }, []);

  const handleBookClick = (doctor: Doctor) => {
    // Set the selected doctor
    setSelectedDoctor(doctor);
    
    // Update form values with doctor ID and patient ID
    setFormValues({
      ...formValues,
      doctorId: doctor.id,
      patientId: user.id
    });
    
    // Show the appointment booking modal
    setShowModal(true);
  };

  // Handle input changes in the form
  const handleChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => {
    setFormValues({
      ...formValues,
      [e.target.name]: e.target.value
    });
  };

  // Handle form submission
  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
  
    if (!formValues.appointmentDate) {
      toast.error("Please select an appointment date");
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
  
      if (!response.ok) {
        const errorText = await response.text();
        throw new Error(`Failed to book appointment: ${errorText || "Unknown error"}`);
      }
  
      toast.success("Appointment booked successfully!");
      setShowModal(false);
      setFormValues({
        appointmentDate: "",
        notes: "",
        doctorId: 0,
        patientId: user.id
      });
    } catch (error) {
      console.error("Error booking appointment", error);
      toast.error(error.message || "Something went wrong!");
    }
  };

  return (
    <div className="dashboard-content">
      <h2>Find a Doctor</h2>
      
      <ToastContainer />

      {/* Doctor List Grid */}
      <div className="doctor-list">
        {doctors.map((doctor) => (
          <div key={doctor.id} className="doctor-card">
            <h3>Dr. {doctor.firstName} {doctor.lastName}</h3>
            <p><strong>Specialization:</strong> {doctor.specialization || "General Practice"}</p>
            <p><strong>License:</strong> {doctor.licenseNumber || "N/A"}</p>
            <button 
              className="book-button" 
              onClick={() => handleBookClick(doctor)}
            >
              Book Appointment
            </button>
          </div>
        ))}
      </div>

      {/* Appointment Booking Modal */}
      {showModal && selectedDoctor && (
        <div className="modal-overlay" onClick={() => setShowModal(false)}>
          <div className="modal-content" onClick={(e) => e.stopPropagation()}>
            <h3>Book Appointment with Dr. {selectedDoctor.firstName} {selectedDoctor.lastName}</h3>
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
                className="notes"
                name="notes"
                value={formValues.notes}
                onChange={handleChange}
                placeholder="Enter any details for the doctor..."
              ></textarea>

              <div style={{ display: 'flex', gap: '1rem', justifyContent: 'center' }}>
                <button type="submit">Confirm Booking</button>
                <button type="button" onClick={() => setShowModal(false)}>Cancel</button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
};

export default FindDoctor;