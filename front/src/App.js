import React from 'react';
import { Routes, Route } from 'react-router-dom';
import { Sidebar } from './components/Sidebar';
import { Balances } from './components/Balances';
import { Receipts } from './components/Receipts';
import { Shipments } from './components/Shipments';
import { Clients } from './components/Clients';
import { Units } from './components/Units';
import { Resources } from './components/Resources';

const App = () => {
  return (
    <div style={{ display: 'flex', height: '100vh' }}>
      <Sidebar />
      <div style={{ flex: 1, padding: '20px' }}>
        <Routes>
          <Route path="/" element={<Balances />} />
          <Route path="/balances" element={<Balances />} />
          <Route path="/receipts" element={<Receipts />} />
          <Route path="/shipments" element={<Shipments />} />
          <Route path="/clients" element={<Clients />} />
          <Route path="/units" element={<Units />} />
          <Route path="/resources" element={<Resources />} />
        </Routes>
      </div>
    </div>
  );
};

export default App;