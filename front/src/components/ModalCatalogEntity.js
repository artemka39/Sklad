import React from 'react';
import { Modal, Button, Form } from 'react-bootstrap';

const fieldLabels = {
  name: 'Имя',
  address: 'Адрес',
};

export const ModalCatalogEntity = ({
  show,
  onClose,
  title,
  form,
  setForm,
  onSave,
  onDelete,
  onArchive,
}) => {
  const isEdit = !!onDelete || !!onArchive;

  return (
    <Modal show={show} onHide={onClose} centered>
      <Modal.Header closeButton>
        <Modal.Title>{title}</Modal.Title>
      </Modal.Header>

      <Modal.Body>
        {Object.keys(form).map((key) => {
          if (key === 'id' || key === 'state') return null;
          return (
            <Form.Group className="mb-3" key={key}>
              <Form.Label>{fieldLabels[key] || key}</Form.Label>
              <Form.Control
                value={form[key]}
                onChange={(e) => setForm({ ...form, [key]: e.target.value })}
                placeholder={`Введите ${fieldLabels[key] || key}`}
              />
            </Form.Group>
          );
        })}
      </Modal.Body>

      <Modal.Footer className="d-flex justify-content-between">
        {isEdit && (
          <div>
            {onDelete && <Button variant="danger" onClick={onDelete} className="me-2">Удалить</Button>}
            {onArchive && <Button variant="warning" onClick={onArchive}>В архив</Button>}
          </div>
        )}
        <Button variant="primary" onClick={onSave}>
          {isEdit ? 'Сохранить' : 'Добавить'}
        </Button>
      </Modal.Footer>
    </Modal>
  );
};