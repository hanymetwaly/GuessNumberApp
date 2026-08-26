import { useEffect, useState } from "react";
import client from "../api/client";

export default function Leaderboard() {
  const [rows, setRows] = useState([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    client.get("/game/leaderboard?count=10")
      .then(({ data }) => setRows(data))
      .finally(() => setLoading(false));
  }, []);

  const medal = (rank) => (rank === 1 ? "🥇" : rank === 2 ? "🥈" : rank === 3 ? "🥉" : rank);

  return (
    <div className="card">
      <h2>🏆 Global Leaderboard</h2>
      {loading ? <p>Loading…</p> : rows.length === 0 ? (
        <p>No winners yet. Be the first!</p>
      ) : (
        <table className="leaderboard">
          <thead>
            <tr><th>Rank</th><th>Player</th><th>Best (guesses)</th></tr>
          </thead>
          <tbody>
            {rows.map((r) => (
              <tr key={r.rank}>
                <td>{medal(r.rank)}</td>
                <td>{r.username}</td>
                <td>{r.bestScore}</td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
    </div>
  );
}