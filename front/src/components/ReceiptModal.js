import React from "react";
import { ResourceTable } from "./ResourceTable";

export const ReceiptModal = ({
  title,
  isOpen,
  items,
  resources,
  units,
  onChange,
  onRemove,
  onAddRow,
  onSave,
  onClose,
  onDelete,
}) => {
  if (!isOpen) return null;

  return (
    <div style={{
      position: "fixed", top: 0, left: 0,
      width: "100vw", height: "100vh",
      background: "rgba(0,0,0,0.3)",
      display: "flex", alignItems: "center", justifyContent: "center",
      zIndex: 1000
    }}>
      <div style={{ background: "#fff", padding: 30, borderRadius: 8, minWidth: 400 }}>
        <h3>{title}</h3>

        <ResourceTable
          items={items}
          resources={resources}
          units={units}
          onChange={onChange}
          onRemove={onRemove}
          onAddRow={onAddRow}
        />

        <div style={{ display: "flex", justifyContent: "space-between", marginTop: 20 }}>
          <button style={{ color: "white", background: "#1976d2" }} onClick={onSave}>
            Сохранить
          </button>
          {onDelete && (
            <button style={{ color: "white", background: "#d32f2f" }} onClick={onDelete}>
              Удалить
            </button>
          )}
          <button onClick={onClose}>Отмена</button>
        </div>
      </div>
    </div>
  );
};