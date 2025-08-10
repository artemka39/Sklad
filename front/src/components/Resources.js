import React, { useState, useEffect } from 'react';
import axios from 'axios';
import { ToastContainer, toast } from 'react-toastify';
import 'react-toastify/dist/ReactToastify.css';

export const Resources = () => {
  const [resources, setResources] = useState([]);
  const [newResourceName, setNewResourceName] = useState('');
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [editResourceId, setEditResourceId] = useState(null);
  const [editedName, setEditedName] = useState('');
  const [filterState, setFilterState] = useState('');

  const fetchResources = async () => {
    try {
      const response = await axios.get('https://localhost:7024/api/catalog/resources', {
        params: filterState ? { state: filterState } : {}
      });
      setResources(response.data);
    } catch (error) {
      toast.error('Ошибка загрузки ресурсов');
    }
  };

  useEffect(() => {
    fetchResources();
  }, []);

  const handleAddResource = async () => {
    try {
      const response = await axios.post('https://localhost:7024/api/catalog/resource', { name: newResourceName });
      toast.success(response.data.message || 'Ресурс добавлен');
      setNewResourceName('');
      setIsModalOpen(false);
      fetchResources();
    } catch (error) {
      toast.error(error.response?.data?.message || 'Ошибка добавления ресурса');
    }
  };

  const handleDelete = async (id) => {
    try {
      const response = await axios.delete(`https://localhost:7024/api/catalog/resource/${id}`);
      toast.success(response.data.message || 'Ресурс удалён');
      fetchResources();
    } catch (error) {
      toast.error(error.response?.data?.message || 'Ошибка удаления ресурса');
    }
  };

  const handleArchive = async (resource) => {
    try {
      const response = await axios.patch(`https://localhost:7024/api/catalog/resource`, resource);
      toast.success(response.data.message || 'Ресурс архивирован');
      fetchResources();
    } catch (error) {
      toast.error(error.response?.data?.message || 'Ошибка архивирования ресурса');
    }
  };

  const handleEdit = (id, name) => {
    setEditResourceId(id);
    setEditedName(name);
  };

  const handleSaveEdit = async () => {
    try {
      const response = await axios.put('https://localhost:7024/api/catalog/resource', {
        id: editResourceId,
        name: editedName
      });
      toast.success(response.data.message || 'Ресурс обновлён');
      setEditResourceId(null);
      setEditedName('');
      fetchResources();
    } catch (error) {
      toast.error(error.response?.data?.message || 'Ошибка обновления ресурса');
    }
  };

  const stateLabels = {
    0: 'В архиве',
    1: 'Активен'
  };

  return (
    <div>
      <h2>Ресурсы</h2>
      <button onClick={() => setIsModalOpen(true)}>Добавить ресурс</button>

      {isModalOpen && (
        <div style={{ margin: '20px 0' }}>
          <input
            type="text"
            value={newResourceName}
            onChange={(e) => setNewResourceName(e.target.value)}
            placeholder="Название ресурса"
          />
          <button onClick={handleAddResource}>Сохранить</button>
          <button onClick={() => setIsModalOpen(false)}>Отмена</button>
        </div>
      )}

      <select value={filterState} onChange={(e) => { setFilterState(e.target.value); }}>
        <option value="">Все</option>
        <option value="Active">Активные</option>
        <option value="Archived">Архивированные</option>
      </select>

      <button onClick={fetchResources}>Применить фильтр</button>
      <table border="1" cellPadding="8" style={{ marginTop: '20px' }}>
        <thead>
          <tr>
            <th>Название</th>
            <th>Состояние</th>
            <th>Действия</th>
          </tr>
        </thead>
        <tbody>
          {resources.map((res) => (
            <tr key={res.id}>
              <td onDoubleClick={() => handleEdit(res.id, res.name)}>
                {editResourceId === res.id ? (
                  <input
                    type="text"
                    value={editedName}
                    onChange={(e) => setEditedName(e.target.value)}
                    onBlur={handleSaveEdit}
                    onKeyDown={(e) => e.key === 'Enter' && handleSaveEdit()}
                    autoFocus
                  />
                ) : (
                  res.name
                )}
              </td>
              <td>
                {stateLabels[res.state]}
              </td>
              <td>
                <button onClick={() => handleDelete(res.id)}>Удалить</button>
                <button onClick={() => handleArchive(res)}>Архивировать</button>
              </td>
            </tr>
          ))}
        </tbody>
      </table>

      <ToastContainer position="top-right" autoClose={3000} hideProgressBar />
    </div>
  );
};
