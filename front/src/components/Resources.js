import React, { useState, useEffect } from 'react';
import axios from 'axios';
import { toast, ToastContainer } from 'react-toastify';
import 'react-toastify/dist/ReactToastify.css';

export const Resources = () => {
  const [resources, setResources] = useState([]);
  const [newName, setNewName] = useState('');
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [editId, setEditId] = useState(null);
  const [editName, setEditName] = useState('');
  const [filterState, setFilterState] = useState('');

  const handleDeleteResource = async (id) => {
    try {
      const response = await axios.delete(`https://localhost:7024/api/resources/${id}`);
      toast.success(response.data.message || 'Ресурс удален');
      setEditId(null);
      setEditName('');
      fetchResources();
    } catch (error) {
      toast.error(error.response?.data?.message || 'Ошибка удаления ресурса');
    }
  };

  const handleArchiveResource = async (resource) => {
    try {
      const response = await axios.post('https://localhost:7024/api/resources/archive', resource);
      toast.success(response.data.message || 'Ресурс архивирован');
      setEditId(null);
      setEditName('');
      fetchResources();
    } catch (error) {
      toast.error(error.response?.data?.message || 'Ошибка архивирования ресурса');
    }
  };

  const fetchResources = async () => {
    try {
      const response = await axios.get('https://localhost:7024/api/resources', {
        params: filterState ? { state: filterState } : {}
      });
      setResources(response.data);
    } catch {
      toast.error('Ошибка загрузки ресурсов');
    }
  };

  useEffect(() => {
    fetchResources();
  }, []);

  const handleAdd = async () => {
    try {
      const response = await axios.post('https://localhost:7024/api/resources', {
        name: newName
      });
      toast.success(response.data.message || 'Ресурс добавлен');
      setNewName('');
      setIsModalOpen(false);
      fetchResources();
    } catch (error) {
      toast.error(error.response?.data?.message || 'Ошибка добавления ресурса');
    }
  };

  const handleEdit = (id, name) => {
    setEditId(id);
    setEditName(name);
  };

  const handleSaveEdit = async () => {
    try {
      const resource = resources.find(c => c.id === editId);
      const response = await axios.put(`https://localhost:7024/api/resources`, {
        id: editId,
        name: editName,
        state: resource ? resource.state : 1
      });
      toast.success(response.data.message || 'Ресурс обновлён');
      setEditId(null);
      setEditName('');
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
        <div style={{
          position: 'fixed', top: 0, left: 0, width: '100vw', height: '100vh',
          background: 'rgba(0,0,0,0.3)', display: 'flex', alignItems: 'center', justifyContent: 'center', zIndex: 1000
        }}>
          <div style={{ background: '#fff', padding: 30, borderRadius: 8, minWidth: 300 }}>
            <h3>Добавить ресурс</h3>
            <input
              type="text"
              value={newName}
              onChange={(e) => setNewName(e.target.value)}
              placeholder="Имя"
              style={{ width: '100%', marginBottom: 10 }}
              autoFocus
            />
            <div style={{ textAlign: 'right' }}>
              <button onClick={handleAdd} style={{ marginRight: 10 }}>Сохранить</button>
              <button onClick={() => setIsModalOpen(false)}>Отмена</button>
            </div>
          </div>
        </div>
      )}

      {editId !== null && (
        <div style={{
          position: 'fixed', top: 0, left: 0, width: '100vw', height: '100vh',
          background: 'rgba(0,0,0,0.3)', display: 'flex', alignItems: 'center', justifyContent: 'center', zIndex: 1000
        }}>
          <div style={{ background: '#fff', padding: 30, borderRadius: 8, minWidth: 300 }}>
            <h3>Редактировать ресурс</h3>
            <input
              type="text"
              value={editName}
              onChange={(e) => setEditName(e.target.value)}
              placeholder="Имя"
              style={{ width: '100%', marginBottom: 10 }}
              autoFocus
            />
            <div style={{ display: 'flex', justifyContent: 'space-between', marginTop: 20 }}>
              <button onClick={() => handleDeleteResource(editId)} style={{ color: 'white', background: '#d32f2f' }}>Удалить</button>
              <button onClick={() => handleArchiveResource({ id: editId, name: editName, state: 0 })} style={{ color: 'white', background: '#ffa000' }}>Архивировать</button>
              <div>
                <button onClick={handleSaveEdit} style={{ marginRight: 10 }}>Сохранить</button>
                <button onClick={() => { setEditId(null); setEditName(''); }}>Отмена</button>
              </div>
            </div>
          </div>
        </div>
      )}

      <select value={filterState} onChange={(e) => { setFilterState(e.target.value); }}>
        <option value="">Все</option>
        <option value="Active">Активные</option>
        <option value="Archived">Архив</option>
      </select>

      <button onClick={fetchResources}>Применить фильтр</button>

      <table border="1" cellPadding="8" style={{ marginTop: '20px' }}>
        <thead>
          <tr>
            <th>Имя</th>
            <th>Состояние</th>
          </tr>
        </thead>
        <tbody>
          {resources.map(resource => (
            <tr key={resource.id} onDoubleClick={() => handleEdit(resource.id, resource.name)}>
              <td>
                {resource.name}
              </td>
              <td>
                {stateLabels[resource.state]}
              </td>
            </tr>
          ))}
        </tbody>
      </table>

      <ToastContainer position="top-right" autoClose={3000} hideProgressBar />
    </div>
  );
};