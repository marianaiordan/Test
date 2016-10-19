using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS.Data
{
    public static class ExtensionMethods
    {
        public static string GetDbEntityValidationExceptionMessage(this DbContext context, DbEntityValidationException ex)
        {
            var errorMessages = ex.EntityValidationErrors.SelectMany(x => x.ValidationErrors).Select(x => x.ErrorMessage);

            var fullErrorMessage = string.Join("; ", errorMessages);

            return string.Concat(ex.Message, " The validation errors are: ", fullErrorMessage);
        }
    }
}
