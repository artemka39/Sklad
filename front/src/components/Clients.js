import React, { useState, useEffect } from 'react';
import { config } from '../config';
import { useCrud } from '../hooks/useCrud';
import { CatalogEntityList } from '../components/CatalogEntityList';
import { ToastContainer } from 'react-toastify';
import 'react-toastify/dist/ReactToastify.css';
import { ModalCatalogEntity } from './ModalCatalogEntity';

export const Clients = () => {
  const { items, fetchItems, addItem, updateItem, deleteItem, archiveItem } =
    useCrud(`${config.apiBaseUrl}/clients`);

  const [modalMode, setModalMode] = useState(null);
  const [form, setForm] = useState({ id: null, name: '', address: '', state: 1 });
  const [filterState, setFilterState] = useState('');

  const stateLabels = { 0: 'В архиве', 1: 'Активен' };

  useEffect(() => {
    fetchItems({ state: filterState });
  }, [filterState]);

  const handleAdd = async () => {
    try {
      await addItem({ name: form.name, address: form.address });
      setModalMode(null);
      setForm({ id: null, name: '', address: '', state: 1 });
      fetchItems({ state: filterState });
    } catch {}
  };

  const handleSaveEdit = async () => {
    try {
      await updateItem(form);
      setModalMode(null);
      fetchItems({ state: filterState });
    } catch {}
  };

  const handleDelete = async () => {
    try {
      await deleteItem(form.id);
      setModalMode(null);
      fetchItems({ state: filterState });
    } catch {}
  };

  const handleArchive = async () => {
    try {
      await archiveItem({ ...form, state: 0 });
      setModalMode(null);
      fetchItems({ state: filterState });
    } catch {}
  };

  return (
    <div>
      <CatalogEntityList
        title="Клиенты"
        stateLabels={stateLabels}
        onAddClick={() => {
          setForm({ id: null, name: '', address: '', state: 1 });
          setModalMode("add");
        }}
        onRowDoubleClick={(client) => {
          setForm(client);
          setModalMode("edit");
        }}
        items={items}
        filterState={filterState}
        onFilterChange={setFilterState}
      />

      {modalMode && (
        <ModalCatalogEntity
          show={!!modalMode}
          onClose={() => setModalMode(null)}
          title={modalMode === "add" ? "Добавить клиента" : "Редактировать клиента"}
          form={form}
          setForm={setForm}
          onSave={modalMode === "add" ? handleAdd : handleSaveEdit}
          onDelete={modalMode === "edit" ? handleDelete : null}
          onArchive={modalMode === "edit" ? handleArchive : null}
        />
      )}

      <ToastContainer position="top-right" autoClose={3000} hideProgressBar />
    </div>
  );
};