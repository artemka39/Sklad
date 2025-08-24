import React, { useState, useEffect } from 'react';
import { config } from '../config';
import axios from 'axios';
import { toast, ToastContainer } from 'react-toastify';
import 'react-toastify/dist/ReactToastify.css';
import { ShipmentModal } from './ShipmentModal';
import { DocumentFilters } from './DocumentFilters';
import { DocumentStateEnum } from './DocumentStateEnum.js';

export const Shipments = () => {
  const [documents, setDocuments] = useState([]);
  const [clients, setClients] = useState([]);
  const [resources, setResources] = useState([]);
  const [units, setUnits] = useState([]);

  const [filter, setFilter] = useState({});
  const [isCreateModalOpen, setIsCreateModalOpen] = useState(false);
  const [documentResources, setDocumentResources] = useState([]);
  const [selectedClientId, setSelectedClientId] = useState('');

  const [editDocId, setEditDocId] = useState(null);
  const [editDocResources, setEditDocResources] = useState([]);
  const [editClientId, setEditClientId] = useState('');
  const [editDocumentState, setEditDocumentState] = useState(0);

  const fetchShipments = async () => {
    try {
      const res = await axios.get(`${config.apiBaseUrl}/shipments`, { params: filter });
      setDocuments(res.data);
    } catch {
      toast.error('Ошибка загрузки отгрузок');
    }
  };

  const fetchCatalogs = async () => {
    try {
      const [clientsRes, resourcesRes, unitsRes] = await Promise.all([
        axios.get(`${config.apiBaseUrl}/clients`),
        axios.get(`${config.apiBaseUrl}/resources`),
        axios.get(`${config.apiBaseUrl}/units`)
      ]);
      setClients(clientsRes.data.filter(c => c.state === 1));
      setResources(resourcesRes.data.filter(r => r.state === 1));
      setUnits(unitsRes.data.filter(u => u.state === 1));
    } catch {
      toast.error('Ошибка загрузки справочников');
    }
  };

  useEffect(() => {
    fetchShipments();
    fetchCatalogs();
  }, []);

  useEffect(() => {
    document.title = "Sklad — Отгрузки";
  }, []);

  const openCreateModal = () => {
    setSelectedClientId('');
    setDocumentResources([]);
    setIsCreateModalOpen(true);
  };

  const openEditModal = (doc) => {
    setEditDocId(doc.id);
    setEditClientId(doc.client?.id || '');
    setEditDocumentState(doc.state);
    setEditDocResources((doc.shipmentResources || []).map(r => ({
      id: r.id,
      resourceId: r.resource?.id,
      unitId: r.unit?.id,
      count: r.count
    })));
  };

  const handleCreateDocument = async () => {
    if (!selectedClientId || !documentResources.length) {
      toast.warn('Выберите клиента и добавьте хотя бы одну позицию');
      return;
    }
    try {
      await axios.post(`${config.apiBaseUrl}/shipments`, {
        clientId: Number(selectedClientId),
        resources: documentResources
      });
      toast.success('Документ создан');
      setIsCreateModalOpen(false);
      fetchShipments();
    } catch (error) {
      toast.error(error.response?.data?.message || 'Ошибка создания документа');
    }
  };

  const handleUpdateDocument = async () => {
    try {
      await axios.put(`${config.apiBaseUrl}/shipments`, {
        documentId: editDocId,
        clientId: editClientId,
        resources: editDocResources
      });
      toast.success('Документ обновлён');
      setEditDocId(null);
      fetchShipments();
    } catch (error) {
      toast.error(error.response?.data?.message || 'Ошибка обновления');
    }
  };

  const handleDeleteDocument = async (id) => {
    try {
      await axios.delete(`${config.apiBaseUrl}/shipments/${id}`);
      toast.success('Документ удалён');
      fetchShipments();
    } catch {
      toast.error('Ошибка удаления');
    }
  };

  const handleSignDocument = async (id) => {
    try {
      await axios.patch(`${config.apiBaseUrl}/shipments/sing/${id}`);
      toast.success('Документ подписан');
      fetchShipments();
    } catch {
      toast.error('Ошибка подписания');
    }
  };

  const handleWithdrawDocument = async (id) => {
    try {
      await axios.patch(`${config.apiBaseUrl}/shipments/withdraw/${id}`);
      toast.success('Документ отозван');
      fetchShipments();
    } catch {
      toast.error('Ошибка отзыва');
    }
  };

  return (
    <div>
      <h2>Документы отгрузки</h2>
      <button onClick={openCreateModal} style={{ marginBottom: 20 }}>Создать документ</button>
      <DocumentFilters
        filtersConfig={[
          { name: "Клиенты", options: clients, key: "ClientIds" },
          { name: "Ресурсы", options: resources, key: "ResourceIds" },
          { name: "Единицы", options: units, key: "UnitIds" },
          { name: "Номера документов", options: documents, key: "DocumentNumbers" }
        ]}
        onFilterChange={setFilter}
      />
      
      <ShipmentModal
        title="Создать документ отгрузки"
        isOpen={isCreateModalOpen}
        clients={clients}
        selectedClientId={selectedClientId}
        onClientChange={setSelectedClientId}
        items={documentResources}
        resources={resources}
        units={units}
        onChange={(i, f, v) => {
          const updated = [...documentResources];
          updated[i][f] = v;
          setDocumentResources(updated);
        }}
        onRemove={(i) => setDocumentResources(documentResources.filter((_, idx) => idx !== i))}
        onAddRow={() => setDocumentResources([...documentResources, { resourceId: '', unitId: '', count: '' }])}
        onSave={handleCreateDocument}
        onClose={() => setIsCreateModalOpen(false)}
      />

      <ShipmentModal
        title={`Редактировать документ #${editDocId}`}
        isOpen={editDocId !== null}
        clients={clients}
        selectedClientId={editClientId}
        onClientChange={setEditClientId}
        items={editDocResources}
        resources={resources}
        units={units}
        onChange={(i, f, v) => {
          const updated = [...editDocResources];
          updated[i][f] = v;
          setEditDocResources(updated);
        }}
        onRemove={(i) => setEditDocResources(editDocResources.filter((_, idx) => idx !== i))}
        onAddRow={() => setEditDocResources([...editDocResources, { resourceId: '', unitId: '', count: '' }])}
        onSave={handleUpdateDocument}
        onClose={() => setEditDocId(null)}
        onDelete={() => handleDeleteDocument(editDocId)}
        onSign={() => handleSignDocument(editDocId)}
        onWithdraw={() => handleWithdrawDocument(editDocId)}
        documentState={editDocumentState}
      />

      <table border="1" cellPadding="8">
        <thead>
          <tr>
            <th>Номер</th>
            <th>Дата</th>
            <th>Клиент</th>
            <th>Состояние</th>
            <th>Ресурс</th>
            <th>Единица</th>
            <th>Количество</th>
          </tr>
        </thead>
        <tbody>
          {documents.flatMap(doc =>
          (doc.shipmentResources?.length > 0
            ? doc.shipmentResources.map((res, idx) => (
              <tr key={doc.id + '-' + res.id} onDoubleClick={() => openEditModal(doc)}>
                {idx === 0 && (
                  <>
                    <td rowSpan={doc.shipmentResources.length}>{doc.number}</td>
                    <td rowSpan={doc.shipmentResources.length}>{new Date(doc.date).toLocaleDateString()}</td>
                    <td rowSpan={doc.shipmentResources.length}>{doc.client?.name}</td>
                    <td rowSpan={doc.shipmentResources.length}>{DocumentStateEnum.labels[doc.state]}</td>
                  </>
                )}
                <td>{res.resource?.name}</td>
                <td>{res.unit?.name}</td>
                <td>{res.count}</td>
              </tr>
            ))
            : [
              <tr key={doc.id} onDoubleClick={() => openEditModal(doc)}>
                <td>{doc.number}</td>
                <td>{new Date(doc.date).toLocaleDateString()}</td>
                <td>{doc.client?.name}</td>
                <td>{DocumentStateEnum.labels[doc.state]}</td>
                <td colSpan={3} style={{ textAlign: 'center', fontStyle: 'italic' }}>Нет ресурсов</td>
              </tr>
            ])
          )}
        </tbody>
      </table>

      <ToastContainer position="top-right" autoClose={3000} hideProgressBar />
    </div>
  );
};