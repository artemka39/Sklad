import React, { useState, useEffect } from 'react';

export const Clients = () => {
  const [clients, setClients] = useState([]);
  const [name, setName] = useState('');
  const [address, setAddress] = useState('');

  const fetchClients = async () => {
    try {
      const response = await fetch('https://localhost:7024/api/catalog/clients');
      const data = await response.json();
      setClients(data);
    } catch (error) {
      console.error('Ошибка загрузки клиентов:', error);
    }
  };

  useEffect(() => {
    fetchClients();
  }, []);

  const handleAdd = async (e) => {
    e.preventDefault();
    if (!name.trim() || !address.trim()) return;

    try {
      const response = await fetch('https://localhost:7024/api/catalog/client', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json'
        },
        body: JSON.stringify({ name, address })
      });

      if (response.ok) {
        setName('');
        setAddress('');
        fetchClients();
      } else {
        console.error('Ошибка добавления клиента');
      }
    } catch (error) {
      console.error('Ошибка при отправке:', error);
    }
  };

  return (
    <div>
      <h2>Клиенты</h2>

      <form onSubmit={handleAdd} style={{ marginBottom: '20px' }}>
        <input
          type="text"
          value={name}
          onChange={(e) => setName(e.target.value)}
          placeholder="Имя"
        />
        <input
          type="text"
          value={address}
          onChange={(e) => setAddress(e.target.value)}
          placeholder="Адрес"
        />
        <button type="submit">Добавить</button>
      </form>

      <table border="1" cellPadding="8">
        <thead>
          <tr>
            <th>Имя</th>
            <th>Адрес</th>
          </tr>
        </thead>
        <tbody>
          {clients.map(client => (
            <tr key={client.id}>
              <td>{client.name}</td>
              <td>{client.address}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
};
