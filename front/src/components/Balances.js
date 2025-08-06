import React, { useState, useEffect, useCallback } from 'react';

export const Balances = () => {
  const [balances, setBalances] = useState([]);
  const [resources, setResources] = useState([]);
  const [units, setUnits] = useState([]);

  const [selectedResourceId, setSelectedResourceId] = useState('');
  const [selectedUnitId, setSelectedUnitId] = useState('');

  const fetchResourcesAndUnits = async () => {
    try {
      const [resourcesRes, unitsRes] = await Promise.all([
        fetch('https://localhost:7024/api/catalog/resources'),
        fetch('https://localhost:7024/api/catalog/units')
      ]);

      setResources(await resourcesRes.json());
      setUnits(await unitsRes.json());
    } catch (error) {
      console.error('Ошибка загрузки справочников:', error);
    }
  };

  const fetchBalances = useCallback(async () => {
    try {
      const params = new URLSearchParams();
      if (selectedResourceId) params.append('resourceId', selectedResourceId);
      if (selectedUnitId) params.append('unitId', selectedUnitId);

      const response = await fetch(`https://localhost:7024/api/storage/balance?${params}`);
      const data = await response.json();
      setBalances(data);
    } catch (error) {
      console.error('Ошибка загрузки баланса:', error);
    }
  }, [selectedResourceId, selectedUnitId]);

  useEffect(() => {
    fetchResourcesAndUnits();
    fetchBalances();
  }, [fetchBalances]);

  useEffect(() => {
    fetchBalances();
  }, [fetchBalances]);

  return (
    <div>
      <h2>Остатки на складе</h2>

      <div style={{ marginBottom: '20px' }}>
        <label>
          Ресурс:
          <select value={selectedResourceId} onChange={e => setSelectedResourceId(e.target.value)}>
            <option value="">Все</option>
            {resources.map(r => (
              <option key={r.id} value={r.id}>{r.name}</option>
            ))}
          </select>
        </label>

        <label style={{ marginLeft: '20px' }}>
          Единица измерения:
          <select value={selectedUnitId} onChange={e => setSelectedUnitId(e.target.value)}>
            <option value="">Все</option>
            {units.map(u => (
              <option key={u.id} value={u.id}>{u.name}</option>
            ))}
          </select>
        </label>
      </div>

      <table border="1" cellPadding="8">
        <thead>
          <tr>
            <th>Ресурс</th>
            <th>Единица измерения</th>
            <th>Количество</th>
          </tr>
        </thead>
        <tbody>
          {balances.map((b, idx) => (
            <tr key={idx}>
              <td>{b.resource?.name}</td>
              <td>{b.unitOfMeasurement?.name}</td>
              <td>{b.count}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
};