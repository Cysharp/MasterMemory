using MasterMemory.Validation;
using System;
using System.Linq.Expressions;

namespace MasterMemory
{
    public interface IValidatable<TSelf>
    {
        void Validate(IValidator<TSelf> validator);
    }

    public interface IValidator<T>
    {
        ValidatableSet<T> GetTableSet();
        ReferenceSet<T, TRef> GetReferenceSet<TRef>();
        void Validate(Expression<Func<T, bool>> predicate);
        void Validate(Func<T, bool> predicate, string message);
        void ValidateAction(Expression<Func<bool>> predicate);
        void ValidateAction(Func<bool> predicate, string message);
        void Fail(string message);
        bool CallOnce();
    }
}