import { useState } from "react";
import { useNavigate, Link } from "react-router-dom";
import { useAuth } from "../context/AuthContext";

export default function Register() {
  const { register } = useAuth();
  const navigate = useNavigate();
  const [form, setForm] = useState({ username: "", email: "", password: "" });
  const [errors, setErrors] = useState({});
  const [serverError, setServerError] = useState("");

  const validate = () => {
    const e = {};
    if (!form.username.trim()) e.username = "Username is required";
    else if (form.username.trim().length < 3) e.username = "Username must be at least 3 characters";

    if (!form.email.trim()) e.email = "Email is required";
    else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(form.email)) e.email = "Enter a valid email";

    if (!form.password) e.password = "Password is required";
    else if (form.password.length < 6) e.password = "Password must be at least 6 characters";
    else if (!/[A-Z]/.test(form.password)) e.password = "Password needs an uppercase letter";
    else if (!/[0-9]/.test(form.password)) e.password = "Password needs a digit";

    setErrors(e);
    return Object.keys(e).length === 0;
  };

  const handleChange = (field) => (event) => {
    setForm({ ...form, [field]: event.target.value });
    setErrors((prev) => ({ ...prev, [field]: undefined }));
  };

  const submit = async (e) => {
    e.preventDefault();
    setServerError("");
    if (!validate()) return;

    try {
      await register(form.username, form.email, form.password);
      navigate("/");
   } catch (err) {
    if (err.fieldErrors) setErrors(err.fieldErrors);   // per-field (e.g. Password rule)
    else setServerError(err.detail || err.title);       // general (e.g. duplicate user)
    }
  };

  return (
    <div className="card">
      <h2>Create account</h2>
      <form onSubmit={submit} noValidate>
        <div className="field">
          <label>Username <span className="required">*</span></label>
          <input
            placeholder="Username"
            value={form.username}
            onChange={handleChange("username")}
            className={errors.username ? "invalid" : ""}
          />
          {errors.username && <span className="field-error">{errors.username}</span>}
        </div>

        <div className="field">
          <label>Email <span className="required">*</span></label>
          <input
            placeholder="Email"
            type="email"
            value={form.email}
            onChange={handleChange("email")}
            className={errors.email ? "invalid" : ""}
          />
          {errors.email && <span className="field-error">{errors.email}</span>}
        </div>

        <div className="field">
          <label>Password <span className="required">*</span></label>
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
        <button type="submit">Register</button>
      </form>
      <p>Have an account? <Link to="/login">Login</Link></p>
    </div>
  );
}