import { useState, type SubmitEvent } from "react";

interface LoginResponse {
  userName: string;
  token: string;
}

interface ProblemDetails {
  title: string;
}

interface LoginFormProps {
  onTokenChange: (token: string) => void;
}
const LoginForm = ({ onTokenChange }: LoginFormProps) => {
  const [userName, setUserName] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState("");

  async function handleSubmit(e: SubmitEvent<HTMLFormElement>) {
    e.preventDefault();
    const response = await fetch(
      "http://localhost:5186/api/authentication/login",
      {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify({ userName, password }),
      },
    );

    if (!response.ok) {
      const problem: ProblemDetails = await response.json();
      setError(problem.title ?? "Login failed. Please try again.");
      onTokenChange("");

      return;
    }

    const data: LoginResponse = await response.json();
    onTokenChange(data.token);
    setError("");
  }

  return (
    <form onSubmit={handleSubmit}>
      <div>
        <label htmlFor="username">Username</label>
        <input
          id="username"
          type="text"
          value={userName}
          onChange={(e) => setUserName(e.target.value)}
        />
      </div>
      <div>
        <label htmlFor="password">Password</label>
        <input
          id="password"
          type="password"
          value={password}
          onChange={(e) => setPassword(e.target.value)}
        />
      </div>
      <button type="submit">Log in</button>
      {error && <p style={{ color: "red" }}>{error}</p>}
    </form>
  );
};

export default LoginForm;
