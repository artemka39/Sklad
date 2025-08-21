import React from "react";
import { ResourceTable } from "./ResourceTable";

export const ShipmentModal = ({
  title,
  isOpen,
  items,
  resources,
  units,
  clients,
  selectedClientId,
  onClientChange,
  onChange,
  onRemove,
  onAddRow,
  onSave,
  onClose,
  onDelete,
  onSign,
  onWithdraw,
  documentState
}) => {
  if (!isOpen) return null;

  return (
    <div style={{
      position: "fixed",
      top: 0, left: 0, width: "100vw", height: "100vh",
      background: "rgba(0,0,0,0.3)",
      display: "flex", alignItems: "center", justifyContent: "center",
      zIndex: 1000
    }}>
      <div style={{ background: "#fff", padding: 30, borderRadius: 8, minWidth: 500 }}>
        <h3>{title}</h3>

        <div style={{ marginBottom: 10 }}>
          <label>Клиент: </label>
          <select value={selectedClientId} onChange={e => onClientChange(Number(e.target.value))}>
            <option value="">Выберите клиента</option>
            {clients.map(c => (
              <option key={c.id} value={c.id}>{c.name}</option>
            ))}
          </select>
        </div>

        <ResourceTable
          items={items}
          resources={resources}
          units={units}
          onChange={onChange}
          onRemove={onRemove}
          onAddRow={onAddRow}
        />

        <div style={{ display: "flex", justifyContent: "space-between", marginTop: 20 }}>
          <div>
            <button style={{ marginRight: 10, background: "#1976d2", color: "#fff" }} onClick={onSave}>
              Сохранить
            </button>
            <button style={{ background: "#d32f2f", color: "#fff" }} onClick={onDelete}>
              Удалить
            </button>
          </div>
          <div>
            {documentState === 0 && (
              <button style={{ background: "#4caf50", color: "#fff" }} onClick={onSign}>
                Подписать
              </button>
            )}
            {documentState === 1 && (
              <button style={{ background: "#f57c00", color: "#fff" }} onClick={onWithdraw}>
                Отозвать
              </button>
            )}
            <button style={{ marginLeft: 10 }} onClick={onClose}>Отмена</button>
          </div>
        </div>
      </div>
    </div>
  );
};