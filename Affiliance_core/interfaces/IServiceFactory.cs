using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Affiliance_core.interfaces
{
    public interface IServiceFactory
    {
        T CreateService<T>() where T : notnull;
    }
}
