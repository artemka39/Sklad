using Sklad.Domain.Enums;
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
        public static IReadOnlyDictionary<MessageKeyEnum, string> GetMessagesFor(Type type) =>
            Messages.TryGetValue(type, out var msgs)
                    ? msgs
                    : throw new ArgumentException($"Неизвестный тип: {type.Name}");

        private static readonly Dictionary<Type, Dictionary<WordFormKeyEnum, string>> WordForms =
            new()
            {
                [typeof(Resource)] = new()
                {
                    [WordFormKeyEnum.UpperNominative] = "Ресурс",
                    [WordFormKeyEnum.LowerGenitive] = "ресурса",
                    [WordFormKeyEnum.Suffix] = ""
                },
                [typeof(Client)] = new()
                {
                    [WordFormKeyEnum.UpperNominative] = "Клиент",
                    [WordFormKeyEnum.LowerGenitive] = "клиента",
                    [WordFormKeyEnum.Suffix] = ""
                },
                [typeof(Unit)] = new()
                {
                    [WordFormKeyEnum.UpperNominative] = "Единица измерения",
                    [WordFormKeyEnum.LowerGenitive] = "единицы измерения",
                    [WordFormKeyEnum.Suffix] = "а"
                },
                [typeof(ReceiptDocument)] = new()
                {
                    [WordFormKeyEnum.UpperNominative] = "Документ поступления",
                    [WordFormKeyEnum.LowerGenitive] = "документа поступления",
                    [WordFormKeyEnum.Suffix] = ""
                },
                [typeof(ShipmentDocument)] = new()
                {
                    [WordFormKeyEnum.UpperNominative] = "Документ отгрузки",
                    [WordFormKeyEnum.LowerGenitive] = "документа отгрузки",
                    [WordFormKeyEnum.Suffix] = ""
                }
            };


        private static readonly Dictionary<Type, Dictionary<MessageKeyEnum, string>> Messages =
            WordForms.ToDictionary(
                kv => kv.Key,
                kv =>
                {
                    var forms = kv.Value;
                    var suffix = forms[WordFormKeyEnum.Suffix];
                    return new Dictionary<MessageKeyEnum, string>
                    {
                        [MessageKeyEnum.Created] = $"{forms[WordFormKeyEnum.UpperNominative]} успешно создан{suffix}",
                        [MessageKeyEnum.NameRequired] = $"Имя {forms[WordFormKeyEnum.LowerGenitive]} обязательно для заполнения",
                        [MessageKeyEnum.AddressRequired] = $"Адрес {forms[WordFormKeyEnum.LowerGenitive]} обязателен для заполнения",
                        [MessageKeyEnum.CreationFailed] = $"Ошибка создания {forms[WordFormKeyEnum.LowerGenitive]}",
                        [MessageKeyEnum.Updated] = $"{forms[WordFormKeyEnum.UpperNominative]} успешно обновлен{suffix}",
                        [MessageKeyEnum.UpdateFailed] = $"Ошибка обновления {forms[WordFormKeyEnum.LowerGenitive]}",
                        [MessageKeyEnum.Deleted] = $"{forms[WordFormKeyEnum.UpperNominative]} успешно удален{suffix}",
                        [MessageKeyEnum.DeletionFailed] = $"Ошибка удаления {forms[WordFormKeyEnum.LowerGenitive]}",
                        [MessageKeyEnum.NotFound] = $"{forms[WordFormKeyEnum.UpperNominative]} не найден{suffix}",
                        [MessageKeyEnum.AlreadyExists] = $"{forms[WordFormKeyEnum.UpperNominative]} с таким именем уже существует",
                        [MessageKeyEnum.Archived] = $"{forms[WordFormKeyEnum.UpperNominative]} успешно архивирован{suffix}",
                        [MessageKeyEnum.ArchiveFailed] = $"Ошибка архивирования {forms[WordFormKeyEnum.LowerGenitive]}",
                        [MessageKeyEnum.AlreadyArchived] = $"{forms[WordFormKeyEnum.UpperNominative]} уже архивирован{suffix}",
                        [MessageKeyEnum.InUse] = $"{forms[WordFormKeyEnum.UpperNominative]} не может быть удален{suffix}, так как он{suffix} используется",
                        [MessageKeyEnum.NoResourcesProvided] = "Не указаны ресурсы для поступления",
                        [MessageKeyEnum.Signed] = $"{forms[WordFormKeyEnum.UpperNominative]} успешно подписан{suffix}",
                        [MessageKeyEnum.AlreadySigned] = $"{forms[WordFormKeyEnum.UpperNominative]} уже подписан{suffix}",
                        [MessageKeyEnum.SigningFailed] = $"Ошибка подписания {forms[WordFormKeyEnum.LowerGenitive]}",
                        [MessageKeyEnum.Withdrawn] = $"{forms[WordFormKeyEnum.UpperNominative]} успешно отозван{suffix}",
                        [MessageKeyEnum.AlreadyWithdrawn] = $"{forms[WordFormKeyEnum.UpperNominative]} уже отозван{suffix}",
                        [MessageKeyEnum.WithdrawalFailed] = $"Ошибка отзыва {forms[WordFormKeyEnum.LowerGenitive]}",
                        [MessageKeyEnum.NotEnoughResource] = $"Недостаточно ресурса на складе",
                    };
                });
    }
}
