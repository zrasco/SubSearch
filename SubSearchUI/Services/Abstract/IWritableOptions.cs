using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace SubSearchUI.Services.Abstract
{
    public interface IWritableOptions<out T> : IOptions<T> where T : class, new()
    {
        void Update(Action<T> applyChanges);
    }
}
