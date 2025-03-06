import React, { useState, useEffect } from "react";
import "../../styles/Appointments.css";
import { toast, ToastContainer } from "react-toastify";
import "react-toastify/dist/ReactToastify.css";
import AppointmentDetailsModal from "./AppointmentDetailsModal"; // Reusing the existing modal component

// Define interfaces for type safety
interface AppointmentDto {
  id: number;
  appointmentDate: string;
  notes: string;
  patient?: {
    id: number;
    firstName: string;
    lastName: string;
    email: string;
  };
}

// Define props for the component
interface AppointmentsProps {
  user: {
    id: number;
    role: number;
    firstName?: string;
    lastName?: string;
  };
  token: string;
}

const Appointments: React.FC<AppointmentsProps> = ({ user, token }) => {
  // State to store appointments
  const [appointments, setAppointments] = useState<AppointmentDto[]>([]);
  // State to track loading status
  const [loading, setLoading] = useState<boolean>(true);
  // State to track errors
  const [error, setError] = useState<string | null>(null);
  // State to track which appointment is being viewed in the modal
  const [selectedAppointmentId, setSelectedAppointmentId] = useState<number | null>(null);
  // Use useEffect to fetch appointments when component mounts
  useEffect(() => {
    // Only fetch if we have a valid user ID and token
    if (user?.id && token) {
      console.log("Initializing appointment fetch for doctor:", user.id);
      fetchAppointments();
    } else {
      console.log("Missing user ID or token:", { userId: user?.id, hasToken: !!token });
      setLoading(false);
    }
  }, [user, token]); // Re-run if user or token changes

  // Function to fetch appointments from the backend
  const fetchAppointments = async () => {
    setLoading(true); // Start loading
    setError(null); // Reset any previous errors
    
    try {
      // Make API call to get appointments for the current doctor
      const url = `http://localhost:5156/api/doctors/appointments`;
      console.log("Fetching appointments from:", url);
      
      const response = await fetch(url, {
        method: "GET",
        headers: {
          "Authorization": `Bearer ${token}`,
          "Content-Type": "application/json"
        }
      });

      // Log the response for debugging
      console.log("Response status:", response.status);

      // Check if response is successful
      if (!response.ok) {
        // If not successful, handle the error
        const errorText = await response.text();
        console.error("Error response:", response.status, errorText);
        throw new Error(errorText || `Failed to fetch appointments (Status: ${response.status})`);
      }

      // Parse the response data
      const data = await response.json();
      console.log("Appointments data received:", data);
      
      // Update state with the fetched appointments
      setAppointments(data);
      
    } catch (err) {
      // Handle any errors that occurred during the fetch
      console.error("Error fetching appointments:", err);
      setError(`Failed to load appointments: ${err.message}`);
      toast.error("Failed to load appointments");
    } finally {
      // Always set loading to false when done, whether successful or not
      setLoading(false);
    }
  };

  // Function to handle viewing appointment details
  const handleViewAppointment = (appointmentId: number) => {
    console.log(`Viewing appointment with ID: ${appointmentId}`);
    setSelectedAppointmentId(appointmentId); // This will trigger the modal to open
  };

  // Function to handle modal close
  const handleCloseModal = () => {
    setSelectedAppointmentId(null);
  };

  // Function to handle appointment updates (refresh the list)
  const handleAppointmentUpdated = () => {
    fetchAppointments();
  };

  // Function to format date to a more readable format
  const formatDate = (dateString: string): string => {
    const options: Intl.DateTimeFormatOptions = { 
      year: 'numeric', 
      month: 'long', 
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    };
    return new Date(dateString).toLocaleDateString(undefined, options);
  };

  // Determine if an appointment is upcoming or past for display purposes
  const isUpcoming = (appointmentDate: string) => {
    return new Date(appointmentDate) > new Date();
  };

  return (
    <div className="dashboard-content">
      <h2>My Appointments</h2>
      <ToastContainer /> {/* Toast container for notifications */}
      

      
      {/* Show loading indicator when fetching data */}
      {loading && (
        <div className="loading-container">
          <div className="loading-spinner">Loading appointments...</div>
          <button 
            onClick={fetchAppointments}
            className="retry-button"
          >
            Retry
          </button>
        </div>
      )}
      
      {/* Show error message if one exists */}
      {error && (
        <div className="error-message">
          {error}
          <button 
            onClick={fetchAppointments}
            className="retry-button"
          >
            Try Again
          </button>
        </div>
      )}
      
      {/* Show message if no appointments are found */}
      {!loading && !error && appointments.length === 0 && (
        <div className="no-appointments-message">
          You don't have any appointments scheduled.
        </div>
      )}
      
      {/* Display appointments in a grid layout */}
      <div className="appointments-grid">
        {appointments.map((appointment) => (
          <div key={appointment.id} className="appointment-card">
            <div className="appointment-header">
              <h3>Appointment #{appointment.id}</h3>
              <span className={isUpcoming(appointment.appointmentDate) ? "status-upcoming" : "status-past"}>
                {isUpcoming(appointment.appointmentDate) ? "Upcoming" : "Past"}
              </span>
            </div>
            <div className="appointment-details">
              <p><strong>Date & Time:</strong> {formatDate(appointment.appointmentDate)}</p>
              {appointment.patient && (
                <p><strong>Patient:</strong> {appointment.patient.firstName} {appointment.patient.lastName}</p>
              )}
              <p><strong>Notes:</strong> {appointment.notes || "No notes provided"}</p>
            </div>
            <div className="appointment-actions">
              <button 
                onClick={() => handleViewAppointment(appointment.id)}
                className="view-details-button"
              >
                View Details
              </button>
            </div>
          </div>
        ))}
      </div>
      
      {/* Appointment Details Modal */}
      {selectedAppointmentId && (
        <AppointmentDetailsModal
          appointmentId={selectedAppointmentId}
          token={token}
          userRole={user.role} 
          userId={user.id}
          onClose={handleCloseModal}
          onAppointmentUpdated={handleAppointmentUpdated} 
        />
      )}
    </div>
  );
};

export default Appointments;