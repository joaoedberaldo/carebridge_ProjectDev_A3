import React, { useState, useEffect } from "react";
import "../../styles/DoctorSchedule.css";

const DoctorSchedule = ({ user,token  }) => {
  const [schedules, setSchedules] = useState([]);
  const [showForm, setShowForm] = useState(false);
  const [editMode, setEditMode] = useState(false);
  const [currentScheduleId, setCurrentScheduleId] = useState(null);
  const [formData, setFormData] = useState({
    startTime: "",
    endTime: "",
    description: "",
  });

    // Fetch schedules from API
    const fetchSchedules = async () => {
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
      }
    } catch (error) {
      console.error("Fetch error:", error);
    }
  };

  useEffect(() => {
    if (token) {
      fetchSchedules();
    }
  }, [token]);

   // Handle Add/Edit Form Submission
//   const handleAddSchedule = async (e) => {
//     e.preventDefault();

//     try {
//       const response = await fetch("http://localhost:5156/api/doctors/schedules", {
//         method: "POST",
//         headers: {
//           "Content-Type": "application/json",
//           Authorization: `Bearer ${token}`,
//         },
//         body: JSON.stringify(formData),
//       });

//       if (response.ok) {
//         alert("Schedule added successfully!");
//         fetchSchedules();
//         setShowForm(false);
//       } else {
//         alert("Error adding schedule.");
//       }
//     } catch (error) {
//       console.error("Error adding schedule:", error);
//     }
//   };
    const handleSubmit = async (e) => {
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
            alert(editMode ? "Schedule updated successfully!" : "Schedule added successfully!");
            fetchSchedules();
            setShowForm(false);
            setEditMode(false);
            setCurrentScheduleId(null);
            setFormData({ startTime: "", endTime: "", description: "" });
        } else {
            alert("Error saving schedule.");
        }
        } catch (error) {
        console.error("Error saving schedule:", error);
        }
    };

   // Handle Delete
  const handleDeleteSchedule = async (id) => {
    try {
      const response = await fetch(`http://localhost:5156/api/doctors/schedules/${id}`, {
        method: "DELETE",
        headers: {
          Authorization: `Bearer ${token}`,
        },
      });

      if (response.ok) {
        alert("Schedule deleted!");
        fetchSchedules();
      } else {
        alert("Failed to delete.");
      }
    } catch (error) {
      console.error("Error deleting schedule:", error);
    }
  };

  //Handle edit
  const handleEditSchedule = (schedule) => {
    setEditMode(true);
    setCurrentScheduleId(schedule.id);
    setFormData({
      startTime: schedule.startTime,
      endTime: schedule.endTime,
      description: schedule.description,
    });
    setShowForm(true);
  };

//   return (
//     <div className="doctor-schedule-container">
//       <h2>Your Schedules</h2>

//       <button onClick={() => setShowForm(!showForm)}>Add Schedule</button>

//       {showForm && (
//         <form onSubmit={handleAddSchedule}>
//           <label>Start Time:</label>
//           <input
//             type="datetime-local"
//             value={formData.startTime}
//             onChange={(e) => setFormData({ ...formData, startTime: e.target.value })}
//             required
//           />

//           <label>End Time:</label>
//           <input
//             type="datetime-local"
//             value={formData.endTime}
//             onChange={(e) => setFormData({ ...formData, endTime: e.target.value })}
//             required
//           />

//           <label>Description:</label>
//           <input
//             type="text"
//             value={formData.description}
//             onChange={(e) => setFormData({ ...formData, description: e.target.value })}
//             required
//           />

//           <button type="submit">Save Schedule</button>
//         </form>
//       )}

//       <div className="schedule-list">
//         {schedules.map((schedule) => (
//           <div key={schedule.id} className="schedule-item">
//             <p><strong>{schedule.description}</strong></p>
//             <p>{new Date(schedule.startTime).toLocaleString()} - {new Date(schedule.endTime).toLocaleString()}</p>
            
//             <button onClick={() => handleDeleteSchedule(schedule.id)}>Delete</button>
//           </div>
//         ))}
//       </div>
//     </div>
//   );
// };

return (
    <div className="doctor-schedule-container">
      <h2>Your Schedules</h2>

      <button onClick={() => { setShowForm(true); setEditMode(false); }}>Add Schedule</button>

      {showForm && (
        <form onSubmit={handleSubmit}>
          <label>Start Time:</label>
          <input
            type="datetime-local"
            value={formData.startTime}
            onChange={(e) => setFormData({ ...formData, startTime: e.target.value })}
            required
          />

          <label>End Time:</label>
          <input
            type="datetime-local"
            value={formData.endTime}
            onChange={(e) => setFormData({ ...formData, endTime: e.target.value })}
            required
          />

          <label>Description:</label>
          <input
            type="text"
            value={formData.description}
            onChange={(e) => setFormData({ ...formData, description: e.target.value })}
            required
          />

          <button type="submit">{editMode ? "Save Changes" : "Add Schedule"}</button>
          <button type="button" onClick={() => setShowForm(false)}>Cancel</button>
        </form>
      )}

      <div className="schedule-list">
        {schedules.map((schedule) => (
          <div key={schedule.id} className="schedule-item">
            <p><strong>{schedule.description}</strong></p>
            <p>{new Date(schedule.startTime).toLocaleString()} - {new Date(schedule.endTime).toLocaleString()}</p>
            
            <button onClick={() => handleEditSchedule(schedule)}>Edit</button>
            <button onClick={() => handleDeleteSchedule(schedule.id)}>Delete</button>
          </div>
        ))}
      </div>
    </div>
  );
};

export default DoctorSchedule;