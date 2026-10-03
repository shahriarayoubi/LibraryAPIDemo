import LoginForm from "./components/LoginForm";
import { useState } from "react";
function App() {
  const [token, setToken] = useState("");

  return (
    <div>
      <h1>Log in</h1>
      <LoginForm onTokenChange={(newToken) => setToken(newToken)} />
      {token && <p>Token: {token}</p>}
    </div>
  )
}

export default App
