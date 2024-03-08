using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WPF_MachineService.Models;

namespace WPF_MachineService.Repository
{
    public class UnitOfWork : IDisposable
    {
        private WpfMachineContext context = new WpfMachineContext();

        private GenericRepository<Category> categoryRepository;

        private GenericRepository<Order> orderRepository;
        private GenericRepository<OrderDetail> orderDetailRepositoty;

        private GenericRepository<Product> productRepository;


        public GenericRepository<Category> CategoryRepository
        {
            get
            {

                if (categoryRepository == null)
                {
                    categoryRepository = new GenericRepository<Category>(context);
                }
                return categoryRepository;
            }
        }

        public GenericRepository<Order> OrderRepository
        {
            get
            {

                if (orderRepository == null)
                {
                    orderRepository = new GenericRepository<Order>(context);
                }
                return orderRepository;
            }
        }
        public GenericRepository<OrderDetail> OrderDetailRepository
        {
            get
            {

                if (orderDetailRepositoty == null)
                {
                    orderDetailRepositoty = new GenericRepository<OrderDetail>(context);
                }
                return orderDetailRepositoty;
            }
        }
        public GenericRepository<Product> ProductRepository
        {
            get
            {

                if (productRepository == null)
                {
                    productRepository = new GenericRepository<Product>(context);
                }
                return productRepository;
            }
        }

        public void Save()
        {
            context.SaveChanges();
        }

        private bool disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    context.Dispose();
                }
            }
            disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
