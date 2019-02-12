using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test
{
    class WorkItem
    {
        public int Id { get; set; }

        public DateTime Date { get; set; }

        public DateTime CreationTime { get; set; }

        public string Summary { get; set; }

        public string Type { get; set; }

        public Contract Contract { get; set; }

        public int? ContractId { get; set; }
    }

    class Contract
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Subject { get; set; }

        public string PartnerId { get; set; }

        public string CompanyId { get; set; }

        public decimal Finished { get; set; }

        public decimal Final { get; set; }

        public decimal Invoiced { get; set; }
    }

    class ContractViewModel
    {
        public string PartnerId { get; set; }

        public string CompanyId { get; set; }

        public int Count { get; set; }

        public decimal Paid { get; set; }

        public decimal Final { get; set; }

        public decimal Invoiced { get; set; }

        public decimal MostExpensive { get; set; }

        public decimal Cheapest { get; set; }

        public decimal Average { get; set; }

        public int Grade { get; set; }
    }

    class WorkItemViewModel
    {
        public int Id { get; set; }

        public DateTime Date { get; set; }

        //public DateTime CreationTime { get; set; }

        public string Summary { get; set; }

        public string Type { get; set; }

        public string ContractName { get; set; }
    }

    class TestDbContext : DbContext
    {
        public TestDbContext()
            : base("data source=.\\sqlexpress; initial catalog=WorkAssist; trusted_connection=true")
        {

        }

        public DbSet<WorkItem> WorkItems { get; set; }

        public DbSet<Contract> Contracts { get; set; }
    }
}
