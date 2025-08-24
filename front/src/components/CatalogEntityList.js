export const CatalogEntityList = ({ 
  title, 
  stateLabels, 
  onAddClick, 
  onRowDoubleClick, 
  items, 
  filterState, 
  onFilterChange, 
  onApplyFilter 
}) => (
  <div>
    <h2>{title}</h2>
    <button onClick={onAddClick}>Добавить</button>

    {stateLabels && (
      <div style={{ marginTop: 10, marginBottom: 10 }}>
        <select value={filterState} onChange={e => onFilterChange(e.target.value)}>
          <option value="">Все</option>
          <option value="1">Активные</option>
          <option value="0">Архив</option>
        </select>
      </div>
    )}

    <table border="1" cellPadding="8" style={{ marginTop: '20px' }}>
      <thead>
        <tr>
          <th>Имя</th>
          {stateLabels && <th>Состояние</th>}
        </tr>
      </thead>
      <tbody>
        {items.map(item => (
          <tr key={item.id} onDoubleClick={() => onRowDoubleClick(item)}>
            <td>{item.name}</td>
            {stateLabels && <td>{stateLabels[item.state]}</td>}
          </tr>
        ))}
      </tbody>
    </table>
  </div>
);