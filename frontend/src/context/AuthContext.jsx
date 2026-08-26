import { createContext, useContext, useState } from "react";
import client from "../api/client";

const AuthContext = createContext(null);

export function AuthProvider({ children }) {
  const [user, setUser] = useState(() => {
    const raw = localStorage.getItem("user");
    return raw ? JSON.parse(raw) : null;
  });

  const persist = (data) => {
    // data = { token, username, bestScore }
    localStorage.setItem("token", data.token);
    const u = { username: data.username, bestScore: data.bestScore };
    localStorage.setItem("user", JSON.stringify(u));
    setUser(u);
  };

  const register = async (username, email, password) => {
    const { data } = await client.post("/auth/register", { username, email, password });
    persist(data);
  };

  const login = async (username, password) => {
    const { data } = await client.post("/auth/login", { username, password });
    persist(data);
  };

  const logout = () => {
    localStorage.removeItem("token");
    localStorage.removeItem("user");
    setUser(null);
  };

  // Let the game update the stored best score after a new record
  const updateBestScore = (score) => {
    setUser((prev) => {
      const next = { ...prev, bestScore: score };
      localStorage.setItem("user", JSON.stringify(next));
      return next;
    });
  };

  return (
    <AuthContext.Provider value={{ user, register, login, logout, updateBestScore }}>
      {children}
    </AuthContext.Provider>
  );
}

export const useAuth = () => useContext(AuthContext);