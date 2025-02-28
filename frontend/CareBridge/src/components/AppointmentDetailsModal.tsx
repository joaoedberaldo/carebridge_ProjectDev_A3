import React, { useState, useEffect } from "react";
import "../../styles/AppointmentModal.css"; // We'll create this CSS file later

// Define interfaces for type safety
interface AppointmentDetailDto {
  id: number;
  appointmentDate: string;
  doctor: {
    id: number;
    firstName: string;
    lastName: string;
    email: string;
  };
  patient: {
    id: number;
    firstName: string;
    lastName: string;
    email: string;
  };
  notes: string;
}

interface EditAppointmentDto {
  appointmentDate: string;
  notes: string;
}

interface AppointmentModalProps {
  appointmentId: number | null;
  token: string;
  userRole: number; // We need the user role to determine what actions are allowed
  userId: number;
  onClose: () => void;
  onAppointmentUpdated: () => void; // To refresh the appointments list after an update
}

const AppointmentDetailsModal: React.FC<AppointmentModalProps> = ({
  appointmentId,
  token,
  userRole,
  userId,
  onClose,
  onAppointmentUpdated
}) => {
  // State for storing appointment details
  const [appointment, setAppointment] = useState<AppointmentDetailDto | null>(null);
  // State for loading status
  const [loading, setLoading] = useState<boolean>(true);
  // State for error messages
  const [error, setError] = useState<string | null>(null);
  // State for edit mode
  const [isEditing, setIsEditing] = useState<boolean>(false);
  // State for edited appointment data
  const [editData, setEditData] = useState<EditAppointmentDto>({
    appointmentDate: "",
    notes: ""
  });
  // State for delete confirmation
  const [showDeleteConfirm, setShowDeleteConfirm] = useState<boolean>(false);

  // Fetch appointment details when modal opens
  useEffect(() => {
    if (appointmentId) {
      fetchAppointmentDetails(appointmentId);
    }
  }, [appointmentId]);

  // Function to fetch appointment details
  const fetchAppointmentDetails = async (id: number) => {
    setLoading(true);
    setError(null);

    try {
      // Call the API endpoint to get appointment details
      const response = await fetch(`http://localhost:5156/api/appointments/${id}`, {
        method: "GET",
        headers: {
          "Authorization": `Bearer ${token}`,
          "Content-Type": "application/json"
        }
      });

      if (!response.ok) {
        const errorText = await response.text();
        throw new Error(errorText || `Failed to fetch appointment details (Status: ${response.status})`);
      }

      const data = await response.json();
      setAppointment(data);
      
      // Initialize edit data with current appointment values
      setEditData({
        appointmentDate: data.appointmentDate,
        notes: data.notes || ""
      });
    } catch (err) {
      console.error("Error fetching appointment details:", err);
      setError(`Failed to load appointment details: ${err.message}`);
    } finally {
      setLoading(false);
    }
  };

  // Function to handle editing appointment
  const handleEditAppointment = async () => {
    try {
      // Call the API endpoint to update appointment
      const response = await fetch(`http://localhost:5156/api/appointments/${appointmentId}`, {
        method: "PUT",
        headers: {
          "Authorization": `Bearer ${token}`,
          "Content-Type": "application/json"
        },
        body: JSON.stringify({
          appointmentDate: editData.appointmentDate,
          notes: editData.notes,
          // Include these IDs as they're required by your API
          doctorId: appointment?.doctor.id,
          patientId: appointment?.patient.id
        })
      });

      if (!response.ok) {
        const errorText = await response.text();
        throw new Error(errorText || `Failed to update appointment (Status: ${response.status})`);
      }

      // Exit edit mode and refresh appointment details
      setIsEditing(false);
      fetchAppointmentDetails(appointmentId!);
      // Notify parent component that an update occurred
      onAppointmentUpdated();
    } catch (err) {
      console.error("Error updating appointment:", err);
      setError(`Failed to update appointment: ${err.message}`);
    }
  };

  // Function to handle deleting appointment
  const handleDeleteAppointment = async () => {
    try {
      // Call the API endpoint to delete appointment
      const response = await fetch(`http://localhost:5156/api/appointments/${appointmentId}`, {
        method: "DELETE",
        headers: {
          "Authorization": `Bearer ${token}`,
          "Content-Type": "application/json"
        }
      });

      if (!response.ok) {
        const errorText = await response.text();
        throw new Error(errorText || `Failed to delete appointment (Status: ${response.status})`);
      }

      // Close the modal and refresh the appointments list
      onAppointmentUpdated();
      onClose();
    } catch (err) {
      console.error("Error deleting appointment:", err);
      setError(`Failed to delete appointment: ${err.message}`);
    }
  };

  // Function to format date for display
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

  // Function to format date for input field
  const formatDateForInput = (dateString: string): string => {
    const date = new Date(dateString);
    return date.toISOString().slice(0, 16); // Format: YYYY-MM-DDThh:mm
  };

  // Check if user has permission to edit this appointment
  const canEdit = () => {
    if (!appointment) return false;
    
    // Doctors can edit their own appointments
    if (userRole === 0 && userId === appointment.doctor.id) return true;
    
    // We need to implement a check for assistants assigned to doctors
    // For now, let's assume all assistants can edit
    if (userRole === 2) return true;
    
    return false;
  };

  // Check if user has permission to delete this appointment
  const canDelete = () => {
    if (!appointment) return false;
    
    // Doctors can delete their own appointments
    if (userRole === 0 && userId === appointment.doctor.id) return true;
    
    // Patients can delete their own appointments
    if (userRole === 1 && userId === appointment.patient.id) return true;
    
    // We need to implement a check for assistants assigned to doctors
    // For now, let's assume all assistants can delete
    if (userRole === 2) return true;
    
    return false;
  };

  return (
    <div className="modal-backdrop">
      <div className="modal-content">
        {/* Modal Header */}
        <div className="modal-header">
          <h2>{isEditing ? "Edit Appointment" : "Appointment Details"}</h2>
          <button className="close-button" onClick={onClose}>×</button>
        </div>

        {/* Loading State */}
        {loading && (
          <div className="modal-loading">
            <p>Loading appointment details...</p>
          </div>
        )}

        {/* Error State */}
        {error && (
          <div className="modal-error">
            <p>{error}</p>
            <button onClick={() => fetchAppointmentDetails(appointmentId!)}>
              Try Again
            </button>
          </div>
        )}

        {/* Appointment Details */}
        {!loading && !error && appointment && (
          <div className="modal-body">
            {!isEditing ? (
              // View Mode
              <>
                <div className="appointment-info">
                  <p><strong>Appointment ID:</strong> {appointment.id}</p>
                  <p><strong>Date & Time:</strong> {formatDate(appointment.appointmentDate)}</p>
                  <p><strong>Doctor:</strong> Dr. {appointment.doctor.firstName} {appointment.doctor.lastName}</p>
                  <p><strong>Patient:</strong> {appointment.patient.firstName} {appointment.patient.lastName}</p>
                  <p><strong>Notes:</strong> {appointment.notes || "No notes provided"}</p>
                </div>
                
                {/* Action Buttons */}
                <div className="modal-actions">
                  {canEdit() && (
                    <button 
                      className="edit-button" 
                      onClick={() => setIsEditing(true)}
                    >
                      Edit Appointment
                    </button>
                  )}
                  
                  {canDelete() && (
                    <button 
                      className="delete-button" 
                      onClick={() => setShowDeleteConfirm(true)}
                    >
                      Cancel Appointment
                    </button>
                  )}
                </div>
              </>
            ) : (
              // Edit Mode
              <>
                <div className="edit-form">
                  <div className="form-group">
                    <label htmlFor="appointmentDate">Date & Time:</label>
                    <input
                      type="datetime-local"
                      id="appointmentDate"
                      value={formatDateForInput(editData.appointmentDate)}
                      onChange={(e) => setEditData({...editData, appointmentDate: e.target.value})}
                    />
                  </div>
                  
                  <div className="form-group">
                    <label htmlFor="notes">Notes:</label>
                    <textarea
                      id="notes"
                      value={editData.notes}
                      onChange={(e) => setEditData({...editData, notes: e.target.value})}
                      rows={4}
                    />
                  </div>
                </div>
                
                {/* Edit Mode Buttons */}
                <div className="modal-actions">
                  <button 
                    className="save-button" 
                    onClick={handleEditAppointment}
                  >
                    Save Changes
                  </button>
                  
                  <button 
                    className="cancel-button" 
                    onClick={() => setIsEditing(false)}
                  >
                    Cancel
                  </button>
                </div>
              </>
            )}
          </div>
        )}
        
        {/* Delete Confirmation Dialog */}
        {showDeleteConfirm && (
          <div className="confirm-dialog">
            <p>Are you sure you want to cancel this appointment?</p>
            <div className="confirm-actions">
              <button 
                className="confirm-delete-button" 
                onClick={handleDeleteAppointment}
              >
                Yes, Cancel Appointment
              </button>
              
              <button 
                className="cancel-button" 
                onClick={() => setShowDeleteConfirm(false)}
              >
                No, Keep Appointment
              </button>
            </div>
          </div>
        )}
      </div>
    </div>
  );
};

export default AppointmentDetailsModal;