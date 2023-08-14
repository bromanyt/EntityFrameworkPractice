using Microsoft.AspNetCore.Mvc.ModelBinding;
using WebRetail.Models;

namespace WebRetail.ViewModels
{
    public class FrequencyVisitsViewModel
    {
        public List<IRetailTable> Table { get; set; } = new List<IRetailTable>();

        [BindRequired]
        public DateOnly FirstDate { get; set; } = new DateOnly(2018, 1, 20);

        [BindRequired]
        public DateOnly LastDate { get; set; } = new DateOnly(2022, 08, 20);

        [BindRequired]
        public long AddedTransactionsNumber { get; set; } = 1;

        [BindRequired]
        public decimal MaxChurnIndex { get; set; } = 3M;

        [BindRequired]
        public decimal MaxShareDiscountTransactions { get; set; } = 70M;

        [BindRequired]
        public decimal MarginShare { get; set; } = 30M;

        [BindNever]
        public string EmptyResult { get; set; } = "";

        [BindNever]
        public string ModelName { get; set; } = "FrequencyVisitsViewModel";
    }
}
