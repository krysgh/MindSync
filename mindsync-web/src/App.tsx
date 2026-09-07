import { BrowserRouter, Routes, Route } from 'react-router-dom';
import { Login } from './pages/Login';
import { Register } from './pages/Register';
import { Home } from './pages/Home';
import { Insights } from './pages/Insights';
import { Player } from './pages/Player';
import { DialogProvider } from './contexts/DialogProvider';

export function App() {
  return (
    <BrowserRouter>
      <DialogProvider>
        <div className="background-blob-container">
          <div className="blob blob-red"></div>
          <div className="blob blob-blue"></div>
          <div className="blob blob-purple-collision"></div>
        </div>

        <Routes>
          <Route path="/" element={<Login />} />
          <Route path="/register" element={<Register />} />
          <Route path="/home" element={<Home />} />
          <Route path="/insights" element={<Insights />} />
          <Route path="/player/:sessionId" element={<Player />} />
        </Routes>
      </DialogProvider>
    </BrowserRouter>
  );
}

export default App;