import React, { useState } from "react";
import { toast } from "react-toastify";
import "../../styles/PrescriptionsModal.css";

interface NewPrescriptionModalProps {
  appointmentId: number;
  patientId: number;
  token: string;
  onClose: () => void;
  onPrescriptionCreated: () => void;
}

interface PrescriptionFormData {
  appointmentId: number;
  patientId: number;
  description: string;
  date: string;
}

const NewPrescriptionModal: React.FC<NewPrescriptionModalProps> = ({
  appointmentId,
  patientId,
  token,
  onClose,
  onPrescriptionCreated,
}) => {
  // Get current date in ISO format (YYYY-MM-DD)
  const currentDate = new Date().toISOString().split('T')[0];

  // State for form data
  const [formData, setFormData] = useState<PrescriptionFormData>({
    appointmentId: appointmentId,
    patientId: patientId,
    description: "",
    date: currentDate,
  });

  // State for loading status during submission
  const [isSubmitting, setIsSubmitting] = useState<boolean>(false);

  // Handle input changes
  const handleChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => {
    const { name, value } = e.target;
    setFormData({
      ...formData,
      [name]: value,
    });
  };

  // Handle form submission
  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    
    if (!formData.description.trim()) {
      toast.error("Please enter a description for the prescription");
      return;
    }

    setIsSubmitting(true);

    try {
      const response = await fetch("http://localhost:5156/api/prescriptions", {
        method: "POST",
        headers: {
          "Authorization": `Bearer ${token}`,
          "Content-Type": "application/json"
        },
        body: JSON.stringify(formData),
      });

      if (!response.ok) {
        const errorText = await response.text();
        throw new Error(errorText || `Failed to create prescription (Status: ${response.status})`);
      }

      // Check if there's response content before trying to parse it
      const contentType = response.headers.get("content-type");
      let responseData;
      
      if (contentType && contentType.includes("application/json")) {
        // Only try to parse JSON if the content type is JSON
        const text = await response.text();
        if (text) {
          try {
            responseData = JSON.parse(text);
          } catch (e) {
            console.log("Response couldn't be parsed as JSON:", text);
          }
        }
      }

      // Success regardless of response content
      toast.success("Prescription created successfully");
      onPrescriptionCreated();
    } catch (err) {
      console.error("Error creating prescription:", err);
      toast.error(`Failed to create prescription: ${err.message}`);
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <div className="modal-backdrop">
      <div className="modal-content new-prescription-modal">
        {/* Modal Header */}
        <div className="modal-header">
          <h2>New Prescription</h2>
          <button className="close-button" onClick={onClose}>×</button>
        </div>

        {/* Modal Body */}
        <div className="modal-body">
          <form onSubmit={handleSubmit}>
            <div className="form-group">
              <label htmlFor="date">Date:</label>
              <input
                type="date"
                id="date"
                name="date"
                value={formData.date}
                onChange={handleChange}
                required
              />
            </div>

            <div className="form-group">
              <label htmlFor="description">Description/Instructions:</label>
              <textarea
                id="description"
                name="description"
                value={formData.description}
                onChange={handleChange}
                placeholder="Enter prescription details and instructions"
                rows={5}
                required
              />
            </div>

            <div className="form-actions">
              <button
                type="submit"
                className="save-button"
                disabled={isSubmitting}
              >
                {isSubmitting ? "Creating..." : "Create Prescription"}
              </button>
              <button
                type="button"
                className="cancel-button"
                onClick={onClose}
                disabled={isSubmitting}
              >
                Cancel
              </button>
            </div>
          </form>
        </div>
      </div>
    </div>
  );
};

export default NewPrescriptionModal;