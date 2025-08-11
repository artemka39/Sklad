import React, { useState, useEffect } from 'react';
import axios from 'axios';
import { toast, ToastContainer } from 'react-toastify';
import 'react-toastify/dist/ReactToastify.css';

export const Units = () => {
  const [units, setUnits] = useState([]);
  const [newUnitName, setNewUnitName] = useState('');
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [editId, setEditId] = useState(null);
  const [editName, setEditName] = useState('');
  const [filterState, setFilterState] = useState('');
  const [selectedIds, setSelectedIds] = useState([]);

  const fetchUnits = async () => {
    try {
      const response = await axios.get('https://localhost:7024/api/units', {
        params: filterState ? { state: filterState } : {}
      });
      setUnits(response.data);
    } catch {
      toast.error('Ошибка загрузки единиц измерения');
    }
  };

  useEffect(() => {
    fetchUnits();
  }, []);

  const handleAdd = async () => {
    if (!newUnitName.trim()) {
      toast.warn('Введите название единицы измерения');
      return;
    }
    try {
      const response = await axios.post('https://localhost:7024/api/units', { name: newUnitName });
      toast.success(response.data.message || 'Единица измерения добавлена');
      setNewUnitName('');
      setIsModalOpen(false);
      fetchUnits();
    } catch (error) {
      toast.error(error.response?.data?.message || 'Ошибка добавления единицы измерения');
    }
  };

  const handleDeleteSelected = async () => {
    if (selectedIds.length === 0) {
      toast.warn('Выберите хотя бы одну единицу для удаления');
      return;
    }
    try {
      const response = await axios.post('https://localhost:7024/api/units/delete', selectedIds);
      toast.success(response.data.message || 'Единицы измерения удалены');
      setSelectedIds([]);
      fetchUnits();
    } catch (error) {
      toast.error(error.response?.data?.message || 'Ошибка удаления единиц измерения');
    }
  };

  const handleArchiveSelected = async () => {
    if (selectedIds.length === 0) {
      toast.warn('Выберите хотя бы одну единицу для архивирования');
      return;
    }
    try {
      const unitsToArchive = units.filter(u => selectedIds.includes(u.id));
      const response = await axios.post('https://localhost:7024/api/units/archive', unitsToArchive);
      toast.success(response.data.message || 'Единицы измерения архивированы');
      setSelectedIds([]);
      fetchUnits();
    } catch (error) {
      toast.error(error.response?.data?.message || 'Ошибка архивирования единиц измерения');
    }
  };

  const handleEdit = (id, name) => {
    setEditId(id);
    setEditName(name);
  };

  const handleSaveEdit = async () => {
    if (!editName.trim()) {
      toast.warn('Название не может быть пустым');
      return;
    }
    try {
      const response = await axios.put('https://localhost:7024/api/units', {
        id: editId,
        name: editName
      });
      toast.success(response.data.message || 'Единица измерения обновлена');
      setEditId(null);
      setEditName('');
      fetchUnits();
    } catch (error) {
      toast.error(error.response?.data?.message || 'Ошибка обновления единицы измерения');
    }
  };

  const stateLabels = {
    0: 'В архиве',
    1: 'Активен'
  };

  return (
    <div>
      <h2>Единицы измерения</h2>
      <button onClick={() => setIsModalOpen(true)}>Добавить единицу измерения</button>

      {isModalOpen && (
        <div style={{ margin: '20px 0' }}>
          <input
            type="text"
            value={newUnitName}
            onChange={(e) => setNewUnitName(e.target.value)}
            placeholder="Название единицы измерения"
            autoFocus
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

      <button onClick={fetchUnits}>Применить фильтр</button>

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
                checked={units.length > 0 && selectedIds.length === units.length}
                onChange={e => {
                  if (e.target.checked) {
                    setSelectedIds(units.map(u => u.id));
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
          {units.map(unit => (
            <tr key={unit.id}>
              <td>
                <input
                  type="checkbox"
                  checked={selectedIds.includes(unit.id)}
                  onChange={e => {
                    if (e.target.checked) {
                      setSelectedIds([...selectedIds, unit.id]);
                    } else {
                      setSelectedIds(selectedIds.filter(id => id !== unit.id));
                    }
                  }}
                />
              </td>
              <td onDoubleClick={() => handleEdit(unit.id, unit.name)}>
                {editId === unit.id ? (
                  <input
                    type="text"
                    value={editName}
                    onChange={(e) => setEditName(e.target.value)}
                    onKeyDown={(e) => e.key === 'Enter' && handleSaveEdit()}
                    onBlur={handleSaveEdit}
                    autoFocus
                  />
                ) : (
                  unit.name
                )}
              </td>
              <td>
                {stateLabels[unit.state]}
              </td>
            </tr>
          ))}
        </tbody>
      </table>

      <ToastContainer position="top-right" autoClose={3000} hideProgressBar />
    </div>
  );
};
