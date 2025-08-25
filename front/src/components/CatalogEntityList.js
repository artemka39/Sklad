import React from 'react';
import { Table, Button, Form, Row, Col } from 'react-bootstrap';

export const CatalogEntityList = ({
  title,
  stateLabels,
  onAddClick,
  onRowDoubleClick,
  items,
  filterState,
  onFilterChange,
}) => {
  return (
    <div className="mb-4">
      <Row className="align-items-center mb-3">
        <Col>
          <h2>{title}</h2>
        </Col>
      </Row>

      {stateLabels && (
        <Row className="mb-3 g-2">
          <Col md="auto">
            <Button variant="primary" onClick={onAddClick}>
              +
            </Button>
          </Col>
          <Col md="auto">
            <Form.Select
              value={filterState}
              onChange={(e) => onFilterChange(e.target.value)}
            >
              <option value="">Все</option>
              <option value="1">Активные</option>
              <option value="0">Архив</option>
            </Form.Select>
          </Col>
        </Row>
      )}

      <Table striped bordered hover responsive style={{ width: '300px' }}>
        <thead>
          <tr>
            <th>Имя</th>
            {items.some(item => item.address !== undefined) && <th>Адрес</th>}
            {stateLabels && <th style={{ width: '100px' }}>Состояние</th>}
          </tr>
        </thead>
        <tbody>
          {items.map((item) => (
            <tr
              key={item.id}
              onDoubleClick={() => onRowDoubleClick(item)}
              style={{ cursor: 'pointer' }}
            >
              <td>{item.name}</td>
              {item.address !== undefined && <td>{item.address}</td>}
              {stateLabels && <td>{stateLabels[item.state]}</td>}
            </tr>
          ))}
        </tbody>
      </Table>
    </div>
  );
};