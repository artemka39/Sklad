import React, { useState, useEffect } from 'react';

export const Units = () => {
  const [units, setUnits] = useState([]);
  const [newUnit, setNewUnit] = useState('');

  const fetchUnits = async () => {
    try {
      const response = await fetch('https://localhost:7024/api/catalog/units');
      const data = await response.json();
      setUnits(data);
    } catch (error) {
      console.error('Ошибка загрузки единиц измерения:', error);
    }
  };

  useEffect(() => {
    fetchUnits();
  }, []);

  const handleAdd = async (e) => {
    e.preventDefault();
    if (!newUnit.trim()) return;

    try {
      const response = await fetch('https://localhost:7024/api/catalog/unit', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json'
        },
        body: JSON.stringify({ name: newUnit })
      });

      if (response.ok) {
        setNewUnit('');
        fetchUnits();
      } else {
        console.error('Ошибка добавления единицы измерения');
      }
    } catch (error) {
      console.error('Ошибка при отправке:', error);
    }
  };

  return (
    <div>
      <h2>Единицы измерения</h2>

      <form onSubmit={handleAdd} style={{ marginBottom: '20px' }}>
        <input
          type="text"
          value={newUnit}
          onChange={(e) => setNewUnit(e.target.value)}
          placeholder="Название единицы измерения"
        />
        <button type="submit">Добавить</button>
      </form>

      <table border="1" cellPadding="8">
        <thead>
          <tr>
            <th>Название</th>
          </tr>
        </thead>
        <tbody>
          {units.map(unit => (
            <tr key={unit.id}>
              <td>{unit.name}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
};