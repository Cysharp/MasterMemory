using System;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace MasterMemory.Validation
{
    internal class Validator<T> : IValidator<T>
    {
        readonly ValidationDatabase database;
        readonly T item;
        readonly ValidateResult resultSet;
        readonly StrongBox<bool> onceCalled;

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
            return new ValidatableSet<T>(database.GetTable<T>(), resultSet);
        }

        public ReferenceSet<T, TRef> GetReferenceSet<TRef>()
        {
            var table = database.GetTable<TRef>();
            return new ReferenceSet<T, TRef>(item, table, resultSet);
        }

        public void Validate(Expression<Func<T, bool>> predicate)
        {
            if (!predicate.Compile(true).Invoke(item))
            {
                var memberValues = ExpressionDumper<T>.DumpMemberValues(item, predicate);
                var message = string.Format($"{predicate.ToThisBodyString()}, {memberValues}");
                resultSet.AddFail(typeof(T), "Validate failed: " + message);
            }
        }

        public void Validate(Func<T, bool> predicate, string message)
        {
            if (!predicate(item))
            {
                resultSet.AddFail(typeof(T), "Validate failed: " + message);
            }
        }

        public void ValidateAction(Expression<Func<bool>> predicate)
        {
            if (!predicate.Compile(true).Invoke())
            {
                var expr = predicate.Body.ToString();
                resultSet.AddFail(typeof(T), "ValidateAction failed: " + expr);
            }
        }

        public void ValidateAction(Func<bool> predicate, string message)
        {
            if (!predicate())
            {
                resultSet.AddFail(typeof(T), "ValidateAction failed: " + message);
            }
        }

        public void Fail(string message)
        {
            resultSet.AddFail(typeof(T), message);
        }
    }
}