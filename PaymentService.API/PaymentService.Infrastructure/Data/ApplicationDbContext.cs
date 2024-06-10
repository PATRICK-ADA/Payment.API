using BidService.API.BidService.Domain.Entities;
using InvoiceService.API.InvoiceService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using PaymentService.API.PaymentService.Domain.Entities;




namespace RoomService.Infrastructure.Data
{
    public class AppDbContext : DbContext 
    {

      

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }

       
        public DbSet<BidModel> NotifyBidders { get; set; }
        public DbSet<InvoiceModel> Invoices { get; set; }
        public DbSet<PaymentResult> PaymentResults { get; set; }    


    }
}