import React, { useState, useEffect } from "react";
import { toast } from "react-toastify";
import "../../styles/PrescriptionsModal.css";
import NewPrescriptionModal from "./NewPrescriptionModal";
import NewMedicationModal from "./NewMedicationModal";

interface Medication {
  id: number;
  name: string;
  dosage: string;
  frequency: string;
  notes: string;
}

interface Prescription {
  id: number;
  date: string;
  description: string;
  status: string;
  medications: Medication[];
  doctor: {
    id: number;
    firstName: string;
    lastName: string;
  };
  patient: {
    id: number;
    firstName: string;
    lastName: string;
  };
}

interface PrescriptionsModalProps {
  appointmentId: number;
  patientId: number;
  doctorId: number;
  token: string;
  userRole: number; // 0 = Doctor, 1 = Patient, 2 = Assistant
  onClose: () => void;
}

const PrescriptionsModal: React.FC<PrescriptionsModalProps> = ({
  appointmentId,
  patientId,
  doctorId,
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
  // State for selected prescription (for viewing details)
  const [selectedPrescription, setSelectedPrescription] = useState<Prescription | null>(null);
  // State for showing the new prescription modal
  const [showNewPrescriptionModal, setShowNewPrescriptionModal] = useState<boolean>(false);
  // State for showing the new medication modal
  const [showNewMedicationModal, setShowNewMedicationModal] = useState<boolean>(false);

  // Fetch prescriptions when component mounts
  useEffect(() => {
    fetchPrescriptions();
  }, [appointmentId]);

  // Function to fetch prescriptions
  const fetchPrescriptions = async () => {
    setLoading(true);
    setError(null);

    try {
      const response = await fetch(`http://localhost:5156/api/prescriptions/appointment/${appointmentId}`, {
        method: "GET",
        headers: {
          "Authorization": `Bearer ${token}`,
          "Content-Type": "application/json"
        }
      });

      if (!response.ok) {
        const errorText = await response.text();
        throw new Error(errorText || `Failed to fetch prescriptions (Status: ${response.status})`);
      }

      const data = await response.json();
      setPrescriptions(data);
    } catch (err) {
      console.error("Error fetching prescriptions:", err);
      setError(`Failed to load prescriptions: ${err.message}`);
    } finally {
      setLoading(false);
    }
  };

  // Function to handle prescription creation
  const handlePrescriptionCreated = () => {
    fetchPrescriptions();
    setShowNewPrescriptionModal(false);
  };

  // Function to handle medication addition
  const handleMedicationAdded = () => {
    fetchPrescriptions();
    setShowNewMedicationModal(false);
  };

  // Function to format date
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

// Function to delete a prescription
const handleDeletePrescription = async (prescriptionId: number) => {
    if (!window.confirm("Are you sure you want to delete this prescription?")) {
      return;
    }

    try {
      const response = await fetch(`http://localhost:5156/api/prescriptions/${prescriptionId}`, {
        method: "DELETE",
        headers: {
          "Authorization": `Bearer ${token}`,
          "Content-Type": "application/json"
        }
      });

      if (!response.ok) {
        const errorText = await response.text();
        throw new Error(errorText || `Failed to delete prescription (Status: ${response.status})`);
      }

      // Handle potential empty or non-JSON response
      let responseData;
      try {
        const text = await response.text();
        if (text && text.trim()) {
          responseData = JSON.parse(text);
        }
      } catch (e) {
        console.log("Response couldn't be parsed as JSON or was empty");
      }

      toast.success("Prescription deleted successfully");
      fetchPrescriptions();
    } catch (err) {
      console.error("Error deleting prescription:", err);
      toast.error(`Failed to delete prescription: ${err.message}`);
    }
  };

  // Function to delete a medication
  const handleDeleteMedication = async (medicationId: number) => {
    if (!window.confirm("Are you sure you want to delete this medication?")) {
      return;
    }

    try {
      const response = await fetch(`http://localhost:5156/api/prescriptions/medications/${medicationId}`, {
        method: "DELETE",
        headers: {
          "Authorization": `Bearer ${token}`,
          "Content-Type": "application/json"
        }
      });

      if (!response.ok) {
        const errorText = await response.text();
        throw new Error(errorText || `Failed to delete medication (Status: ${response.status})`);
      }

      // Handle potential empty or non-JSON response
      let responseData;
      try {
        const text = await response.text();
        if (text && text.trim()) {
          responseData = JSON.parse(text);
        }
      } catch (e) {
        console.log("Response couldn't be parsed as JSON or was empty");
      }

      toast.success("Medication deleted successfully");
      fetchPrescriptions();
    } catch (err) {
      console.error("Error deleting medication:", err);
      toast.error(`Failed to delete medication: ${err.message}`);
    }
  };

  // Check if user is a doctor
  const isDoctor = userRole === 0;

  return (
    <div className="modal-backdrop">
      <div className="prescriptions-modal-content">
        {/* Modal Header */}
        <div className="modal-header">
          <h2>Prescriptions</h2>
          <button className="close-button" onClick={onClose}>×</button>
        </div>

        {/* Modal Body */}
        <div className="modal-body">
          {/* Doctor-only actions */}
          {isDoctor && (
            <div className="prescriptions-actions">
              <button 
                className="new-prescription-button"
                onClick={() => setShowNewPrescriptionModal(true)}
              >
                New Prescription
              </button>
            </div>
          )}

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
              <button onClick={fetchPrescriptions}>
                Try Again
              </button>
            </div>
          )}

          {/* No Prescriptions State */}
          {!loading && !error && prescriptions.length === 0 && (
            <div className="no-prescriptions">
              <p>There are no prescriptions for this appointment yet.</p>
            </div>
          )}

          {/* Prescriptions List */}
          {!loading && !error && prescriptions.length > 0 && (
            <div className="prescriptions-list">
              {prescriptions.map((prescription) => (
                <div key={prescription.id} className="prescription-card">
                  <div className="prescription-header">
                    <h3>Prescription #{prescription.id}</h3>
                  </div>

                  <div className="prescription-details">
                    <p><strong>Date:</strong> {formatDate(prescription.date)}</p>
                    <p><strong>Description:</strong> {prescription.description}</p>
                  </div>

                  <div className="prescription-medications">
                    <h4>Medications</h4>
                    {prescription.medications.length === 0 ? (
                      <p className="no-medications">No medications added yet.</p>
                    ) : (
                      <ul className="medications-list">
                        {prescription.medications.map((medication) => (
                          <li key={medication.id} className="medication-item">
                            <div className="medication-details">
                              <p><strong>{medication.name}</strong> ({medication.dosage})</p>
                              <p>Frequency: {medication.frequency}</p>
                              {medication.notes && <p>Notes: {medication.notes}</p>}
                            </div>
                            {isDoctor && (
                              <button 
                                className="delete-medication-button"
                                onClick={() => handleDeleteMedication(medication.id)}
                              >
                                Remove
                              </button>
                            )}
                          </li>
                        ))}
                      </ul>
                    )}
                  </div>

                  {isDoctor && (
                    <div className="prescription-actions">
                      <button 
                        className="add-medication-button"
                        onClick={() => {
                          setSelectedPrescription(prescription);
                          setShowNewMedicationModal(true);
                        }}
                      >
                        Add Medication
                      </button>
                      <button 
                        className="delete-prescription-button"
                        onClick={() => handleDeletePrescription(prescription.id)}
                      >
                        Delete Prescription
                      </button>
                    </div>
                  )}
                </div>
              ))}
            </div>
          )}
        </div>
      </div>

      {/* New Prescription Modal */}
      {showNewPrescriptionModal && (
        <NewPrescriptionModal
          appointmentId={appointmentId}
          patientId={patientId}
          token={token}
          onClose={() => setShowNewPrescriptionModal(false)}
          onPrescriptionCreated={handlePrescriptionCreated}
        />
      )}

      {/* New Medication Modal */}
      {showNewMedicationModal && selectedPrescription && (
        <NewMedicationModal
          prescriptionId={selectedPrescription.id}
          token={token}
          onClose={() => setShowNewMedicationModal(false)}
          onMedicationAdded={handleMedicationAdded}
        />
      )}
    </div>
  );
};

export default PrescriptionsModal;