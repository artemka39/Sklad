export const CatalogEntityList = ({ title, stateLabels, onAddClick, onRowDoubleClick, items }) => (
  <div>
    <h2>{title}</h2>
    <button onClick={onAddClick}>Добавить</button>
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