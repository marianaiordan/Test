using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Transactions;

namespace RS.Data
{
    public class TransactionHelper
    {
        private Func<TransactionScope> _getCurrentScopeDelegate;

        bool IsInsideTransactionScope
        {
            get
            {
                if (_getCurrentScopeDelegate == null)
                    _getCurrentScopeDelegate = CreateGetCurrentScopeDelegate();

                TransactionScope ts = _getCurrentScopeDelegate();
                return ts != null;
            }
        }

        public static TransactionScope CreateTransactionScope(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            TransactionOptions options = new TransactionOptions();
            options.IsolationLevel = isolationLevel;
            options.Timeout = TransactionManager.MaximumTimeout;
            return new TransactionScope(TransactionScopeOption.Required, options);
        }

        private Func<TransactionScope> CreateGetCurrentScopeDelegate()
        {
            DynamicMethod getCurrentScopeDM = new DynamicMethod("GetCurrentScope", typeof(TransactionScope), null, this.GetType(), true);

            Type t = typeof(Transaction).Assembly.GetType("System.Transactions.ContextData");
            MethodInfo getCurrentContextDataMI = t.GetProperty("CurrentData", BindingFlags.NonPublic | BindingFlags.Static).GetGetMethod(true);

            FieldInfo currentScopeFI = t.GetField("CurrentScope", BindingFlags.NonPublic | BindingFlags.Instance);

            ILGenerator gen = getCurrentScopeDM.GetILGenerator();
            gen.Emit(OpCodes.Call, getCurrentContextDataMI);
            gen.Emit(OpCodes.Ldfld, currentScopeFI);
            gen.Emit(OpCodes.Ret);

            return (Func<TransactionScope>)getCurrentScopeDM.CreateDelegate(typeof(Func<TransactionScope>));
        }
    }
}