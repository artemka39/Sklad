import React, { useState, useEffect } from 'react';

export const Resources = () => {
  const [resources, setResources] = useState([]);
  const [newResource, setNewResource] = useState('');

  const fetchResources = async () => {
    try {
      const response = await fetch('https://localhost:7024/api/catalog/resources');
      const data = await response.json();
      setResources(data);
    } catch (error) {
      console.error('Ошибка загрузки ресурсов:', error);
    }
  };

  useEffect(() => {
    fetchResources();
  }, []);

  const handleAdd = async (e) => {
    e.preventDefault();
    if (!newResource.trim()) return;

    try {
      const response = await fetch('https://localhost:7024/api/catalog/resource', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json'
        },
        body: JSON.stringify({ name: newResource })
      });

      if (response.ok) {
        setNewResource('');
        fetchResources();
      } else {
        console.error('Ошибка добавления ресурса');
      }
    } catch (error) {
      console.error('Ошибка при отправке:', error);
    }
  };

  return (
    <div>
      <h2>Ресурсы</h2>

      <form onSubmit={handleAdd} style={{ marginBottom: '20px' }}>
        <input
          type="text"
          value={newResource}
          onChange={(e) => setNewResource(e.target.value)}
          placeholder="Название ресурса"
        />
        <button type="submit">Добавить</button>
      </form>

      <table border="1" cellPadding="8">
        <thead>
          <tr>
            <th>ID</th>
            <th>Название</th>
          </tr>
        </thead>
        <tbody>
          {resources.map(res => (
            <tr key={res.id}>
              <td>{res.id}</td>
              <td>{res.name}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
};