import React, { useState, useEffect } from 'react';
import axios from 'axios';
import { toast, ToastContainer } from 'react-toastify';
import 'react-toastify/dist/ReactToastify.css';

export const Shipments = () => {
  const [documents, setDocuments] = useState([]);
  const [clients, setClients] = useState([]);
  const [resources, setResources] = useState([]);
  const [units, setUnits] = useState([]);
  const [selectedClientId, setSelectedClientId] = useState('');
  const [selectedResourceId, setSelectedResourceId] = useState('');
  const [selectedUnitId, setSelectedUnitId] = useState('');
  const [count, setCount] = useState('');
  const [documentResources, setDocumentResources] = useState([]);
  const [filter, setFilter] = useState({});

  const fetchShipments = async () => {
    try {
      const response = await axios.get('https://localhost:7024/api/shipments', { params: filter });
      setDocuments(response.data);
    } catch (error) {
      toast.error('Ошибка загрузки отгрузок');
    }
  };

  const fetchCatalogs = async () => {
    try {
      const [clientsRes, resourcesRes, unitsRes] = await Promise.all([
        axios.get('https://localhost:7024/api/clients'),
        axios.get('https://localhost:7024/api/resources'),
        axios.get('https://localhost:7024/api/units')
      ]);
      setClients(clientsRes.data.filter(c => c.state === 1));
      setResources(resourcesRes.data.filter(r => r.state === 1));
      setUnits(unitsRes.data.filter(u => u.state === 1));
    } catch (error) {
      toast.error('Ошибка загрузки справочников');
    }
  };

  useEffect(() => {
    fetchShipments();
    fetchCatalogs();
  }, []);

  const handleAddResource = (e) => {
    e.preventDefault();
    if (!selectedResourceId || !selectedUnitId || !count) return;
    const existing = documentResources.find(
      r => r.resourceId === selectedResourceId && r.unitId === selectedUnitId
    );
    if (existing) {
      toast.warn('Такая комбинация ресурса и единицы уже добавлена');
      return;
    }
    setDocumentResources([...documentResources, {
      resourceId: Number(selectedResourceId),
      unitId: Number(selectedUnitId),
      count: Number(count)
    }]);
    setSelectedResourceId('');
    setSelectedUnitId('');
    setCount('');
  };

  const handleCreateDocument = async () => {
    if (!selectedClientId || !documentResources.length) {
      toast.warn('Выберите клиента и добавьте хотя бы одну позицию');
      return;
    }
    try {
      const response = await axios.post('https://localhost:7024/api/shipments', {
        clientId: Number(selectedClientId),
        resources: documentResources
      });
      toast.success('Документ отгрузки создан');
      setSelectedClientId('');
      setDocumentResources([]);
      fetchShipments();
    } catch (error) {
      toast.error(error.response?.data?.message || 'Ошибка создания документа');
    }
  };

  const handleDeleteDocument = async (id) => {
    try {
      await axios.delete(`https://localhost:7024/api/shipments/${id}`);
      toast.success('Документ удалён');
      fetchShipments();
    } catch (error) {
      toast.error('Ошибка удаления документа');
    }
  };

  // ...можно добавить методы для Update, Sign, Withdraw
  return (
    <div>
      <h2>Документы отгрузки</h2>

      <div style={{ marginBottom: '20px' }}>
        <h4>Создать документ отгрузки</h4>
        <select value={selectedClientId} onChange={e => setSelectedClientId(e.target.value)}>
          <option value="">Выберите клиента</option>
          {clients.map(c => (
            <option key={c.id} value={c.id}>{c.name} ({c.address})</option>
          ))}
        </select>
        <form onSubmit={handleAddResource} style={{ display: 'inline-block', marginLeft: '20px' }}>
          <select value={selectedResourceId} onChange={e => setSelectedResourceId(e.target.value)}>
            <option value="">Ресурс</option>
            {resources.map(r => (
              <option key={r.id} value={r.id}>{r.name}</option>
            ))}
          </select>
          <select value={selectedUnitId} onChange={e => setSelectedUnitId(e.target.value)}>
            <option value="">Единица</option>
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
          <div style={{ marginTop: '10px' }}>
            <ul>
              {documentResources.map((r, idx) => {
                const resName = resources.find(x => x.id === r.resourceId)?.name;
                const unitName = units.find(x => x.id === r.unitId)?.name;
                return (
                  <li key={idx}>{resName} — {r.count} {unitName}</li>
                );
              })}
            </ul>
            <button onClick={handleCreateDocument}>Создать документ</button>
          </div>
        )}
      </div>

      <h3>Список документов отгрузки</h3>
      <table border="1" cellPadding="8">
        <thead>
          <tr>
            <th>Номер</th>
            <th>Дата</th>
            <th>Клиент</th>
            <th>Состояние</th>
            <th>Ресурсы</th>
            <th></th>
          </tr>
        </thead>
        <tbody>
          {documents.map(doc => (
            <tr key={doc.id}>
              <td>{doc.number}</td>
              <td>{new Date(doc.date).toLocaleDateString()}</td>
              <td>{doc.client?.name} ({doc.client?.address})</td>
              <td>{doc.state}</td>
              <td>
                <ul>
                  {(doc.shipmentResources || []).map(res => (
                    <li key={res.id}>
                      {res.resource?.name} — {res.count} {res.unit?.name}
                    </li>
                  ))}
                </ul>
              </td>
              <td>
                <button onClick={() => handleDeleteDocument(doc.id)}>Удалить</button>
                {/* Можно добавить кнопки для Sign/Withdraw/Update */}
              </td>
            </tr>
          ))}
        </tbody>
      </table>

      <ToastContainer position="top-right" autoClose={3000} hideProgressBar />
    </div>
  );
}