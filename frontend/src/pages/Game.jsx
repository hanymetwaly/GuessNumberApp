import { useState } from "react";
import confetti from "canvas-confetti";
import client from "../api/client";
import { useAuth } from "../context/AuthContext";

export default function Game() {
  const { user, updateBestScore } = useAuth();
  const [gameId, setGameId] = useState(null);
  const [guess, setGuess] = useState("");
  const [message, setMessage] = useState("");
  const [history, setHistory] = useState([]);
  const [attempts, setAttempts] = useState(0);
  const [finished, setFinished] = useState(false);

  const startGame = async () => {
    const { data } = await client.post("/game/start");
    setGameId(data.gameId);
    setMessage(data.message);
    setHistory([]);
    setAttempts(0);
    setFinished(false);
    setGuess("");
  };

  const submitGuess = async (e) => {
    e.preventDefault();
    if (!gameId || guess === "") return;

    const value = Number(guess);
    const { data } = await client.post("/game/guess", { gameId, guess: value });
    setAttempts(data.attempts);

    if (data.result === "correct") {
      setFinished(true);
      setMessage(`🎉 Correct! You got it in ${data.attempts} guesses.`);
      setHistory((h) => [...h, { value, hint: "correct" }]);
      confetti({ particleCount: 150, spread: 80, origin: { y: 0.6 } });
      if (data.isNewRecord) {
        setMessage(`🏆 NEW RECORD! ${data.attempts} guesses!`);
        updateBestScore(data.bestScore);
      }
    } else {
      const hint = data.result === "higher" ? "⬆️ Go higher" : "⬇️ Go lower";
      setMessage(hint);
      setHistory((h) => [...h, { value, hint: data.result }]);
    }
    setGuess("");
  };

  return (
    <div className="card">
      <h2>Guess the Number (1–43)</h2>
      <p className="best">
        Your best score: {user?.bestScore ?? "— (no wins yet)"}
      </p>

      {!gameId || finished ? (
        <button onClick={startGame}>
          {finished ? "Play again" : "Start game"}
        </button>
      ) : null}

      {gameId && !finished && (
        <form onSubmit={submitGuess} className="guess-form">
          <input
            type="number" min="1" max="43"
            value={guess}
            onChange={(e) => setGuess(e.target.value)}
            placeholder="1-43" autoFocus />
          <button type="submit">Guess</button>
        </form>
      )}

      {message && <p className="message">{message}</p>}
      {attempts > 0 && <p>Attempts: {attempts}</p>}

      {history.length > 0 && (
        <div className="history">
          <h4>Your guesses</h4>
          <div className="chips">
            {history.map((h, i) => (
              <span key={i} className={`chip ${h.hint}`}>{h.value}</span>
            ))}
          </div>
        </div>
      )}
    </div>
  );
}