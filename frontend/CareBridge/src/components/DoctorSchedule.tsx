import React, { useState, useEffect } from "react";
import "../../styles/DoctorSchedule.css";
import { toast, ToastContainer } from "react-toastify";
import "react-toastify/dist/ReactToastify.css";

interface ScheduleData {
  id: number;
  startTime: string;
  endTime: string;
  description: string;
}

interface DoctorScheduleProps {
  user: {
    id: number;
    role: number;
  };
  token: string;
}

const DoctorSchedule: React.FC<DoctorScheduleProps> = ({ user, token }) => {
  // State to store schedules
  const [schedules, setSchedules] = useState<ScheduleData[]>([]);
  // State to control form visibility
  const [showForm, setShowForm] = useState<boolean>(false);
  // State to control edit mode
  const [editMode, setEditMode] = useState<boolean>(false);
  // State to store current schedule ID being edited
  const [currentScheduleId, setCurrentScheduleId] = useState<number | null>(null);
  // State to track loading status
  const [loading, setLoading] = useState<boolean>(true);
  // State to track errors
  const [error, setError] = useState<string | null>(null);
  // State for form data
  const [formData, setFormData] = useState<{
    startTime: string;
    endTime: string;
    description: string;
  }>({
    startTime: "",
    endTime: "",
    description: "",
  });

  // Fetch schedules from API
  const fetchSchedules = async () => {
    setLoading(true);
    setError(null);
    
    try {
      const response = await fetch("http://localhost:5156/api/doctors/schedules", {
        method: "GET",
        headers: {
          "Content-Type": "application/json",
          Authorization: `Bearer ${token}`,
        },
      });

      if (response.ok) {
        const data = await response.json();
        setSchedules(data);
      } else {
        console.error("Error fetching schedules:", response.status);
        setError("Failed to load schedules");
        toast.error("Failed to load schedules");
      }
    } catch (error) {
      console.error("Fetch error:", error);
      setError("Could not connect to the server");
      toast.error("Could not connect to the server");
    } finally {
      setLoading(false);
    }
  };

  // Fetch schedules when component mounts or token changes
  useEffect(() => {
    if (token) {
      fetchSchedules();
    }
  }, [token]);

  // Handle form submission for adding or updating a schedule
  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    const url = editMode
      ? `http://localhost:5156/api/doctors/schedules/${currentScheduleId}`
      : "http://localhost:5156/api/doctors/schedules";

    const method = editMode ? "PUT" : "POST";

    try {
      const response = await fetch(url, {
        method: method,
        headers: {
          "Content-Type": "application/json",
          Authorization: `Bearer ${token}`,
        },
        body: JSON.stringify(formData),
      });

      if (response.ok) {
        toast.success(editMode ? "Schedule updated successfully!" : "Schedule added successfully!");
        fetchSchedules();
        setShowForm(false);
        setEditMode(false);
        setCurrentScheduleId(null);
        setFormData({ startTime: "", endTime: "", description: "" });
      } else {
        toast.error("Error saving schedule");
      }
    } catch (error) {
      console.error("Error saving schedule:", error);
      toast.error("Could not connect to the server");
    }
  };

  // Handle deleting a schedule
  const handleDeleteSchedule = async (id: number) => {
    if (!window.confirm("Are you sure you want to delete this schedule?")) {
      return;
    }
    
    try {
      const response = await fetch(`http://localhost:5156/api/doctors/schedules/${id}`, {
        method: "DELETE",
        headers: {
          Authorization: `Bearer ${token}`,
        },
      });

      if (response.ok) {
        toast.success("Schedule deleted successfully!");
        fetchSchedules();
      } else {
        toast.error("Failed to delete schedule");
      }
    } catch (error) {
      console.error("Error deleting schedule:", error);
      toast.error("Could not connect to the server");
    }
  };

  // Handle editing a schedule
  const handleEditSchedule = (schedule: ScheduleData) => {
    setEditMode(true);
    setCurrentScheduleId(schedule.id);
    setFormData({
      startTime: schedule.startTime,
      endTime: schedule.endTime,
      description: schedule.description,
    });
    setShowForm(true);
  };

  // Format date for display
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

  return (
    <div className="dashboard-content">
      <h2>Your Schedule</h2>
      <ToastContainer />
      
      <div className="schedule-actions">
        <button 
          className="add-schedule-button" 
          onClick={() => { 
            setShowForm(true); 
            setEditMode(false);
            setFormData({ startTime: "", endTime: "", description: "" });
          }}
        >
          Add Schedule
        </button>
      </div>

      {/* Schedule Form Modal */}
      {showForm && (
        <div className="modal-overlay" onClick={() => setShowForm(false)}>
          <div className="modal-content" onClick={(e) => e.stopPropagation()}>
            <h3>{editMode ? "Edit Schedule" : "Add Schedule"}</h3>
            <form onSubmit={handleSubmit}>
              <div className="form-group">
                <label>Start Time:</label>
                <input
                  type="datetime-local"
                  value={formData.startTime}
                  onChange={(e) => setFormData({ ...formData, startTime: e.target.value })}
                  required
                />
              </div>

              <div className="form-group">
                <label>End Time:</label>
                <input
                  type="datetime-local"
                  value={formData.endTime}
                  onChange={(e) => setFormData({ ...formData, endTime: e.target.value })}
                  required
                />
              </div>

              <div className="form-group">
                <label>Description:</label>
                <input
                  type="text"
                  value={formData.description}
                  onChange={(e) => setFormData({ ...formData, description: e.target.value })}
                  required
                  placeholder="E.g., Office Hours, Consultation Block"
                />
              </div>

              <div className="form-actions">
                <button type="submit" className="save-button">
                  {editMode ? "Save Changes" : "Add Schedule"}
                </button>
                <button 
                  type="button" 
                  className="cancel-button"
                  onClick={() => setShowForm(false)}
                >
                  Cancel
                </button>
              </div>
            </form>
          </div>
        </div>
      )}

      {/* Loading State */}
      {loading && (
        <div className="loading-container">
          <div className="loading-spinner">Loading schedules...</div>
        </div>
      )}

      {/* Error State */}
      {error && (
        <div className="error-message">
          {error}
          <button 
            onClick={fetchSchedules}
            className="retry-button"
          >
            Try Again
          </button>
        </div>
      )}

      {/* No Schedules State */}
      {!loading && !error && schedules.length === 0 && (
        <div className="no-schedules-message">
          You don't have any schedules yet. Click "Add Schedule" to create one.
        </div>
      )}

      {/* Schedules List */}
      <div className="schedules-grid">
        {schedules.map((schedule) => (
          <div key={schedule.id} className="schedule-card">
            <div className="schedule-header">
              <h3>{schedule.description}</h3>
            </div>
            <div className="schedule-details">
              <p><strong>Start:</strong> {formatDate(schedule.startTime)}</p>
              <p><strong>End:</strong> {formatDate(schedule.endTime)}</p>
            </div>
            <div className="schedule-actions">
              <button 
                onClick={() => handleEditSchedule(schedule)}
                className="edit-schedule-button"
              >
                Edit
              </button>
              <button 
                onClick={() => handleDeleteSchedule(schedule.id)}
                className="delete-schedule-button"
              >
                Delete
              </button>
            </div>
          </div>
        ))}
      </div>
    </div>
  );
};

export default DoctorSchedule;