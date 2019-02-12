using Nabla;
using Nabla.Conversion;
using Nabla.Linq;
using Nabla.Linq.Entity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            //WhereTest().Wait();
            //ConvertTest();
            //InterfaceTest();

            //AggregateTest().Wait();

            InterpolatedStringTest();
        }

        private static void InterpolatedStringTest()
        {
            InterpolatedString itstr = new InterpolatedString("{Name}, {{I}}'m {Age:N1} years old, birthday is {Dob:D}");

            var t = itstr.Interpolate(new { Name = "Effun", Age = 40, Dob = new DateTime(1967, 11, 13) });

            Console.WriteLine(t);

            Console.ReadLine();
        }

        private static async Task AggregateTest()
        {
            using (TestDbContext db = new TestDbContext())
            {
                var aggregation = new AggregationHelper<Contract, ContractViewModel>(db.Contracts);

                var query = await aggregation
                    .GroupBy(o => new { o.PartnerId, o.CompanyId })
                    .Sum(o => o.Final)
                    .Sum(o => o.Finished, o => o.Paid)
                    .Sum(o => o.Invoiced)
                    .Max(o => o.Final, o => o.MostExpensive)
                    .Min(o => o.Final, o => o.Cheapest)
                    .OrderByDescending(o => o.Final )
                    .Count(o => o.Count)
                    .Average(o => o.Final, o => o.Average)
                    .ToResultAsync(o => o.CompanyId + "-" + o.PartnerId);

                Console.WriteLine("Before lift:");
                PrintAggregationView(query);

                query.Lift(o => o.CompanyId, o => o.CompanyId + "小计").Lift("合计");

                //Console.WriteLine("Generated");
                //Console.WriteLine(query);

                Console.WriteLine("After lift:");

                PrintAggregationView(query);

                foreach (FlattenMode mode in Enum.GetValues(typeof(FlattenMode)))
                {
                    Console.WriteLine("Flatten " + mode);

                    PrintViewItems(query.Flatten(mode), false);
                }

                //query.Count();

                //var query1 = db.Contracts.GroupBy(o => new { o.PartnerId, o.CompanyId })
                //    .Select(o => new ContractViewModel
                //    {
                //        PartnerId = o.Key.PartnerId,
                //        CompanyId = o.Key.CompanyId,
                //        Final = o.Sum(i => i.Final),
                //        Paid = o.Sum(i => i.Finished),
                //        Invoiced = o.Sum(i => i.Invoiced),
                //        MostExpensive = o.Max(i => i.Final),
                //        Cheapest = o.Min(i => i.Final),
                //        Average = o.Average(i => i.Final),
                //        Count = o.Count()
                //    });

                //Console.WriteLine("Typed");
                ////Console.WriteLine(query1);
                //PrintAggretationResult(query1);

            }

            Console.ReadLine();
        }

        private static void PrintAggregationView(AggregationResult<ContractViewModel> view)
        {
            PrintViewItems(view.Items, true);
        }

        private static void PrintViewItems(IEnumerable<AggregationResultItem<ContractViewModel>> items, bool nested)
        {
            foreach (var item in items)
            {
                if (item.Depth > 0)
                    Console.Write(new string(' ', item.Depth));

                ContractViewModel model = item.Model;

                Console.WriteLine($"{item.Label?.PadRight(20)}{item.Level}\t{model.CompanyId?.PadRight(10)}\t{model.PartnerId?.PadRight(10)}\t{model.Count.ToString().PadRight(3)}\t{model.Final.ToString("0.00").PadRight(10)}");

                if (nested)
                    PrintViewItems(item.SubItems, nested);
            }
        }

        private static void PrintAggretationResult(IEnumerable<ContractViewModel> models)
        {
            foreach (var item in models)
            {
                Console.WriteLine($"{item.PartnerId}\t{item.CompanyId}\t{item.Count}\t{item.Final}\t{item.Paid}\t{item.Invoiced}\t{item.MostExpensive}\t{item.Cheapest}\t{item.Average}");
            }
        }

        private static async Task WhereTest()
        {
            TestDbContext data = new TestDbContext();
            WorkItemQueryArgs args = new WorkItemQueryArgs
            {
                //Date = new DateTimeRange(new DateTime(2018,3,10), new DateTime(2018,3, 18)),
                //Summary = "",
                Id = new int[] { 3, 8, 6, 10},
                //Sort = new Sort[]
                //{
                //    new Sort{ PropertyName = "Summary", SortType = SortType.Descending }
                //}
                //ContractId = Criteria.IsNull(false)
                PageSize = 2,
                PageIndex = 0,
                Sort = new Sort[] {new Sort ("Summary", SortType.Ascending) }
            };

            Nabla.Linq.QueryableExtensions.TraceSQL = true;

            var result = await data.WorkItems.PagedResultAsync<WorkItem, WorkItemViewModel>(args);

            int count = result.Items.Length;

            if (count > 0)
            {
                Console.WriteLine("Total {0} out of {1} items found.", count, result.Total);

                foreach (var item in result.Items)
                {
                    Console.WriteLine($"{item.Id}: {item.Date.ToShortDateString()} {item.Summary}, Contract={item.ContractName}");
                }
            }
            else
                Console.WriteLine("Nothing found.");

            data.Dispose();

            Console.ReadLine();
        }


        private static void InterfaceTest()
        {
            object test = new ArrayList();

            var coll = test.GetType().GetCollectionInfo();


        }

        private static void ConvertTest()
        {
            //Entity entity = new Entity {  Id = 1, Name = "abc" };

            //entity.Items = new EntityItem[]
            //{
            //    new EntityItem { Index = 1, ItemName = "Item1"},
            //    new EntityItem { Index = 2, ItemName = "Item2" }
            //};

            //Model model = new Model();

            //ModelConvertOptions options = new ModelConvertOptions
            //{
            //    IgnoreReadOnly = true
            //};

            //options
            //    .Property<Model>(o => o.Number, new PropertyConvertOptions { DefaultValue = 1 })
            //    .Property<Model>(o => o.Name1, new PropertyConvertOptions { Convert = (context) => context.Value = ((string)context.Value) + "xyz" })
            //    .Property<Entity>(o => o.Name, new PropertyConvertOptions { MapTo = nameof(Model.Name1) })
            //    .Property<Entity>(o => o.Items, new PropertyConvertOptions { Filter = (item, context) => ((EntityItem)item).Index >= 2 });
            
            //ModelConvert.Populate(model, entity, options);

            //Console.WriteLine("Name = {0}\r\nNumber = {1}", model.Name1, model.Number);
            //if (model.Items != null)
            //{
            //    Console.WriteLine("Items type = {0}", model.Items.GetType());

            //    int index = 0;

            //    foreach (var item in model.Items)
            //    {
            //        Console.WriteLine($"{index++}: Index = {item.Index}, ItemName = {item.ItemName}");
            //    }

            //}
            //else
            //    Console.WriteLine("Items is null");

            //Console.ReadLine();
        }
    }

    /*                    entity.Items.Add(item1 = new InvoiceItem
                    {
                        Index = index++,
                        Amount = item.Amount.GetValueOrDefault(),
                        Total = item.Total.GetValueOrDefault(),
                        Unit = item.Unit,
                        UnitPrice = item.UnitPrice.GetValueOrDefault(),
                        Content = item.Content,
                        Quantity = item.Quantity.GetValueOrDefault(),
                        Tax = item.Tax.GetValueOrDefault(),
                        TaxRate = item.TaxRate.GetValueOrDefault()
                    });

                    if (item.Quantity == null && item.UnitPrice == null)
                    {
                        item1.Quantity = 1;
                        item1.UnitPrice = item1.Amount;
                    }*/

    class Entity : ITest, Test1.ITest
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public double? Number { get; set; }

        public EntityItem[] Items { get; set; }
    }

    class EntityItem
    {
        public int Index { get; set; }

        public int EntityId { get; set; }

        public string ItemName { get; set; }
    }

    class Model
    {
        public Model()
        {
        }

        public int Id { get; set; }

        public string Name1 { get; set; }

        public int Number { get; set; }

        public EntityItem[] Items { get; set; }
    }

    class ModelItem
    {
        public int Index { get; set; }

        public string ItemName { get; set; }
    }

    interface ITest
    {
        int Id { get; }
    }


    class WorkItemQueryArgs : QueryArgs
    {
        public int[] Id { get; set; }

        //public string Summary { get; set; }

        public DateTimeRange Date { get; set; }

        [ModelMap(typeof(WorkItem), "Summary")]
        public string Type { get; set; }

        public IQueryable<WorkItem> DefaultSort(IQueryable<WorkItem> query)
        {
            return query.OrderByDescending(o => o.Id);
        }

        public Criteria ContractId { get; set; }

        //private Expression<Func<WorkItem, bool>> ResolveType(string value)
        //{
        //    return o => o.Type == value;
        //}

        //private Criteria ResolveType(string value)
        //{
        //    return Criteria.String(value, StringOperator.Equal);
        //}

        //private static IQueryable<WorkItem> ResolveType(IQueryable<WorkItem> source, string value)
        //{
        //    return source.Where(o => o.Type == value);
        //}

    }
}

namespace Test1
{
    interface ITest
    {
        string Name { get; }
    }
}

