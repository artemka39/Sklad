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
      throw new Error('Ошибка загрузки данных');
    }
  };

  const addItem = async (data) => {
    try {
      const res = await axios.post(baseUrl, data);
      toast.success(res.data.message || 'Элемент добавлен');
      await fetchItems();
      return res.data;
    } catch (err) {
      const msg = err.response?.data?.message || 'Ошибка добавления';
      toast.error(msg);
      throw new Error(msg);
    }
  };

  const updateItem = async (data) => {
    try {
      const res = await axios.put(baseUrl, data);
      toast.success(res.data.message || 'Элемент обновлён');
      await fetchItems();
      return res.data;
    } catch (err) {
      const msg = err.response?.data?.message || 'Ошибка обновления';
      toast.error(msg);
      throw new Error(msg);
    }
  };

  const deleteItem = async (id) => {
    try {
      const res = await axios.delete(`${baseUrl}/${id}`);
      toast.success(res.data.message || 'Элемент удалён');
      await fetchItems();
      return res.data;
    } catch (err) {
      const msg = err.response?.data?.message || 'Ошибка удаления';
      toast.error(msg);
      throw new Error(msg);
    }
  };

  const archiveItem = async (data) => {
    try {
      const res = await axios.post(`${baseUrl}/archive`, data);
      toast.success(res.data.message || 'Элемент архивирован');
      await fetchItems();
      return res.data;
    } catch (err) {
      const msg = err.response?.data?.message || 'Ошибка архивирования';
      toast.error(msg);
      throw new Error(msg);
    }
  };

  return { items, fetchItems, addItem, updateItem, deleteItem, archiveItem };
};