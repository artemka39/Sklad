using Sklad.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sklad.Domain.Constants
{
    public static class OperationResultMessages
    {
        public const string GoodsReceiptDocumentNotFound = "Документ поступления не найден";
        public const string GoodsReceiptDocumentCreated = "Документ поступления успешно создан";
        public const string GoodsReceiptDocumentCreationFailed = "Ошибка создания документа поступления";
        public const string GoodsReceiptDocumentUpdated = "Документ поступления успешно обновлен";
        public const string GoodsReceiptDocumentUpdateFailed = "Ошибка обновления документа поступления";
        public const string GoodsReceiptDocumentDeleted = "Документ поступления успешно удален";
        public const string GoodsReceiptDocumentDeletionFailed = "Ошибка удаления документа поступления";

        public const string GoodsIssueDocumentNotFound = "Документ отгрузки не найден";
        public const string GoodsIssueDocumentCreated = "Документ отгрузки успешно создан";
        public const string GoodsIssueDocumentCreationFailed = "Ошибка создания документа отгрузки";
        public const string GoodsIssueDocumentUpdated = "Документ отгрузки успешно обновлен";
        public const string GoodsIssueDocumentUpdateFailed = "Ошибка обновления документа отгрузки";
        public const string GoodsIssueDocumentDeleted = "Документ отгрузки успешно удален";
        public const string GoodsIssueDocumentDeletionFailed = "Ошибка удаления документа отгрузки";
        public const string GoodsIssueDocumentSigned = "Документ отгрузки успешно подписан";
        public const string GoodsIssueDocumentAlreadySigned = "Документ отгрузки уже подписан";
        public const string GoodsIssueDocumentSigningFailed = "Ошибка подписания документа отгрузки";
        public const string GoodsIssueDocumentWithdrawn = "Документ отгрузки успешно отозван";
        public const string GoodsIssueDocumentAlreadyWithdrawn = "Документ отгрузки уже отозван";
        public const string GoodsIssueDocumentWithdrawalFailed = "Ошибка отзыва документа отгрузки";

        public const string ResourceCreated = "Ресурс успешно создан";
        public const string ResourceCreationFailed = "Ошибка создания ресурса";
        public const string ResourceUpdated = "Ресурс успешно обновлен";
        public const string ResourceUpdateFailed = "Ошибка обновления ресурса";
        public const string ResourceDeleted = "Ресурс успешно удален";
        public const string ResourceDeletionFailed = "Ошибка удаления ресурса";
        public const string ResourceNotFound = "Ресурс не найден";
        public const string ResourceAlreadyExists = "Ресурс с таким именем уже существует";
        public const string ResourceArchived = "Ресурс успешно архивирован";
        public const string ResourceArchiveFailed = "Ошибка архивирования ресурса";
        public const string ResourceAlreadyArchived = "Ресурс уже архивирован";
        public const string ResourceInUse = "Ресурс не может быть удален, так как он используется в документах";

        public const string UnitOfMeasurementCreated = "Единица измерения успешно создана";
        public const string UnitOfMeasurementCreationFailed = "Ошибка создания единицы измерения";
        public const string UnitOfMeasurementUpdated = "Единица измерения успешно обновлена";
        public const string UnitOfMeasurementUpdateFailed = "Ошибка обновления единицы измерения";
        public const string UnitOfMeasurementDeleted = "Единица измерения успешно удалена";
        public const string UnitOfMeasurementDeletionFailed = "Ошибка удаления единицы измерения";
        public const string UnitOfMeasurementNotFound = "Единица измерения не найдена";
        public const string UnitOfMeasurementAlreadyExists = "Единица измерения с таким именем уже существует";
        public const string UnitOfMeasurementArchived = "Единица измерения успешно архивирована";
        public const string UnitOfMeasurementArchiveFailed = "Ошибка архивирования единицы измерения";
        public const string UnitOfMeasurementAlreadyArchived = "Единица измерения уже архивирована";
        public const string UnitOfMeasurementInUse = "Единица измерения не может быть удалена, так как она используется в ресурсах";

        public const string ClientCreated = "Клиент успешно создан";
        public const string ClientCreationFailed = "Ошибка создания клиента";
        public const string ClientUpdated = "Клиент успешно обновлен";
        public const string ClientUpdateFailed = "Ошибка обновления клиента";
        public const string ClientDeleted = "Клиент успешно удален";
        public const string ClientDeletionFailed = "Ошибка удаления клиента";
        public const string ClientNotFound = "Клиент не найден";
        public const string ClientAlreadyExists = "Клиент с таким именем уже существует";
        public const string ClientArchived = "Клиент успешно архивирован";
        public const string ClientArchiveFailed = "Ошибка архивирования клиента";
        public const string ClientAlreadyArchived = "Клиент уже архивирован";
        public const string ClientInUse = "Клиент не может быть удален, так как он используется в документах";


        public const string NoResourcesProvided = "Не указаны ресурсы для поступления";

        public const string NotEnoughResource = "Недостаточно остатка для ресурсов";
    }
}
