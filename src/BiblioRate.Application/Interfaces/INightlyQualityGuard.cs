using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BiblioRate.Application.Interfaces
{
    public interface INightlyQualityGuard
    {
      Task RunAsync(CancellationToken ct);   
    }
}