import React, { useState, useEffect } from "react";
import "../../styles/Coverage.css";
import { toast, ToastContainer } from "react-toastify";
import "react-toastify/dist/ReactToastify.css";

// Define the interface for plan data
interface PlanData {
  planName: string;
  description: string;
}

// Define the props for the component
interface CoverageProps {
  token: string;
}

const Coverage: React.FC<CoverageProps> = ({ token }) => {
  // State to store plans data
  const [plans, setPlans] = useState<PlanData[]>([]);
  // State to track loading status
  const [loading, setLoading] = useState<boolean>(true);
  // State to track errors
  const [error, setError] = useState<string | null>(null);
  // State to track if request is being retried
  const [retrying, setRetrying] = useState<boolean>(false);

  // Fetch plans when component mounts
  useEffect(() => {
    fetchPlans();
  }, []);

  // Function to fetch plans from the backend
  const fetchPlans = async () => {
    setLoading(true);
    setError(null);
    setRetrying(false);

    try {
      // Make API call to get health coverage plans
      const response = await fetch("http://localhost:5156/api/info/plans", {
        method: "GET",
        headers: {
          "Authorization": `Bearer ${token}`,
          "Content-Type": "application/json"
        }
      });

      // Check if response is successful
      if (!response.ok) {
        const errorText = await response.text();
        throw new Error(errorText || `Failed to fetch plans (Status: ${response.status})`);
      }

      // Parse the response data
      const data = await response.json();
      console.log("Plans data received:", data);
      
      // Update state with the fetched plans
      setPlans(data);
    } catch (err) {
      console.error("Error fetching plans:", err);
      setError(`Failed to load coverage plans: ${err instanceof Error ? err.message : 'Unknown error'}`);
      toast.error("Failed to load coverage plans");
    } finally {
      setLoading(false);
    }
  };

  // Function to handle retry
  const handleRetry = () => {
    setRetrying(true);
    fetchPlans();
  };

  return (
    <div className="dashboard-content">
      <ToastContainer />
      
      <div className="coverage-container">
        <h2>Health Coverage Information</h2>
        <p className="coverage-intro">
          CareBridge supports various health coverage plans. View the details below to understand what's covered under each plan.
        </p>

        {/* Loading State */}
        {loading && (
          <div className="loading-container">
            <div className="loading-spinner">Loading coverage plans...</div>
            {retrying && <p>Retrying...</p>}
          </div>
        )}

        {/* Error State */}
        {error && !loading && (
          <div className="error-message">
            <p>{error}</p>
            <button 
              onClick={handleRetry}
              className="retry-button"
            >
              Try Again
            </button>
          </div>
        )}

        {/* No Plans State */}
        {!loading && !error && plans.length === 0 && (
          <div className="no-plans-message">
            <p>No coverage plans are currently available. Please check back later.</p>
          </div>
        )}

        {/* Plans Grid */}
        {!loading && !error && plans.length > 0 && (
          <div className="plans-grid">
            {plans.map((plan, index) => (
              <div key={index} className="plan-card">
                <div className="plan-header">
                  <h3>{plan.planName}</h3>
                </div>
                <div className="plan-details">
                  <p>{plan.description}</p>
                </div>
              </div>
            ))}
          </div>
        )}

        {/* Additional Help Section */}
        <div className="additional-help">
          <h3>Need Assistance?</h3>
          <p>If you have questions about coverage or using the CareBridge platform, please contact our support team:</p>
          <div className="contact-info">
            <p><strong>Email:</strong> support@carebridge.com</p>
            <p><strong>Phone:</strong> 1-800-123-4444 </p>
            <p><strong>Hours:</strong> Monday to Friday, 8:00 AM - 8:00 PM ET</p>
          </div>
        </div>
      </div>
    </div>
  );
};

export default Coverage;