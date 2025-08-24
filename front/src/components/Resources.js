import React, { useState, useEffect } from 'react';
import { config } from '../config';
import { useCrud } from '../hooks/useCrud';
import { ModalCatalogEntity } from '../components/ModalCatalogEntity';
import { CatalogEntityList } from '../components/CatalogEntityList';
import { ToastContainer } from 'react-toastify';
import 'react-toastify/dist/ReactToastify.css';

export const Resources = () => {
  const { items, fetchItems, addItem, updateItem, deleteItem, archiveItem } =
    useCrud(`${config.apiBaseUrl}/resources`);

  const [isModalOpen, setIsModalOpen] = useState(false);
  const [isEditOpen, setIsEditOpen] = useState(false);
  const [form, setForm] = useState({ id: null, name: '', state: 1 });
  const [filterState, setFilterState] = useState('');

  const stateLabels = { 0: 'В архиве', 1: 'Активен' };

  useEffect(() => { fetchItems({ state: filterState }); }, [filterState]);
  useEffect(() => { document.title = "Sklad — Ресурсы"; }, []);

  const handleAdd = () => {
    addItem({ name: form.name });
    setIsModalOpen(false);
    setForm({ id: null, name: '', state: 1 });
  };

  const handleSaveEdit = () => {
    updateItem(form);
    setIsEditOpen(false);
    setForm({ id: null, name: '', state: 1 });
  };

  return (
    <div>
      <CatalogEntityList
        title="Ресурсы"
        stateLabels={stateLabels}
        onAddClick={() => setIsModalOpen(true)}
        onRowDoubleClick={(resource) => { setForm(resource); setIsEditOpen(true); }}
        items={items}
        filterState={filterState}
        onFilterChange={setFilterState}
        onApplyFilter={() => fetchItems({ state: filterState })}
      />

      {isModalOpen && (
        <ModalCatalogEntity title="Добавить ресурс" onClose={() => setIsModalOpen(false)}>
          <input value={form.name} onChange={e => setForm({ ...form, name: e.target.value })} placeholder="Имя" />
          <button onClick={handleAdd}>Сохранить</button>
        </ModalCatalogEntity>
      )}

      {isEditOpen && (
        <ModalCatalogEntity title="Редактировать ресурс" onClose={() => setIsEditOpen(false)}>
          <input value={form.name} onChange={e => setForm({ ...form, name: e.target.value })} />
          <button onClick={handleSaveEdit}>Сохранить</button>
          <button onClick={() => deleteItem(form.id)}>Удалить</button>
          <button onClick={() => archiveItem({ ...form, state: 0 })}>Архивировать</button>
        </ModalCatalogEntity>
      )}

      <ToastContainer position="top-right" autoClose={3000} hideProgressBar />
    </div>
  );
};