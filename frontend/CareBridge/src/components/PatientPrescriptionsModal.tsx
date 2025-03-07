import React, { useState, useEffect } from "react";
import { toast, ToastContainer } from "react-toastify";
import "react-toastify/dist/ReactToastify.css";
import "../../styles/HealthRecordsModal.css";

// Define interfaces for the data structures
interface Medication {
  id: number;
  name: string;
  dosage: string;
  frequency: string;
  notes?: string;
}

interface Prescription {
  id: number;
  doctorId: number;
  doctorName: string;
  date: string;
  description: string;
  status: string; // or number if using enum
  appointmentId?: number;
  medications: Medication[];
}

// Define props for the component
interface PatientPrescriptionsModalProps {
  patientId: number;
  patientName: string;
  token: string;
  userRole: number; // 0 = Doctor, 1 = Patient, 2 = Assistant
  onClose: () => void;
}

const PatientPrescriptionsModal: React.FC<PatientPrescriptionsModalProps> = ({
  patientId,
  patientName,
  token,
  userRole,
  onClose,
}) => {
  // State for prescriptions
  const [prescriptions, setPrescriptions] = useState<Prescription[]>([]);
  // State for loading status
  const [loading, setLoading] = useState<boolean>(true);
  // State for error messages
  const [error, setError] = useState<string | null>(null);
  // State for expanded prescription (to show medications)
  const [expandedPrescription, setExpandedPrescription] = useState<number | null>(null);

  // Fetch prescriptions when component mounts
  useEffect(() => {
    fetchPrescriptions();
  }, [patientId]);

  // Function to fetch prescriptions by patient ID
  const fetchPrescriptions = async () => {
    setLoading(true);
    setError(null);

    try {
      // Make API call to get prescriptions for the patient
      const response = await fetch(`http://localhost:5156/api/prescriptions/patient/${patientId}`, {
        method: "GET",
        headers: {
          "Authorization": `Bearer ${token}`,
          "Content-Type": "application/json"
        }
      });

      // Check if response is successful
      if (!response.ok) {
        const errorText = await response.text();
        throw new Error(errorText || `Failed to fetch prescriptions (Status: ${response.status})`);
      }

      // Get response text and check for potential circular reference
      const responseText = await response.text();
      
      // Check if the response contains an error about circular reference
      if (responseText.includes('object cycle was detected') || responseText.includes('JsonException')) {
        throw new Error("The server encountered a data structure issue. Please contact your administrator.");
      }
      
      // Parse the JSON response
      let data;
      try {
        data = JSON.parse(responseText);
        setPrescriptions(Array.isArray(data) ? data : []);
      } catch (parseError) {
        console.error("JSON parse error:", parseError);
        throw new Error("Failed to parse prescription data. The response format is invalid.");
      }
    } catch (err) {
      console.error("Error fetching prescriptions:", err);
      setError(`${err.message}`);
    } finally {
      setLoading(false);
    }
  };

  // Function to format date
  const formatDate = (dateString: string): string => {
    const options: Intl.DateTimeFormatOptions = { 
      year: 'numeric', 
      month: 'long', 
      day: 'numeric'
    };
    return new Date(dateString).toLocaleDateString(undefined, options);
  };

  // Function to toggle expanded prescription
  const togglePrescription = (prescriptionId: number) => {
    if (expandedPrescription === prescriptionId) {
      // If already expanded, collapse it
      setExpandedPrescription(null);
    } else {
      // Expand the clicked prescription
      setExpandedPrescription(prescriptionId);
    }
  };

  // Function to map status code to human-readable string
  const getStatusText = (status: string | number): string => {
    // If status is already a string, just return it
    if (typeof status === 'string') {
      return status;
    }
    
    // Map status code to text
    switch (status) {
      case 0:
        return "Active";
      case 1:
        return "Completed";
      case 2:
        return "Cancelled";
      default:
        return "Unknown";
    }
  };

  // Function to get status class based on status
  const getStatusClass = (status: string | number): string => {
    const statusText = typeof status === 'string' ? status.toLowerCase() : getStatusText(status).toLowerCase();
    
    if (statusText === 'active') return 'status-active';
    if (statusText === 'completed') return 'status-completed';
    if (statusText === 'cancelled') return 'status-cancelled';
    
    return 'status-unknown';
  };

  return (
    <div className="modal-backdrop">
      <div className="patient-prescriptions-modal">
        {/* Modal Header */}
        <div className="modal-header">
          <h2>Prescriptions for {patientName}</h2>
          <button className="close-button" onClick={onClose}>×</button>
        </div>

        <div className="modal-body">
          <ToastContainer />
          
          {/* Loading State */}
          {loading && (
            <div className="modal-loading">
              <p>Loading prescriptions...</p>
            </div>
          )}

          {/* Error State */}
          {error && (
            <div className="modal-error">
              <p>{error}</p>
              <button onClick={fetchPrescriptions} className="retry-button">
                Try Again
              </button>
            </div>
          )}

          {/* No Prescriptions State */}
          {!loading && !error && prescriptions.length === 0 && (
            <div className="no-data-message">
              <p>No prescriptions found for this patient.</p>
            </div>
          )}

          {/* Prescriptions List */}
          {!loading && !error && prescriptions.length > 0 && (
            <div className="prescriptions-list">
              {prescriptions.map((prescription) => (
                <div key={prescription.id} className="prescription-card">
                  <div 
                    className="prescription-header"
                    onClick={() => togglePrescription(prescription.id)}
                  >
                    <div className="prescription-header-content">
                      <h3>Prescribed on {formatDate(prescription.date)}</h3>
                      <p>by Dr. {prescription.doctorName}</p>
                    </div>
                    <span className={`status-badge ${getStatusClass(prescription.status)}`}>
                      {getStatusText(prescription.status)}
                    </span>
                  </div>

                  <div className="prescription-summary" onClick={() => togglePrescription(prescription.id)}>
                    <p>{prescription.description}</p>
                    <p className="view-more">
                      {expandedPrescription === prescription.id ? "Hide medications" : "View medications"} 
                      <span className="arrow">{expandedPrescription === prescription.id ? "▲" : "▼"}</span>
                    </p>
                  </div>

                  {/* Expanded Medications List */}
                  {expandedPrescription === prescription.id && (
                    <div className="medications-container">
                      <h4>Medications</h4>
                      
                      {prescription.medications.length === 0 ? (
                        <p className="no-data-message">No medications were prescribed.</p>
                      ) : (
                        <ul className="medications-list">
                          {prescription.medications.map((medication) => (
                            <li key={medication.id} className="medication-item">
                              <div className="medication-details">
                                <h5>{medication.name}</h5>
                                <p><strong>Dosage:</strong> {medication.dosage}</p>
                                <p><strong>Frequency:</strong> {medication.frequency}</p>
                                {medication.notes && <p><strong>Notes:</strong> {medication.notes}</p>}
                              </div>
                            </li>
                          ))}
                        </ul>
                      )}
                    </div>
                  )}
                </div>
              ))}
            </div>
          )}
        </div>
      </div>
    </div>
  );
};

export default PatientPrescriptionsModal;