import React from 'react';
import { Link } from 'react-router-dom';

export const Sidebar = () => {
  return (
    <div style={{ width: '200px', background: '#f0f0f0', padding: '10px' }}>
      <h3>Склад</h3>
      <ul style={{ listStyle: 'none', paddingLeft: 0 }}>
        <li><Link to="/balances">Баланс</Link></li>
        <li><Link to="/receipts">Поступления</Link></li>
        <li><Link to="/shipments">Отгрузки</Link></li>
      </ul>
      <h3>Справочники</h3>
      <ul style={{ listStyle: 'none', paddingLeft: 0 }}>
        <li><Link to="/clients">Клиенты</Link></li>
        <li><Link to="/units">Единицы измерения</Link></li>
        <li><Link to="/resources">Ресурсы</Link></li>
      </ul>
    </div>
  );
};