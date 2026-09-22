using System;
using Autodesk.AutoCAD.DatabaseServices;
using RackCad.Application.Views.Redraw;

namespace RackCad.Plugin.Systems.Shared
{
    /// <summary>One already-prepared write. Product policy and planning remain outside this adapter.</summary>
    internal sealed class SiblingRedrawUnit
    {
        private readonly Func<Transaction, RackSiblingMutationResult> apply;
        private readonly Action post;

        internal SiblingRedrawUnit(
            string definitionKey,
            Func<Transaction, RackSiblingMutationResult> apply,
            Action post = null)
        {
            DefinitionKey = definitionKey;
            this.apply = apply ?? throw new ArgumentNullException(nameof(apply));
            this.post = post;
        }

        internal string DefinitionKey { get; }
        internal RackSiblingMutationResult Apply(Transaction transaction) => apply(transaction);
        internal void Post() => post?.Invoke();
    }
}
