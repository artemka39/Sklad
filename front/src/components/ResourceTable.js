import React from "react";

export const ResourceTable = ({ items, resources, units, onChange, onRemove, onAddRow }) => {
  return (
    <table border="1" cellPadding="8" style={{ width: "100%", marginBottom: 10 }}>
      <thead>
        <tr>
          <th>
            <button type="button" onClick={onAddRow}>+</button>
          </th>
          <th>Ресурс</th>
          <th>Единица</th>
          <th>Количество</th>
        </tr>
      </thead>
      <tbody>
        {items.map((r, idx) => (
          <tr key={idx}>
            <td>
              <button onClick={() => onRemove(idx)}>-</button>
            </td>
            <td>
              <select
                value={r.resourceId}
                onChange={e => onChange(idx, "resourceId", Number(e.target.value))}
              >
                <option value="">Ресурс</option>
                {resources.map(res => (
                  <option key={res.id} value={res.id}>{res.name}</option>
                ))}
              </select>
            </td>
            <td>
              <select
                value={r.unitId}
                onChange={e => onChange(idx, "unitId", Number(e.target.value))}
              >
                <option value="">Единица</option>
                {units.map(u => (
                  <option key={u.id} value={u.id}>{u.name}</option>
                ))}
              </select>
            </td>
            <td>
              <input
                type="number"
                min="1"
                value={r.count}
                onChange={e => onChange(idx, "count", Number(e.target.value))}
              />
            </td>
          </tr>
        ))}
      </tbody>
    </table>
  );
};