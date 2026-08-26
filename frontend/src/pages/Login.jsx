import { useState } from "react";
import { useNavigate, Link } from "react-router-dom";
import { useAuth } from "../context/AuthContext";

export default function Login() {
  const { login } = useAuth();
  const navigate = useNavigate();
  const [form, setForm] = useState({ username: "", password: "" });
  const [errors, setErrors] = useState({});   // per-field errors
  const [serverError, setServerError] = useState("");

  const validate = () => {
    const e = {};
    if (!form.username.trim()) e.username = "Username is required";
    if (!form.password) e.password = "Password is required";
    setErrors(e);
    return Object.keys(e).length === 0; // true if no errors
  };

  const handleChange = (field) => (event) => {
    setForm({ ...form, [field]: event.target.value });
    // clear that field's error as the user types
    setErrors((prev) => ({ ...prev, [field]: undefined }));
  };

  const submit = async (e) => {
    e.preventDefault();
    setServerError("");
    if (!validate()) return;          // stop if client-side checks fail

    try {
      await login(form.username, form.password);
      navigate("/");
    } catch (err) {
  if (err.fieldErrors) setErrors(err.fieldErrors);   // per-field (e.g. Password rule)
  else setServerError(err.detail || err.title);       // general (e.g. duplicate user)
}
  };

  return (
    <div className="card">
      <h2>Login</h2>
      <form onSubmit={submit} noValidate>
        <div className="field">
          <input
            placeholder="Username"
            value={form.username}
            onChange={handleChange("username")}
            className={errors.username ? "invalid" : ""}
          />
          {errors.username && <span className="field-error">{errors.username}</span>}
        </div>

        <div className="field">
          <input
            placeholder="Password"
            type="password"
            value={form.password}
            onChange={handleChange("password")}
            className={errors.password ? "invalid" : ""}
          />
          {errors.password && <span className="field-error">{errors.password}</span>}
        </div>

        {serverError && <p className="error">{serverError}</p>}
        <button type="submit">Login</button>
      </form>
      <p>No account? <Link to="/register">Register</Link></p>
    </div>
  );
}