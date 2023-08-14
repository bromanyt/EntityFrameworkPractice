using Microsoft.AspNetCore.Mvc.ModelBinding;
using WebRetail.Models;

namespace WebRetail.ViewModels
{
    public class CrossSellingViewModel
    {
        public List<IRetailTable> Table { get; set; } = new List<IRetailTable>();

        [BindRequired]
        public long GroupsNumber { get; set; } = 5;

        [BindRequired]
        public decimal MaxChurnIndex { get; set; } = 3M;

        [BindRequired]
        public decimal MaxConsumptionStabilityIndex { get; set; } = 0.5M;

        [BindRequired]
        public decimal MaxSkuShare { get; set; } = 100M;

        [BindRequired]
        public decimal MarginShare { get; set; } = 30M;

        [BindNever]
        public string EmptyResult { get; set; } = "";

        [BindNever]
        public string ModelName { get; set; } = "CrossSellingViewModel";
    }
}
