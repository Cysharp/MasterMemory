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
        readonly string pkName;
        readonly Delegate pkSelector;

        public Validator(ValidationDatabase database, T item, ValidateResult resultSet, StrongBox<bool> onceCalled, string pkName, Delegate pkSelector)
        {
            this.database = database;
            this.item = item;
            this.resultSet = resultSet;
            this.onceCalled = onceCalled;
            this.pkName = pkName;
            this.pkSelector = pkSelector;
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
            return new ValidatableSet<T>(database.GetTable<T>(), resultSet, pkName, pkSelector);
        }

        public ReferenceSet<T, TRef> GetReferenceSet<TRef>()
        {
            var table = database.GetTable<TRef>();
            return new ReferenceSet<T, TRef>(item, table, resultSet, pkName, pkSelector);
        }

        public void Validate(Expression<Func<T, bool>> predicate)
        {
            if (!predicate.Compile(true).Invoke(item))
            {
                var memberValues = ExpressionDumper<T>.DumpMemberValues(item, predicate);
                var message = string.Format($"{predicate.ToThisBodyString()}, {memberValues}, {BuildPkMessage()}");
                resultSet.AddFail(typeof(T), "Validate failed: " + message, item);
            }
        }

        public void Validate(Func<T, bool> predicate, string message)
        {
            if (!predicate(item))
            {
                resultSet.AddFail(typeof(T), "Validate failed: " + message + ", " + BuildPkMessage(), item);
            }
        }

        public void ValidateAction(Expression<Func<bool>> predicate)
        {
            if (!predicate.Compile(true).Invoke())
            {
                var expr = predicate.Body.ToString();
                resultSet.AddFail(typeof(T), "ValidateAction failed: " + expr + ", " + BuildPkMessage(), item);
            }
        }

        public void ValidateAction(Func<bool> predicate, string message)
        {
            if (!predicate())
            {
                resultSet.AddFail(typeof(T), "ValidateAction failed: " + message + ", " + BuildPkMessage(), item);
            }
        }

        public void Fail(string message)
        {
            resultSet.AddFail(typeof(T), message + ", " + BuildPkMessage(), item);
        }

        string BuildPkMessage()
        {
            var pk = pkSelector.DynamicInvoke(item).ToString();
            return $"PK({pkName}) = {pk}";
        }
    }
}