export const ModalCatalogEntity = ({ title, children, onClose }) => (
  <div style={{
    position: 'fixed', top: 0, left: 0, width: '100vw', height: '100vh',
    background: 'rgba(0,0,0,0.3)', display: 'flex', alignItems: 'center', justifyContent: 'center', zIndex: 1000
  }}>
    <div style={{ background: '#fff', padding: 30, borderRadius: 8, minWidth: 300 }}>
      <h3>{title}</h3>
      {children}
      <div style={{ textAlign: 'right', marginTop: 10 }}>
        <button onClick={onClose}>Закрыть</button>
      </div>
    </div>
  </div>
);