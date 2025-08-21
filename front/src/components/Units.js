import React, { useState, useEffect } from 'react';
import axios from 'axios';
import { toast, ToastContainer } from 'react-toastify';
import 'react-toastify/dist/ReactToastify.css';

export const Units = () => {
  const [units, setUnits] = useState([]);
  const [newName, setNewName] = useState('');
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [editId, setEditId] = useState(null);
  const [editName, setEditName] = useState('');
  const [filterState, setFilterState] = useState('');

  const handleDeleteUnit = async (id) => {
    try {
      const response = await axios.delete(`https://localhost:7024/api/units/${id}`);
      toast.success(response.data.message || 'Единица измерения удалена');
      setEditId(null);
      setEditName('');
      fetchUnits();
    } catch (error) {
      toast.error(error.response?.data?.message || 'Ошибка удаления единицы измерения');
    }
  };

  const handleArchiveUnit = async (unit) => {
    try {
      const response = await axios.post('https://localhost:7024/api/units/archive', unit);
      toast.success(response.data.message || 'Единица измерения архивирован');
      setEditId(null);
      setEditName('');
      fetchUnits();
    } catch (error) {
      toast.error(error.response?.data?.message || 'Ошибка архивирования единицы измерения');
    }
  };

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
    try {
      const response = await axios.post('https://localhost:7024/api/units', {
        name: newName
      });
      toast.success(response.data.message || 'Единица измерения добавлена');
      setNewName('');
      setIsModalOpen(false);
      fetchUnits();
    } catch (error) {
      toast.error(error.response?.data?.message || 'Ошибка добавления единицы измерения');
    }
  };

  const handleEdit = (id, name) => {
    setEditId(id);
    setEditName(name);
  };

  const handleSaveEdit = async () => {
    try {
      const unit = units.find(c => c.id === editId);
      const response = await axios.put(`https://localhost:7024/api/units`, {
        id: editId,
        name: editName,
        state: unit ? unit.state : 1
      });
      toast.success(response.data.message || 'Единица измерения обновлён');
      setEditId(null);
      setEditName('');
      fetchUnits();
    } catch (error) {
      toast.error(error.response?.data?.message || 'Ошибка обновления единицы измерения');
    }
  };

  const stateLabels = {
    0: 'В архиве',
    1: 'Активна'
  };

  return (
    <div>
      <h2>Единицы измерения</h2>
      <button onClick={() => setIsModalOpen(true)}>Добавить единицу измерения</button>

      {isModalOpen && (
        <div style={{
          position: 'fixed', top: 0, left: 0, width: '100vw', height: '100vh',
          background: 'rgba(0,0,0,0.3)', display: 'flex', alignItems: 'center', justifyContent: 'center', zIndex: 1000
        }}>
          <div style={{ background: '#fff', padding: 30, borderRadius: 8, minWidth: 300 }}>
            <h3>Добавить единицу измерения</h3>
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
            <h3>Редактировать единицу измерения</h3>
            <input
              type="text"
              value={editName}
              onChange={(e) => setEditName(e.target.value)}
              placeholder="Имя"
              style={{ width: '100%', marginBottom: 10 }}
              autoFocus
            />
            <div style={{ display: 'flex', justifyContent: 'space-between', marginTop: 20 }}>
              <button onClick={() => handleDeleteUnit(editId)} style={{ color: 'white', background: '#d32f2f' }}>Удалить</button>
              <button onClick={() => handleArchiveUnit({ id: editId, name: editName, state: 0 })} style={{ color: 'white', background: '#ffa000' }}>Архивировать</button>
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

      <button onClick={fetchUnits}>Применить фильтр</button>

      <table border="1" cellPadding="8" style={{ marginTop: '20px' }}>
        <thead>
          <tr>
            <th>Имя</th>
            <th>Состояние</th>
          </tr>
        </thead>
        <tbody>
          {units.map(unit => (
            <tr key={unit.id} onDoubleClick={() => handleEdit(unit.id, unit.name)}>
              <td>
                {unit.name}
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