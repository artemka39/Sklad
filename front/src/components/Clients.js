import React, { useState, useEffect } from 'react';
import { useCrud } from '../hooks/useCrud';
import { ModalCatalogEntity } from '../components/ModalCatalogEntity';
import { CatalogEntityList } from '../components/CatalogEntityList';
import { ToastContainer } from 'react-toastify';
import 'react-toastify/dist/ReactToastify.css';

export const Clients = () => {
  const { items, fetchItems, addItem, updateItem, deleteItem, archiveItem } =
    useCrud('https://localhost:7024/api/clients');

  const [isModalCatalogEntityOpen, setIsModalCatalogEntityOpen] = useState(false);
  const [isEditOpen, setIsEditOpen] = useState(false);
  const [form, setForm] = useState({ id: null, name: '', address: '', state: 1 });

  const stateLabels = { 0: 'В архиве', 1: 'Активен' };

  useEffect(() => { fetchItems(); }, []);

  const handleAdd = () => {
    addItem({ name: form.name, address: form.address });
    setIsModalCatalogEntityOpen(false);
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
        onAddClick={() => setIsModalCatalogEntityOpen(true)}
        onRowDoubleClick={(client) => { setForm(client); setIsEditOpen(true); }}
        items={items}
      />

      {isModalCatalogEntityOpen && (
        <ModalCatalogEntity title="Добавить клиента" onClose={() => setIsModalCatalogEntityOpen(false)}>
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
