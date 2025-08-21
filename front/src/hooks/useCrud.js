import { useState } from 'react';
import axios from 'axios';
import { toast } from 'react-toastify';

export const useCrud = (baseUrl) => {
  const [items, setItems] = useState([]);

  const fetchItems = async (params = {}) => {
    try {
      const res = await axios.get(baseUrl, { params });
      setItems(res.data);
    } catch {
      toast.error('Ошибка загрузки данных');
    }
  };

  const addItem = async (data) => {
    try {
      const res = await axios.post(baseUrl, data);
      toast.success(res.data.message || 'Элемент добавлен');
      fetchItems();
    } catch (err) {
      toast.error(err.response?.data?.message || 'Ошибка добавления');
    }
  };

  const updateItem = async (data) => {
    try {
      const res = await axios.put(baseUrl, data);
      toast.success(res.data.message || 'Элемент обновлён');
      fetchItems();
    } catch (err) {
      toast.error(err.response?.data?.message || 'Ошибка обновления');
    }
  };

  const deleteItem = async (id) => {
    try {
      const res = await axios.delete(`${baseUrl}/${id}`);
      toast.success(res.data.message || 'Элемент удалён');
      fetchItems();
    } catch (err) {
      toast.error(err.response?.data?.message || 'Ошибка удаления');
    }
  };

  const archiveItem = async (data) => {
    try {
      const res = await axios.post(`${baseUrl}/archive`, data);
      toast.success(res.data.message || 'Элемент архивирован');
      fetchItems();
    } catch (err) {
      toast.error(err.response?.data?.message || 'Ошибка архивирования');
    }
  };

  return { items, fetchItems, addItem, updateItem, deleteItem, archiveItem };
};