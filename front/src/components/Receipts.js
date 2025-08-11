import React, { useState, useEffect } from 'react';

export const Receipts = () => {
  const [receipts, setReceipts] = useState([]);
  const [resources, setResources] = useState([]);
  const [units, setUnits] = useState([]);

  const [selectedResourceId, setSelectedResourceId] = useState('');
  const [selectedUnitId, setSelectedUnitId] = useState('');
  const [count, setCount] = useState('');
  const [documentResources, setDocumentResources] = useState([]);

  const fetchReceipts = async () => {
    try {
      const response = await fetch('https://localhost:7024/api/receipts');
      const data = await response.json();
      setReceipts(data);
    } catch (error) {
      console.error('Ошибка загрузки поступлений:', error);
    }
  };

  const fetchResourcesAndUnits = async () => {
    try {
      const [resourcesRes, unitsRes] = await Promise.all([
        fetch('https://localhost:7024/api/resources'),
        fetch('https://localhost:7024/api/units')
      ]);

      const resourcesData = await resourcesRes.json();
      const unitsData = await unitsRes.json();

      setResources(resourcesData);
      setUnits(unitsData);
    } catch (error) {
      console.error('Ошибка загрузки справочников:', error);
    }
  };

  useEffect(() => {
    fetchReceipts();
    fetchResourcesAndUnits();
  }, []);

  const handleAddResource = (e) => {
    e.preventDefault();
    if (!selectedResourceId || !selectedUnitId || !count) return;

    const existing = documentResources.find(
      r => r.resourceId === selectedResourceId && r.unitOfMeasurementId === selectedUnitId
    );
    if (existing) {
      alert('Такая комбинация ресурса и единицы уже добавлена');
      return;
    }

    setDocumentResources([...documentResources, {
      resourceId: selectedResourceId,
      unitOfMeasurementId: selectedUnitId,
      count: Number(count)
    }]);

    setSelectedResourceId('');
    setSelectedUnitId('');
    setCount('');
  };

  const handleSubmitDocument = async () => {
    if (!documentResources.length) return;

    try {
      const response = await fetch('https://localhost:7024/api/receipts', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ resources: documentResources })
      });

      if (response.ok) {
        setDocumentResources([]);
        fetchReceipts();
      } else {
        console.error('Ошибка создания документа');
      }
    } catch (error) {
      console.error('Ошибка при отправке:', error);
    }
  };

  return (
    <div>
      <h2>Поступления</h2>

      <form onSubmit={handleAddResource} style={{ marginBottom: '10px' }}>
        <select value={selectedResourceId} onChange={e => setSelectedResourceId(e.target.value)}>
          <option value="">Выберите ресурс</option>
          {resources.map(r => (
            <option key={r.id} value={r.id}>{r.name}</option>
          ))}
        </select>

        <select value={selectedUnitId} onChange={e => setSelectedUnitId(e.target.value)}>
          <option value="">Выберите единицу</option>
          {units.map(u => (
            <option key={u.id} value={u.id}>{u.name}</option>
          ))}
        </select>

        <input
          type="number"
          min="1"
          value={count}
          onChange={e => setCount(e.target.value)}
          placeholder="Количество"
        />
        <button type="submit">Добавить позицию</button>
      </form>

      {documentResources.length > 0 && (
        <div style={{ marginBottom: '20px' }}>
          <h4>Добавленные ресурсы:</h4>
          <ul>
            {documentResources.map((r, idx) => {
              const resName = resources.find(x => x.id === r.resourceId)?.name;
              const unitName = units.find(x => x.id === r.unitOfMeasurementId)?.name;
              return (
                <li key={idx}>
                  {resName} — {r.count} {unitName}
                </li>
              );
            })}
          </ul>
          <button onClick={handleSubmitDocument}>Создать документ</button>
        </div>
      )}

      <h3>Список документов поступления</h3>
<table border="1" cellPadding="8">
  <thead>
    <tr>
      <th>Номер документа</th>
      <th>Дата</th>
      <th>Ресурс</th>
      <th>Количество</th>
      <th>Ед. измерения</th>
    </tr>
  </thead>
  <tbody>
    {receipts.flatMap(receipt =>
      (receipt.inboundResources || []).map(res => (
        <tr key={`${receipt.id}-${res.id}`}>
          <td>{receipt.number}</td>
          <td>{new Date(receipt.date).toLocaleDateString()}</td>
          <td>{res.resource?.name}</td>
          <td>{res.count}</td>
          <td>{res.unitOfMeasurement?.name}</td>
        </tr>
      ))
    )}
  </tbody>

</table>

    </div>
  );
};
