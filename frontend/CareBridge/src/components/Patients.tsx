
import React, { useState, useEffect } from "react";
import "../../styles/Patients.css";
import { toast, ToastContainer } from "react-toastify";
import "react-toastify/dist/ReactToastify.css";
import HealthRecordsModal from "./HealthRecordsModal";

// Define interface for patient data
interface PatientDto {
  id: number;
  firstName: string;
  lastName: string;
  email: string;
  phoneNumber: string;
  dateOfBirth?: string;
}

// Define props for the component
interface PatientsProps {
  user: {
    id: number;
    role: number;
    firstName?: string;
    lastName?: string;
  };
  token: string;
}

const Patients: React.FC<PatientsProps> = ({ user, token }) => {
  // State to store patients data
  const [patients, setPatients] = useState<PatientDto[]>([]);
  // State to track loading status
  const [loading, setLoading] = useState<boolean>(true);
  // State to track errors
  const [error, setError] = useState<string | null>(null);
  // State to track selected patient for health records
  const [selectedPatient, setSelectedPatient] = useState<PatientDto | null>(null);

  // Use useEffect to fetch patients when component mounts
  useEffect(() => {
    // Only fetch if we have a valid user ID and token
    if (user?.id && token) {
      console.log("Initializing patients fetch for doctor:", user.id);
      fetchPatients();
    } else {
      console.log("Missing user ID or token:", { userId: user?.id, hasToken: !!token });
      setLoading(false);
    }
  }, [user, token]); // Re-run if user or token changes

  // Function to fetch patients from the backend
  const fetchPatients = async () => {
    setLoading(true); // Start loading
    setError(null); // Reset any previous errors
    
    try {
      // Make API call to get patients for the current doctor
      const url = `http://localhost:5156/api/doctors/patients`;
      console.log("Fetching patients from:", url);
      
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
        throw new Error(errorText || `Failed to fetch patients (Status: ${response.status})`);
      }

      // Parse the response data
      const data = await response.json();
      console.log("Patients data received:", data);
      
      // Update state with the fetched patients
      setPatients(data);
      
    } catch (err) {
      // Handle any errors that occurred during the fetch
      console.error("Error fetching patients:", err);
      setError(`Failed to load patients: ${err.message}`);
      toast.error("Failed to load patients");
    } finally {
      // Always set loading to false when done, whether successful or not
      setLoading(false);
    }
  };

  // Function to handle "View Health Records" button click
  const handleViewHealthRecords = (patient: PatientDto) => {
    console.log(`Viewing health records for patient with ID: ${patient.id}`);
    console.log(patient)
    setSelectedPatient(patient);
  };

  return (
    <div className="dashboard-content">
      <h2>My Patients</h2>
      <ToastContainer /> {/* Toast container for notifications */}
      
      {/* Show loading indicator when fetching data */}
      {loading && (
        <div className="loading-container">
          <div className="loading-spinner">Loading patients...</div>
          <button 
            onClick={fetchPatients}
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
            onClick={fetchPatients}
            className="retry-button"
          >
            Try Again
          </button>
        </div>
      )}
      
      {/* Show message if no patients are found */}
      {!loading && !error && patients.length === 0 && (
        <div className="no-patients-message">
          You don't have any patients yet.
        </div>
      )}
      
      {/* Display patients in a grid layout */}
      <div className="patients-grid">
        {patients.map((patient) => (
          <div key={patient.id} className="patient-card">
            <div className="patient-header">
              <h3>{patient.firstName} {patient.lastName}</h3>
            </div>
            <div className="patient-details">
              <p><strong>Email:</strong> {patient.email}</p>
              <p><strong>Phone:</strong> {patient.phoneNumber || "Not provided"}</p>
            </div>
            <div className="patient-actions">
              <button 
                onClick={() => handleViewHealthRecords(patient)}
                className="view-record-button"
              >
                Health Records
              </button>
            </div>
          </div>
        ))}
      </div>

      {/* Health Records Modal */}
      {selectedPatient && (
        <HealthRecordsModal
          patient={selectedPatient}
          token={token}
          userRole={user.role}
          onClose={() => setSelectedPatient(null)}
        />
      )}
    </div>
  );
};

export default Patients;