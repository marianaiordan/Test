using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS.Data
{
    using System.Data.Entity;

    public abstract class RepositoryBase<T> : IDisposable where T : DbContext, new()
    {
        private T dataContext;

        public T DataContext
        {
            get
            {
                if (this.dataContext == null)
                    this.dataContext = new T();

                return this.dataContext;
            }
            set
            {
                dataContext = value;
            }
        }

        public void Dispose()
        {
            if (this.dataContext != null)
                this.dataContext.Dispose();
        }
    }
}