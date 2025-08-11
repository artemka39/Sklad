import React, { useState, useEffect } from 'react';
import axios from 'axios';
import { toast, ToastContainer } from 'react-toastify';
import 'react-toastify/dist/ReactToastify.css';

export const Clients = () => {
  const [clients, setClients] = useState([]);
  const [newName, setNewName] = useState('');
  const [newAddress, setNewAddress] = useState('');
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [editId, setEditId] = useState(null);
  const [editName, setEditName] = useState('');
  const [editAddress, setEditAddress] = useState('');
  const [filterState, setFilterState] = useState('');
  const [selectedIds, setSelectedIds] = useState([]);

  const fetchClients = async () => {
    try {
      const response = await axios.get('https://localhost:7024/api/clients', {
        params: filterState ? { state: filterState } : {}
      });
      setClients(response.data);
    } catch {
      toast.error('Ошибка загрузки клиентов');
    }
  };

  useEffect(() => {
    fetchClients();
  }, []);

  const handleAdd = async () => {
    try {
      const response = await axios.post('https://localhost:7024/api/clients', {
        name: newName,
        address: newAddress
      });
      toast.success(response.data.message || 'Клиент добавлен');
      setNewName('');
      setNewAddress('');
      setIsModalOpen(false);
      fetchClients();
    } catch (error) {
      toast.error(error.response?.data?.message || 'Ошибка добавления клиента');
    }
  };

  const handleDeleteSelected = async () => {
    if (selectedIds.length === 0) {
      toast.warn('Выберите хотя бы одного клиента для удаления');
      return;
    }
    try {
      const response = await axios.post('https://localhost:7024/api/clients/delete', selectedIds);
      toast.success(response.data.message || 'Клиенты удалены');
      setSelectedIds([]);
      fetchClients();
    } catch (error) {
      toast.error(error.response?.data?.message || 'Ошибка удаления клиентов');
    }
  };

  const handleArchiveSelected = async () => {
    if (selectedIds.length === 0) {
      toast.warn('Выберите хотя бы одного клиента для архивирования');
      return;
    }
    try {
      const clientsToArchive = clients.filter(c => selectedIds.includes(c.id));
      const response = await axios.post('https://localhost:7024/api/clients/archive', clientsToArchive);
      toast.success(response.data.message || 'Клиенты архивированы');
      setSelectedIds([]);
      fetchClients();
    } catch (error) {
      toast.error(error.response?.data?.message || 'Ошибка архивирования клиентов');
    }
  };

  const handleEdit = (id, name, address) => {
    setEditId(id);
    setEditName(name);
    setEditAddress(address);
  };

  const handleSaveEdit = async () => {
    try {
      const response = await axios.put(`https://localhost:7024/api/clients`, {
        id: editId,
        name: editName,
        address: editAddress
      });
      toast.success(response.data.message || 'Клиент обновлён');
      setEditId(null);
      setEditName('');
      setEditAddress('');
      fetchClients();
    } catch (error) {
      toast.error(error.response?.data?.message || 'Ошибка обновления клиента');
    }
  };

  const stateLabels = {
    0: 'В архиве',
    1: 'Активен'
  };

  return (
    <div>
      <h2>Клиенты</h2>
      <button onClick={() => setIsModalOpen(true)}>Добавить клиента</button>

      {isModalOpen && (
        <div style={{ margin: '20px 0' }}>
          <input
            type="text"
            value={newName}
            onChange={(e) => setNewName(e.target.value)}
            placeholder="Имя"
          />
          <input
            type="text"
            value={newAddress}
            onChange={(e) => setNewAddress(e.target.value)}
            placeholder="Адрес"
          />
          <button onClick={handleAdd}>Сохранить</button>
          <button onClick={() => setIsModalOpen(false)}>Отмена</button>
        </div>
      )}

      <select value={filterState} onChange={(e) => { setFilterState(e.target.value); }}>
        <option value="">Все</option>
        <option value="Active">Активные</option>
        <option value="Archived">Архивированные</option>
      </select>

      <button onClick={fetchClients}>Применить фильтр</button>

      <div style={{ margin: '20px 0' }}>
        <button onClick={handleDeleteSelected}>Удалить выбранные</button>
        <button onClick={handleArchiveSelected} style={{ marginLeft: '10px' }}>Архивировать выбранные</button>
      </div>

      <table border="1" cellPadding="8" style={{ marginTop: '20px' }}>
        <thead>
          <tr>
            <th>
              <input
                type="checkbox"
                checked={clients.length > 0 && selectedIds.length === clients.length}
                onChange={e => {
                  if (e.target.checked) {
                    setSelectedIds(clients.map(c => c.id));
                  } else {
                    setSelectedIds([]);
                  }
                }}
              />
            </th>
            <th>Имя</th>
            <th>Адрес</th>
            <th>Состояние</th>
          </tr>
        </thead>
        <tbody>
          {clients.map(client => (
            <tr key={client.id}>
              <td>
                <input
                  type="checkbox"
                  checked={selectedIds.includes(client.id)}
                  onChange={e => {
                    if (e.target.checked) {
                      setSelectedIds([...selectedIds, client.id]);
                    } else {
                      setSelectedIds(selectedIds.filter(id => id !== client.id));
                    }
                  }}
                />
              </td>
              <td onDoubleClick={() => handleEdit(client.id, client.name, client.address)}>
                {editId === client.id ? (
                  <input
                    type="text"
                    value={editName}
                    onChange={(e) => setEditName(e.target.value)}
                    onKeyDown={(e) => e.key === 'Enter' && handleSaveEdit()}
                    onBlur={handleSaveEdit}
                    autoFocus
                  />
                ) : (
                  client.name
                )}
              </td>
              <td>
                {editId === client.id ? (
                  <input
                    type="text"
                    value={editAddress}
                    onChange={(e) => setEditAddress(e.target.value)}
                    onKeyDown={(e) => e.key === 'Enter' && handleSaveEdit()}
                    onBlur={handleSaveEdit}
                  />
                ) : (
                  client.address
                )}
              </td>
              <td>
                {stateLabels[client.state]}
              </td>
            </tr>
          ))}
        </tbody>
      </table>

      <ToastContainer position="top-right" autoClose={3000} hideProgressBar />
    </div>
  );
};
