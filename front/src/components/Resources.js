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
  const [selectedIds, setSelectedIds] = useState([]);

  const fetchResources = async () => {
    try {
      const response = await axios.get('https://localhost:7024/api/resources', {
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
      const response = await axios.post('https://localhost:7024/api/resources', { name: newResourceName });
      toast.success(response.data.message || 'Ресурс добавлен');
      setNewResourceName('');
      setIsModalOpen(false);
      fetchResources();
    } catch (error) {
      toast.error(error.response?.data?.message || 'Ошибка добавления ресурса');
    }
  };

  const handleDeleteSelected = async () => {
    if (selectedIds.length === 0) {
      toast.warn('Выберите хотя бы один ресурс для удаления');
      return;
    }
    try {
      const response = await axios.post('https://localhost:7024/api/resources/delete', selectedIds);
      toast.success(response.data.message || 'Ресурсы удалены');
      setSelectedIds([]);
      fetchResources();
    } catch (error) {
      toast.error(error.response?.data?.message || 'Ошибка удаления ресурсов');
    }
  };

  const handleArchiveSelected = async () => {
    if (selectedIds.length === 0) {
      toast.warn('Выберите хотя бы один ресурс для архивирования');
      return;
    }
    try {
      const resourcesToArchive = resources.filter(r => selectedIds.includes(r.id));
      const response = await axios.post('https://localhost:7024/api/resources/archive', resourcesToArchive);
      toast.success(response.data.message || 'Ресурсы архивированы');
      setSelectedIds([]);
      fetchResources();
    } catch (error) {
      toast.error(error.response?.data?.message || 'Ошибка архивирования ресурсов');
    }
  };

  const handleEdit = (id, name) => {
    setEditResourceId(id);
    setEditedName(name);
  };

  const handleSaveEdit = async () => {
    try {
      const response = await axios.put('https://localhost:7024/api/resources', {
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
                checked={resources.length > 0 && selectedIds.length === resources.length}
                onChange={e => {
                  if (e.target.checked) {
                    setSelectedIds(resources.map(r => r.id));
                  } else {
                    setSelectedIds([]);
                  }
                }}
              />
            </th>
            <th>Название</th>
            <th>Состояние</th>
          </tr>
        </thead>
        <tbody>
          {resources.map((res) => (
            <tr key={res.id}>
              <td>
                <input
                  type="checkbox"
                  checked={selectedIds.includes(res.id)}
                  onChange={e => {
                    if (e.target.checked) {
                      setSelectedIds([...selectedIds, res.id]);
                    } else {
                      setSelectedIds(selectedIds.filter(id => id !== res.id));
                    }
                  }}
                />
              </td>
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
            </tr>
          ))}
        </tbody>
      </table>

      <ToastContainer position="top-right" autoClose={3000} hideProgressBar />
    </div>
  );
};
