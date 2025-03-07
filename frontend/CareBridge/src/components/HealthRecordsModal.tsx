import React, { useState } from "react";
import "../../styles/HealthRecordsModal.css";
import PatientPrescriptionsModal from "./PatientPrescriptionsModal";

// Define interfaces for the patient data
interface PatientData {
  id: number;
  firstName: string;
  lastName: string;
  email: string;
  phoneNumber?: string;
  dateOfBirth?: string;
}

// Define props for the component
interface HealthRecordsModalProps {
  patient: PatientData;
  token: string;
  userRole: number; // 0 = Doctor, 1 = Patient, 2 = Assistant
  onClose: () => void;
}

const HealthRecordsModal: React.FC<HealthRecordsModalProps> = ({
  patient,
  token,
  userRole,
  onClose,
}) => {
  // State to control showing prescriptions modal
  const [showPrescriptionsModal, setShowPrescriptionsModal] = useState<boolean>(false);
  
  // State to control showing treatment modal
  const [showTreatmentModal, setShowTreatmentModal] = useState<boolean>(false);

  // Format date of birth if available
  const formatDateOfBirth = (dateString?: string): string => {
    if (!dateString) return "Not provided";
    
    const options: Intl.DateTimeFormatOptions = { 
      year: 'numeric', 
      month: 'long', 
      day: 'numeric'
    };
    return new Date(dateString).toLocaleDateString(undefined, options);
  };

  // Calculate age from date of birth
  const calculateAge = (dateOfBirth?: string): string => {
    if (!dateOfBirth) return "Unknown";
    
    const birthDate = new Date(dateOfBirth);
    const today = new Date();
    let age = today.getFullYear() - birthDate.getFullYear();
    const monthDifference = today.getMonth() - birthDate.getMonth();
    
    if (monthDifference < 0 || (monthDifference === 0 && today.getDate() < birthDate.getDate())) {
      age--;
    }
    
    return age.toString();
  };

  return (
    <div className="modal-backdrop">
      <div className="health-records-modal">
        {/* Modal Header */}
        <div className="modal-header">
          <h2>Patient Health Records</h2>
          <button className="close-button" onClick={onClose}>×</button>
        </div>

        {/* Modal Body */}
        <div className="modal-body">
          {/* Patient Information Section */}
          <div className="patient-info-section">
            <h3>Patient Information</h3>
            <div className="patient-details">
              <p><strong>Name:</strong> {patient.firstName} {patient.lastName}</p>
              <p><strong>Email:</strong> {patient.email}</p>
              <p><strong>Phone:</strong> {patient.phoneNumber || "Not provided"}</p>
              {/* NOT WORKING <p><strong>Date of Birth:</strong> {formatDateOfBirth(patient.dateOfBirth)}</p>
              <p><strong>Age:</strong> {calculateAge(patient.dateOfBirth)}</p> */}
            </div>
          </div>

          {/* Action Buttons */}
          <div className="health-record-actions">
            <button 
              className="prescriptions-view-button" 
              onClick={() => setShowPrescriptionsModal(true)}
            >
              View Prescriptions
            </button>
            
            <button 
              className="treatment-button" 
              onClick={() => setShowTreatmentModal(true)}
            >
              Treatment History
            </button>
          </div>
        </div>
      </div>

      {/* Patient Prescriptions Modal */}
      {showPrescriptionsModal && (
        <PatientPrescriptionsModal
          patientId={patient.id}
          patientName={`${patient.firstName} ${patient.lastName}`}
          token={token}
          userRole={userRole}
          onClose={() => setShowPrescriptionsModal(false)}
        />
      )}

      {/* Treatment Modal - Placeholder for now */}
      {showTreatmentModal && (
        <div className="modal-backdrop">
          <div className="modal-content">
            <div className="modal-header">
              <h2>Treatment History</h2>
              <button className="close-button" onClick={() => setShowTreatmentModal(false)}>×</button>
            </div>
            <div className="modal-body">
              <p>Treatment history functionality will be implemented in a future update.</p>
              <button 
                className="close-button-alt"
                onClick={() => setShowTreatmentModal(false)}
              >
                Close
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

export default HealthRecordsModal;