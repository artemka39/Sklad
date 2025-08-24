import React, { useState, useEffect } from 'react';
import { config } from '../config';
import { useCrud } from '../hooks/useCrud';
import { ModalCatalogEntity } from '../components/ModalCatalogEntity';
import { CatalogEntityList } from '../components/CatalogEntityList';
import { ToastContainer } from 'react-toastify';
import 'react-toastify/dist/ReactToastify.css';

export const Clients = () => {
  const { items, fetchItems, addItem, updateItem, deleteItem, archiveItem } =
    useCrud(`${config.apiBaseUrl}/clients`);

  const [isModalOpen, setIsModalOpen] = useState(false);
  const [isEditOpen, setIsEditOpen] = useState(false);
  const [form, setForm] = useState({ id: null, name: '', address: '', state: 1 });
  const [filterState, setFilterState] = useState('');

  const stateLabels = { 0: 'В архиве', 1: 'Активен' };

  useEffect(() => { fetchItems({ state: filterState }); }, [filterState]);

  useEffect(() => {
    document.title = "Sklad — Клиенты";
  }, []);

  const handleAdd = () => {
    addItem({ name: form.name, address: form.address });
    setIsModalOpen(false);
    setForm({ id: null, name: '', address: '', state: 1 });
  };

  const handleSaveEdit = () => {
    updateItem(form);
    setIsEditOpen(false);
    setForm({ id: null, name: '', address: '', state: 1 });
  };

  return (
    <div>
      <CatalogEntityList
        title="Клиенты"
        stateLabels={stateLabels}
        onAddClick={() => setIsModalOpen(true)}
        onRowDoubleClick={(client) => { setForm(client); setIsEditOpen(true); }}
        items={items}
        filterState={filterState}
        onFilterChange={setFilterState}
        onApplyFilter={() => fetchItems({ state: filterState })}
      />

      {isModalOpen && (
        <ModalCatalogEntity title="Добавить клиента" onClose={() => setIsModalOpen(false)}>
          <input value={form.name} onChange={e => setForm({ ...form, name: e.target.value })} placeholder="Имя" />
          <input value={form.address} onChange={e => setForm({ ...form, address: e.target.value })} placeholder="Адрес" />
          <button onClick={handleAdd}>Сохранить</button>
        </ModalCatalogEntity>
      )}

      {isEditOpen && (
        <ModalCatalogEntity title="Редактировать клиента" onClose={() => setIsEditOpen(false)}>
          <input value={form.name} onChange={e => setForm({ ...form, name: e.target.value })} />
          <input value={form.address} onChange={e => setForm({ ...form, address: e.target.value })} />
          <button onClick={handleSaveEdit}>Сохранить</button>
          <button onClick={() => deleteItem(form.id)}>Удалить</button>
          <button onClick={() => archiveItem({ ...form, state: 0 })}>Архивировать</button>
        </ModalCatalogEntity>
      )}

      <ToastContainer position="top-right" autoClose={3000} hideProgressBar />
    </div>
  );
};