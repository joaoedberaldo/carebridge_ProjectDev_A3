import React, { useState } from "react";
import { toast } from "react-toastify";
import "../../styles/PrescriptionsModal.css";

interface NewMedicationModalProps {
  prescriptionId: number;
  token: string;
  onClose: () => void;
  onMedicationAdded: () => void;
}

interface MedicationFormData {
  name: string;
  dosage: string;
  frequency: string;
  notes: string;
}

const NewMedicationModal: React.FC<NewMedicationModalProps> = ({
  prescriptionId,
  token,
  onClose,
  onMedicationAdded,
}) => {
  // State for form data
  const [formData, setFormData] = useState<MedicationFormData>({
    name: "",
    dosage: "",
    frequency: "",
    notes: "",
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
    
    if (!formData.name.trim() || !formData.dosage.trim() || !formData.frequency.trim()) {
      toast.error("Please fill in all required fields");
      return;
    }

    setIsSubmitting(true);

    try {
      const response = await fetch(`http://localhost:5156/api/prescriptions/${prescriptionId}/medications`, {
        method: "POST",
        headers: {
          "Authorization": `Bearer ${token}`,
          "Content-Type": "application/json"
        },
        body: JSON.stringify(formData),
      });

      if (!response.ok) {
        const errorText = await response.text();
        throw new Error(errorText || `Failed to add medication (Status: ${response.status})`);
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
      toast.success("Medication added successfully");
      onMedicationAdded();
    } catch (err) {
      console.error("Error adding medication:", err);
      toast.error(`Failed to add medication: ${err.message}`);
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <div className="modal-backdrop">
      <div className="modal-content new-medication-modal">
        {/* Modal Header */}
        <div className="modal-header">
          <h2>Add Medication</h2>
          <button className="close-button" onClick={onClose}>×</button>
        </div>

        {/* Modal Body */}
        <div className="modal-body">
          <form onSubmit={handleSubmit}>
            <div className="form-group">
              <label htmlFor="name">Medication Name:</label>
              <input
                type="text"
                id="name"
                name="name"
                value={formData.name}
                onChange={handleChange}
                placeholder="Enter medication name"
                required
              />
            </div>

            <div className="form-group">
              <label htmlFor="dosage">Dosage:</label>
              <input
                type="text"
                id="dosage"
                name="dosage"
                value={formData.dosage}
                onChange={handleChange}
                placeholder="E.g., 500mg, 1 tablet, 10ml"
                required
              />
            </div>

            <div className="form-group">
              <label htmlFor="frequency">Frequency:</label>
              <input
                type="text"
                id="frequency"
                name="frequency"
                value={formData.frequency}
                onChange={handleChange}
                placeholder="E.g., Once daily, Twice daily, Every 8 hours"
                required
              />
            </div>

            <div className="form-group">
              <label htmlFor="notes">Additional Notes:</label>
              <textarea
                id="notes"
                name="notes"
                value={formData.notes}
                onChange={handleChange}
                placeholder="Enter any additional instructions or notes"
                rows={3}
              />
            </div>

            <div className="form-actions">
              <button
                type="submit"
                className="save-button"
                disabled={isSubmitting}
              >
                {isSubmitting ? "Adding..." : "Add Medication"}
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

export default NewMedicationModal;