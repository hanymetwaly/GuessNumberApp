import axios from "axios";

const client = axios.create({
  baseURL: import.meta.env.VITE_API_URL,
});

// Attach the JWT to every request if we have one
client.interceptors.request.use((config) => {
  const token = localStorage.getItem("token");
  if (token) config.headers.Authorization = `Bearer ${token}`;
  return config;
});

// If the token is rejected, kick the user back to login
client.interceptors.response.use(
  (res) => res,
  (err) => {
    const data = err.response?.data;
    const normalized = {
      status: err.response?.status,
      title: data?.title || "Something went wrong",
      detail: data?.detail,
      fieldErrors: data?.errors
        ? Object.fromEntries(
            Object.entries(data.errors).map(([k, v]) => [k.toLowerCase(), v[0]])
          )
        : null,
      traceId: data?.traceId,
    };
    if (err.response?.status === 401) {
      localStorage.removeItem("token");
      window.location.href = "/login";
    }
    return Promise.reject(normalized);
  }
);

export default client;