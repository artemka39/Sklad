import React, { useState, useEffect } from 'react';
import { config } from '../config';
import { useCrud } from '../hooks/useCrud';
import { CatalogEntityList } from '../components/CatalogEntityList';
import { ModalCatalogEntity } from './ModalCatalogEntity';
import { ToastContainer } from 'react-toastify';
import 'react-toastify/dist/ReactToastify.css';

export const Units = () => {
  const { items, fetchItems, addItem, updateItem, deleteItem, archiveItem } =
    useCrud(`${config.apiBaseUrl}/units`);

  const [modalMode, setModalMode] = useState(null);
  const [form, setForm] = useState({ id: null, name: '', state: 1 });
  const [filterState, setFilterState] = useState('');

  const stateLabels = { 0: 'В архиве', 1: 'Активна' };

  useEffect(() => {
    fetchItems({ state: filterState });
    document.title = "Sklad — Единицы измерения";
  }, [filterState]);

  const handleAdd = async () => {
    try {
      await addItem({ name: form.name });
      setModalMode(null);
      setForm({ id: null, name: '', state: 1 });
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
        title="Единицы измерения"
        stateLabels={stateLabels}
        items={items}
        filterState={filterState}
        onFilterChange={setFilterState}
        onAddClick={() => {
          setForm({ id: null, name: '', state: 1 });
          setModalMode("add");
        }}
        onRowDoubleClick={(unit) => {
          setForm(unit);
          setModalMode("edit");
        }}
      />

      {modalMode && (
        <ModalCatalogEntity
          show={!!modalMode}
          onClose={() => setModalMode(null)}
          title={modalMode === "add" ? "Добавить единицу измерения" : "Редактировать единицу измерения"}
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