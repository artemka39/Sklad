import React, { useState, useEffect } from "react";
import Select from "react-select";

export const DocumentFilters = ({ filtersConfig, onFilterChange }) => {
  // filtersConfig: [{ name: "Resources", options: [...], key: "ResourceIds" }, ...]

  const [filterValues, setFilterValues] = useState(
    filtersConfig.reduce((acc, f) => {
      acc[f.key] = [];
      return acc;
    }, { FromDate: "", ToDate: "" })
  );

  useEffect(() => {
    const payload = {
      FromDate: filterValues.FromDate || null,
      ToDate: filterValues.ToDate || null
    };

    filtersConfig.forEach(f => {
      payload[f.key] = filterValues[f.key].map(v => v.value);
    });

    onFilterChange(payload);
  }, [filterValues]);

  const handleReset = () => {
    const reset = { FromDate: "", ToDate: "" };
    filtersConfig.forEach(f => (reset[f.key] = []));
    setFilterValues(reset);
  };

  return (
    <div>
      <div style={{ display: "flex", gap: 20, flexWrap: "wrap", marginBottom: 10 }}>
        <div>
          <label>Дата с: </label>
          <input
            type="date"
            value={filterValues.FromDate}
            onChange={e => setFilterValues({ ...filterValues, FromDate: e.target.value })}
          />
        </div>
        <div>
          <label>Дата по: </label>
          <input
            type="date"
            value={filterValues.ToDate}
            onChange={e => setFilterValues({ ...filterValues, ToDate: e.target.value })}
          />
        </div>
      </div>

      <div style={{ display: "flex", gap: 20, flexWrap: "wrap" }}>
        {filtersConfig.map(f => (
          <div key={f.key} style={{ minWidth: f.width || 150 }}>
            <label>{f.name}: </label>
            <Select
              placeholder="Выберите"
              options={f.options.map(o => ({ value: o.id ?? o.number, label: o.name ?? o.number }))}
              isMulti
              value={filterValues[f.key]}
              onChange={selected => setFilterValues({ ...filterValues, [f.key]: selected })}
            />
          </div>
        ))}
      </div>

      <button style={{ marginTop: 10 }} onClick={handleReset}>Сбросить фильтры</button>
    </div>
  );
};