using Microsoft.AspNetCore.Mvc.ModelBinding;
using WebRetail.Models;

namespace WebRetail.ViewModels
{
    public class AverageCheckViewModel
    {
        public List<IRetailTable> Table { get; set; } = new List<IRetailTable>();

        [BindRequired]
        public int Method { get; set; } = 0;

        public DateOnly FirstDate { get; set; } = new DateOnly(2018, 1, 20);

        public DateOnly LastDate { get; set; } = new DateOnly(2022, 08, 20);

        public int TransactionsNumber { get; set; } = 100;

        [BindRequired]
        public decimal CoefAverCheckIncrease { get; set; } = 1.15M;

        [BindRequired]
        public int MaxChurnIndex { get; set; } = 3;

        [BindRequired]
        public decimal MaxShareDiscountTransactions { get; set; } = 70M;

        [BindRequired]
        public decimal MarginShare { get; set; } = 30M;

        [BindNever]
        public string EmptyResult { get; set; } = "";

        [BindNever]
        public string ModelName { get; set; } = "AverageCheckViewModel";
    }

}
