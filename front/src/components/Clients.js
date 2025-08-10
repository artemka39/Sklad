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

  const fetchClients = async () => {
    try {
      const response = await axios.get('https://localhost:7024/api/catalog/clients', {
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
      const response = await axios.post('https://localhost:7024/api/catalog/client', {
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

  const handleDelete = async (id) => {
    try {
      const response = await axios.delete(`https://localhost:7024/api/catalog/client/${id}`);
      toast.success(response.data.message || 'Клиент удалён');
      fetchClients();
    } catch (error) {
      toast.error(error.response?.data?.message || 'Ошибка удаления клиента');
    }
  };

  const handleArchive = async (client) => {
    try {
      const response = await axios.patch(`https://localhost:7024/api/catalog/client`, client);
      toast.success(response.data.message || 'Клиент архивирован');
      fetchClients();
    } catch (error) {
      toast.error(error.response?.data?.message || 'Ошибка архивирования клиента');
    }
  };

  const handleEdit = (id, name, address) => {
    setEditId(id);
    setEditName(name);
    setEditAddress(address);
  };

  const handleSaveEdit = async () => {
    try {
      const response = await axios.put(`https://localhost:7024/api/catalog/client`, {
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
      <table border="1" cellPadding="8" style={{ marginTop: '20px' }}>
        <thead>
          <tr>
            <th>Имя</th>
            <th>Адрес</th>
            <th>Состояние</th>
            <th>Действия</th>
          </tr>
        </thead>
        <tbody>
          {clients.map(client => (
            <tr key={client.id}>
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
              <td>
                <button onClick={() => handleDelete(client.id)}>Удалить</button>
                <button onClick={() => handleArchive(client)}>Архивировать</button>
              </td>
            </tr>
          ))}
        </tbody>
      </table>

      <ToastContainer position="top-right" autoClose={3000} hideProgressBar />
    </div>
  );
};
