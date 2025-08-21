import React, { useState, useEffect } from "react";
import axios from "axios";
import { toast, ToastContainer } from "react-toastify";
import "react-toastify/dist/ReactToastify.css";
import { ReceiptModal } from "./ReceiptModal";
import { DocumentFilters } from "./DocumentFilters";

export const Receipts = () => {
  const [documents, setDocuments] = useState([]);
  const [resources, setResources] = useState([]);
  const [units, setUnits] = useState([]);

  const [filters, setFilter] = useState({
    FromDate: null,
    ToDate: null,
    ResourceIds: [],
    UnitIds: [],
    DocumentNumbers: []
  });

  const [isCreateModalOpen, setIsCreateModalOpen] = useState(false);
  const [documentResources, setDocumentResources] = useState([]);
  const [editDocId, setEditDocId] = useState(null);
  const [editDocNumber, setEditDocNumber] = useState("");
  const [editDocResources, setEditDocResources] = useState([]);

  const fetchCatalogs = async () => {
    try {
      const [resourcesRes, unitsRes] = await Promise.all([
        axios.get("https://localhost:7024/api/resources"),
        axios.get("https://localhost:7024/api/units"),
      ]);
      setResources(resourcesRes.data.filter(r => r.state === 1));
      setUnits(unitsRes.data.filter(u => u.state === 1));
    } catch {
      toast.error("Ошибка загрузки справочников");
    }
  };

  const fetchReceipts = async () => {
    try {
      const response = await axios.get("https://localhost:7024/api/receipts", {
        params: filters,
        paramsSerializer: params => {
          return Object.entries(params)
            .map(([key, value]) =>
              Array.isArray(value)
                ? value.map(v => `${key}=${v}`).join("&")
                : value !== null ? `${key}=${value}` : ""
            )
            .filter(Boolean)
            .join("&");
        }
      });
      setDocuments(response.data);
    } catch {
      toast.error("Ошибка загрузки поступлений");
    }
  };

  useEffect(() => {
    fetchCatalogs();
  }, []);

  useEffect(() => {
    fetchReceipts();
  }, [filters]);

  const handleCreateDocument = async () => {
    try {
      await axios.post("https://localhost:7024/api/receipts", {
        resources: documentResources,
      });
      toast.success("Документ поступления создан");
      setIsCreateModalOpen(false);
      setDocumentResources([]);
      fetchReceipts();
    } catch (error) {
      toast.error(error.response?.data?.message || "Ошибка создания документа");
    }
  };

  const handleUpdateDocument = async () => {
    try {
      await axios.put("https://localhost:7024/api/receipts", {
        documentId: editDocId,
        resources: editDocResources,
      });
      toast.success("Документ обновлён");
      closeEditModal();
      fetchReceipts();
    } catch (error) {
      toast.error(error.response?.data?.message || "Ошибка обновления документа");
    }
  };

  const handleDeleteDocument = async (id) => {
    try {
      await axios.delete(`https://localhost:7024/api/receipts/${id}`);
      toast.success("Документ удалён");
      closeEditModal();
      fetchReceipts();
    } catch {
      toast.error("Ошибка удаления документа");
    }
  };

  const openCreateModal = () => {
    setDocumentResources([]);
    setIsCreateModalOpen(true);
  };

  const closeEditModal = () => {
    setEditDocId(null);
    setEditDocNumber("");
    setEditDocResources([]);
  };

  const openEditModal = (doc) => {
    setEditDocId(doc.id);
    setEditDocNumber(doc.number);
    setEditDocResources(
      (doc.receiptResources || []).map(r => ({
        id: r.id,
        resourceId: r.resource?.id,
        unitId: r.unit?.id,
        count: r.count,
      }))
    );
  };

  return (
    <div>
      <h2>Документы поступления</h2>

<DocumentFilters
  filtersConfig={[
    { name: "Ресурсы", options: resources, key: "ResourceIds" },
    { name: "Единицы", options: units, key: "UnitIds" },
    { name: "Номера документов", options: documents, key: "DocumentNumbers" }
  ]}
  onFilterChange={setFilter}
/>

      <ReceiptModal
        title="Создать документ поступления"
        isOpen={isCreateModalOpen}
        items={documentResources}
        resources={resources}
        units={units}
        onChange={(i, f, v) => {
          const updated = [...documentResources];
          updated[i][f] = v;
          setDocumentResources(updated);
        }}
        onRemove={(i) => setDocumentResources(documentResources.filter((_, idx) => idx !== i))}
        onAddRow={() => setDocumentResources([...documentResources, { resourceId: "", unitId: "", count: "" }])}
        onSave={handleCreateDocument}
        onClose={() => setIsCreateModalOpen(false)}
      />

      <ReceiptModal
        title={`Редактировать документ №${editDocNumber}`}
        isOpen={editDocId !== null}
        items={editDocResources}
        resources={resources}
        units={units}
        onChange={(i, f, v) => {
          const updated = [...editDocResources];
          updated[i][f] = v;
          setEditDocResources(updated);
        }}
        onRemove={(i) => setEditDocResources(editDocResources.filter((_, idx) => idx !== i))}
        onAddRow={() => setEditDocResources([...editDocResources, { resourceId: "", unitId: "", count: "" }])}
        onSave={handleUpdateDocument}
        onClose={closeEditModal}
        onDelete={() => handleDeleteDocument(editDocId)}
      />

      <button onClick={openCreateModal} style={{ marginBottom: 20 }}>
        Создать документ
      </button>

      <table border="1" cellPadding="8">
        <thead>
          <tr>
            <th>Номер</th>
            <th>Дата</th>
            <th>Ресурс</th>
            <th>Единица</th>
            <th>Количество</th>
          </tr>
        </thead>
        <tbody>
          {documents.flatMap(doc =>
            (doc.receiptResources?.length > 0
              ? doc.receiptResources.map((res, idx) => (
                  <tr key={doc.id + "-" + res.id} onDoubleClick={() => openEditModal(doc)}>
                    {idx === 0 && (
                      <>
                        <td rowSpan={doc.receiptResources.length}>{doc.number}</td>
                        <td rowSpan={doc.receiptResources.length}>
                          {new Date(doc.date).toLocaleDateString()}
                        </td>
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
                    <td colSpan={3} style={{ textAlign: "center", fontStyle: "italic" }}>
                      Нет ресурсов
                    </td>
                  </tr>,
                ])
          )}
        </tbody>
      </table>

      <ToastContainer position="top-right" autoClose={3000} hideProgressBar />
    </div>
  );
};