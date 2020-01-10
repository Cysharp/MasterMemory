using System;
using System.Linq;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace MasterMemory.Validation
{
    public class Validator<T> : IValidator<T>
    {
        readonly ValidationDatabase database;
        readonly T item;
        readonly ValidateResult resultSet;

        StrongBox<bool> onceCalled;
        

        public Validator(ValidationDatabase database, T item, ValidateResult resultSet, StrongBox<bool> onceCalled)
        {
            this.database = database;
            this.item = item;
            this.resultSet = resultSet;
            this.onceCalled = onceCalled;
        }

        public bool CallOnce()
        {
            if (!onceCalled.Value)
            {
                onceCalled.Value = true;
                return true;
            }

            return false;
        }

        public ValidatableSet<T> GetTableSet()
        {
            return new ValidatableSet<T>(database.GetTable<T>());
        }

        public ReferenceSet<T, TRef> GetReferenceSet<TRef>()
        {
            var table = database.GetTable<TRef>();
            return new ReferenceSet<T, TRef>(item, table);
        }

        public void Validate(Expression<Func<T, bool>> predicate)
        {
            var memberValues = ExpressionDumper<T>.DumpMemberValues(item, predicate);
            var message = string.Format($"{predicate.ToThisBodyString()}, {memberValues}");
            Validate(predicate.Compile(true), message);
        }

        public void Validate(Func<T, bool> predicate, string message)
        {
            if (!predicate(item))
            {
                resultSet.AddFail(typeof(T), "Validate failed: " + message);
            }
        }
    }
}
